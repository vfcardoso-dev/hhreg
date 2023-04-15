using hhreg.business.domain;
using hhreg.business.infrastructure;
using Microsoft.Data.Sqlite;

namespace hhreg.business.repositories;

public interface ITimeRepository {
    DayEntry? GetDayEntry(DateOnly day);
    IEnumerable<DayEntry> GetDayEntries(DateOnly startDay, DateOnly endDay);
    IEnumerable<DayEntry> GetDayEntriesByType(DateOnly startDay, DateOnly endDay, DayType dayType);
    IEnumerable<DayEntry> GetInvalidDayEntries();
    DayEntry GetOrCreateDay(DateOnly day, string? justification = null, DayType? dayType = DayType.Work);
    void CreateTime(long dayEntryId, string time);
    void CreateTime(long dayEntryId, params string[] timeList);
    void OverrideDayEntry(long dayEntryId, string? justification = null, DayType? dayType = DayType.Work, params string[] timeList);
    double GetAccumulatedBalance(Settings cfg, DateOnly limitDay);
}

public class TimeRepository : ITimeRepository
{
    private readonly IUnitOfWork _unitOfWork;

    private const string DayEntryByDayQuery = "select * from DayEntry where Day = @day;";
    private const string DayEntryByIdQuery = "select * from DayEntry where Id = @dayEntryId;";
    private const string DayEntriesByIdsQuery = "select * from DayEntry where Id in @ids order by Day;";
    private const string TimeEntryByDayEntryIdQuery = "select * from TimeEntry where DayEntryId = @dayEntryId order by Time;";
    private const string TimeEntriesByDayEntryIdsQuery = "select * from TimeEntry where DayEntryId in @dayEntryIds order by Time;";
    private const string InsertTimeEntryByDateEntryIdCommand = "insert into TimeEntry (Time, DayEntryId) values (@time, @dayEntryId);";
    private const string UpdateDayEntryTotalMinutesCommand = "update DayEntry set TotalMinutes = @totalMinutes where Id = @dayEntryId";
    private const string DayEntryIdsWithOddTimeEntriesQuery = @"
        select DayEntryId, count(*) as TimeEntriesCount 
        from TimeEntry 
        group by DayEntryId
        having TimeEntriesCount % 2 <> 0";

    public TimeRepository(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public DayEntry? GetDayEntry(DateOnly day)
    {
        var dayEntry = _unitOfWork.QuerySingleOrDefault<DayEntry>(DayEntryByDayQuery, new { day = day.ToString("yyyy-MM-dd") });
        if (dayEntry == null) return null;

        var timeEntries = _unitOfWork.Query<TimeEntry>(TimeEntryByDayEntryIdQuery, new { dayEntryId = dayEntry.Id });
        dayEntry.TimeEntries = timeEntries;

        return dayEntry;
    }

    public IEnumerable<DayEntry> GetDayEntries(DateOnly startDay, DateOnly endDay)
    {
        var query = "select * from DayEntry where Day between @startDay and @endDay order by Day;";
        var dayEntries = _unitOfWork.Query<DayEntry>(query, new
        {
            startDay = startDay.ToString("yyyy-MM-dd"), 
            endDay = endDay.ToString("yyyy-MM-dd")
        });
        return FillTimeEntries(dayEntries);
    }

    public IEnumerable<DayEntry> GetDayEntriesByType(DateOnly startDay, DateOnly endDay, DayType dayType)
    {
        var query = "select * from DayEntry where DayType = @dayType and Day between @startDay and @endDay order by Day;";
        var dayEntries = _unitOfWork.Query<DayEntry>(query, new
        {
            startDay = startDay.ToString("yyyy-MM-dd"), 
            endDay = endDay.ToString("yyyy-MM-dd"), 
            dayType
        });
        return FillTimeEntries(dayEntries);
    }

    public IEnumerable<DayEntry> GetInvalidDayEntries()
    {
        var invalidDayEntryIds = _unitOfWork.Query<long>(DayEntryIdsWithOddTimeEntriesQuery).ToList();
        var invalidDayEntries = _unitOfWork.Query<DayEntry>(DayEntriesByIdsQuery, new { ids = invalidDayEntryIds }).ToList();

        var timeEntries = _unitOfWork.Query<TimeEntry>(TimeEntriesByDayEntryIdsQuery, new { dayEntryIds = invalidDayEntryIds }).ToList();

        foreach(var dayEntry in invalidDayEntries) 
        {
            dayEntry.TimeEntries = timeEntries.Where(x => x.DayEntryId == dayEntry.Id);
        }

        return invalidDayEntries;
    }

    public DayEntry GetOrCreateDay(DateOnly day, string? justification = null, DayType? dayType = DayType.Work)
    {
        var existingDay = GetDayEntry(day);
        if (existingDay != null) return existingDay;
        
        _unitOfWork.Execute(
            @"insert into DayEntry (Day, Justification, DayType) values (@day, @justification, @dayType);", new
            {
                day = day.ToString("yyyy-MM-dd"),
                justification, 
                dayType
            });

        return GetDayEntry(day)!;

    }

    public void CreateTime(long dayEntryId, string time)
    {
        var cmdList = new List<SqliteCommand>{
            _unitOfWork.CreateSqlCommand(
                InsertTimeEntryByDateEntryIdCommand,
                new Dictionary<string, object?> {{"@time",time},{"@dayEntryId",dayEntryId}})
        };

        var dayEntry = _unitOfWork.QuerySingle<DayEntry>(DayEntryByIdQuery, new { dayEntryId });
        var timeStrs = dayEntry.TimeEntries.Select(x => x.Time!).Append(time);
        var totalMinutes = CalculateTotalMinutes(timeStrs, dayEntry.DayType);

        cmdList.Add(
            _unitOfWork.CreateSqlCommand(
                UpdateDayEntryTotalMinutesCommand,
                new Dictionary<string, object?> {{"@totalMinutes",totalMinutes},{"@dayEntryId",dayEntryId}}));

        _unitOfWork.BulkExecute(cmdList);
    }

    public void CreateTime(long dayEntryId, params string[] timeList)
    {
        var cmdList = timeList.Select(time => 
            _unitOfWork.CreateSqlCommand(
                InsertTimeEntryByDateEntryIdCommand,
                new Dictionary<string, object?> {{"@time",time},{"@dayEntryId",dayEntryId}}
            )
        );

        var dayEntry = _unitOfWork.QuerySingle<DayEntry>(DayEntryByIdQuery, new { dayEntryId });
        var timeStrs = dayEntry.TimeEntries.Select(x => x.Time!).Union(timeList);
        var totalMinutes = CalculateTotalMinutes(timeStrs, dayEntry.DayType);

        cmdList = cmdList.Append(
            _unitOfWork.CreateSqlCommand(
                UpdateDayEntryTotalMinutesCommand,
                new Dictionary<string, object?> {{"@totalMinutes",totalMinutes},{"@dayEntryId",dayEntryId}}));

        _unitOfWork.BulkExecute(cmdList);
    }

    public void OverrideDayEntry(long dayEntryId, string? justification = null, DayType? dayType = DayType.Work, params string[] timeList) 
    {
        var totalMinutes = CalculateTotalMinutes(timeList, dayType!.Value);

        var cmdList = new List<SqliteCommand>{
            _unitOfWork.CreateSqlCommand(@"update DayEntry set Justification = @justification, DayType = @dayType, TotalMinutes = @totalMinutes where Id = @dayEntryId;",
                new Dictionary<string, object?> {
                    {"@justification", justification},
                    {"@dayType", dayType}, 
                    {"@dayEntryId", dayEntryId},
                    {"@totalMinutes", totalMinutes}
                }
            ),
            _unitOfWork.CreateSqlCommand(@"delete from TimeEntry where DayEntryId = @dayEntryId;",
                new Dictionary<string, object?> {{"@dayEntryId", dayEntryId}}
            )
        };

        cmdList.AddRange(
            timeList.Select(time => {
                return _unitOfWork.CreateSqlCommand(InsertTimeEntryByDateEntryIdCommand,
                    new Dictionary<string, object?> {{"@time", time},{"@dayEntryId", dayEntryId}}
                );
            })
        );

        _unitOfWork.BulkExecute(cmdList);
    }

    public double GetAccumulatedBalance(Settings cfg, DateOnly limitDay)
    {
        var startDayQuery = "select StartCalculationsAt from Settings limit 1";
        var balanceQuery = "select total(TotalMinutes - @workDay) from DayEntry where Day between @startDay and @limitDay";
        
        var startDay = _unitOfWork.QuerySingle<string>(startDayQuery);
        var balance = _unitOfWork.QuerySingle<double>(balanceQuery, new { startDay, limitDay = limitDay.ToString("yyyy-MM-dd"), workDay = cfg.WorkDay });
        return cfg.InitialBalance + balance;
    }

    private double CalculateTotalMinutes(IEnumerable<string> timeEntries, DayType dayType) 
    {
        var summary = TimeSpan.Zero;

        if (dayType != DayType.Work) {
            var workDay = _unitOfWork.QuerySingle<int>("select coalesce(WorkDay, 0) from Settings limit 1");
            return workDay;
        }

        for(int i = 0, j = 1; j < (timeEntries.OrderBy(x => x).Count()); i=j+1, j+=2) {
            var second = TimeSpan.Parse(timeEntries.ElementAt(j));
            var first = TimeSpan.Parse(timeEntries.ElementAt(i));
            var diff = second.Subtract(first);
            summary = summary.Add(diff);
        }

        return summary.TotalMinutes;
    }

    private IEnumerable<DayEntry> FillTimeEntries(IEnumerable<DayEntry> dayEntries)
    {
        var dayEntryIds = dayEntries.Select(x => x.Id);

        var timeEntries = _unitOfWork.Query<TimeEntry>(TimeEntriesByDayEntryIdsQuery, new { dayEntryIds });

        foreach(var dayEntry in dayEntries) 
        {
            dayEntry.TimeEntries = timeEntries.Where(x => x.DayEntryId == dayEntry.Id);
        }

        return dayEntries;
    }
}