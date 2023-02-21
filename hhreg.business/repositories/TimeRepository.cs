using hhreg.business;
using hhreg.business.domain;
using Microsoft.Data.Sqlite;

public interface ITimeRepository {
    DayEntry? GetDayEntry(string day);
    IEnumerable<DayEntry> GetDayEntries(string startDay, string endDay);
    IEnumerable<DayEntry> GetInvalidDayEntries();
    DayEntry GetOrCreateDay(string day, string? justification = null, DayType? dayType = DayType.Work);
    void CreateTime(long dayEntryId, string time);
    void CreateTime(long dayEntryId, params string[] timeList);
    void OverrideDayEntry(long dayEntryId, string? justification = null, DayType? dayType = DayType.Work, params string[] timeList);
    double GetAccumulatedBalance(double initialBalance, string limitDay);
}

public class TimeRepository : ITimeRepository
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly string _dayEntryByDayQuery = @"
        select * from DayEntry where Day = @day;";
    private readonly string _dayEntryByIdQuery = @"
        select * from DayEntry where Id = @dayEntryId;";
    private readonly string _dayEntriesBetweenQuery = @"
        select * from DayEntry where Day between @startDay and @endDay order by Day;";
    private readonly string _dayEntriesByIdsQuery = @"
        select * from DayEntry where Id in @ids order by Day;";
    private readonly string _timeEntryByDayEntryIdQuery = @"
        select * from TimeEntry where DayEntryId = @dayEntryId order by Time;";
    private readonly string _timeEntriesByDayEntryIdsQuery = @"
        select * from TimeEntry where DayEntryId in @dayEntryIds order by Time;";
    private readonly string _insertTimeEntryByDateEntryIdCommand = @"
        insert into TimeEntry (Time, DayEntryId) values (@time, @dayEntryId);";
    private readonly string _updateDayEntryTotalMinutesCommand = @"
        update DayEntry set TotalMinutes = @totalMinutes where Id = @dayEntryId";
    private readonly string _dayEntryIdsWithOddTimeEntriesQuery = @"
        select DayEntryId, count(*) as TimeEntriesCount 
        from TimeEntry 
        group by DayEntryId
        having TimeEntriesCount % 2 <> 0";

    public TimeRepository(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public DayEntry? GetDayEntry(string day)
    {
        var dayEntry = _unitOfWork.QuerySingleOrDefault<DayEntry>(_dayEntryByDayQuery, new { day });
        if (dayEntry == null) return null;

        var timeEntries = _unitOfWork.Query<TimeEntry>(_timeEntryByDayEntryIdQuery, new { dayEntryId = dayEntry.Id });
        dayEntry.TimeEntries = timeEntries;

        return dayEntry;
    }

    public IEnumerable<DayEntry> GetDayEntries(string startDay, string endDay)
    {
        var dayEntries = _unitOfWork.Query<DayEntry>(_dayEntriesBetweenQuery, new { startDay, endDay });
        var dayEntryIds = dayEntries.Select(x => x.Id);

        var timeEntries = _unitOfWork.Query<TimeEntry>(_timeEntriesByDayEntryIdsQuery, new { dayEntryIds = dayEntryIds });

        foreach(var dayEntry in dayEntries) 
        {
            dayEntry.TimeEntries = timeEntries.Where(x => x.DayEntryId == dayEntry.Id);
        }

        return dayEntries;
    }

    public IEnumerable<DayEntry> GetInvalidDayEntries()
    {
        var invalidDayEntryIds = _unitOfWork.Query<long>(_dayEntryIdsWithOddTimeEntriesQuery);
        var invalidDayEntries = _unitOfWork.Query<DayEntry>(_dayEntriesByIdsQuery, new { ids = invalidDayEntryIds });

        var timeEntries = _unitOfWork.Query<TimeEntry>(_timeEntriesByDayEntryIdsQuery, new { dayEntryIds = invalidDayEntryIds });

        foreach(var dayEntry in invalidDayEntries) 
        {
            dayEntry.TimeEntries = timeEntries.Where(x => x.DayEntryId == dayEntry.Id);
        }

        return invalidDayEntries;
    }

    public DayEntry GetOrCreateDay(string day, string? justification = null, DayType? dayType = DayType.Work)
    {
        var existingDay = GetDayEntry(day);
        
        if (existingDay == null) {
            _unitOfWork.Execute(
                    @"insert into DayEntry (Day, Justification, DayType) values (@day, @justification, @dayType);", new { day, justification, dayType });

            return GetDayEntry(day)!;
        }

        return existingDay;
    }

    public void CreateTime(long dayEntryId, string time)
    {
        var cmdList = new List<SqliteCommand>{
            _unitOfWork.CreateSqlCommand(
                _insertTimeEntryByDateEntryIdCommand,
                new Dictionary<string, object?> {{"@time",time},{"@dayEntryId",dayEntryId}})
        };

        var dayEntry = _unitOfWork.QuerySingle<DayEntry>(_dayEntryByIdQuery, new { dayEntryId = dayEntryId });
        var timeStrs = dayEntry.TimeEntries.Select(x => x.Time!).Append(time);
        var totalMinutes = CalculateTotalMinutes(timeStrs, dayEntry.DayType);

        cmdList.Add(
            _unitOfWork.CreateSqlCommand(
                _updateDayEntryTotalMinutesCommand,
                new Dictionary<string, object?> {{"@totalMinutes",totalMinutes},{"@dayEntryId",dayEntryId}}));

        _unitOfWork.BulkExecute(cmdList);
    }

    public void CreateTime(long dayEntryId, params string[] timeList)
    {
        var cmdList = timeList.Select(time => 
            _unitOfWork.CreateSqlCommand(
                _insertTimeEntryByDateEntryIdCommand,
                new Dictionary<string, object?> {{"@time",time},{"@dayEntryId",dayEntryId}}
            )
        );

        var dayEntry = _unitOfWork.QuerySingle<DayEntry>(_dayEntryByIdQuery, new { dayEntryId = dayEntryId });
        var timeStrs = dayEntry.TimeEntries.Select(x => x.Time!).Union(timeList);
        var totalMinutes = CalculateTotalMinutes(timeStrs, dayEntry.DayType);

        cmdList = cmdList.Append(
            _unitOfWork.CreateSqlCommand(
                _updateDayEntryTotalMinutesCommand,
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
                return _unitOfWork.CreateSqlCommand(_insertTimeEntryByDateEntryIdCommand,
                    new Dictionary<string, object?> {{"@time", time},{"@dayEntryId", dayEntryId}}
                );
            })
        );

        _unitOfWork.BulkExecute(cmdList);
    }

    public double GetAccumulatedBalance(double initialBalance, string limitDay)
    {
        var startDayQuery = "select StartCalculationsAt from Settings limit 1";
        var balanceQuery = "select total(TotalMinutes) from DayEntry where Day between @startDay and @limitDay";
        
        var startDay = _unitOfWork.QuerySingle<string>(startDayQuery);
        var balance = _unitOfWork.QuerySingle<double>(balanceQuery, new { startDay, limitDay });
        return initialBalance + balance;
    }

    private double CalculateTotalMinutes(IEnumerable<string> timeEntries, DayType dayType) {
        var summary = TimeSpan.Zero;

        if (dayType != DayType.Work) {
            var workDay = _unitOfWork.QuerySingle<int>("select coalesce(WorkDay, 0) from Settings limit 1");
            return workDay;
        }

        for(int i = 0, j = 1; j < (timeEntries.OrderBy(x => x).Count()); i=j+1, j=j+2) {
            var second = TimeSpan.Parse(timeEntries.ElementAt(j));
            var first = TimeSpan.Parse(timeEntries.ElementAt(i));
            var diff = second.Subtract(first);
            summary = summary.Add(diff);
        }

        return summary.TotalMinutes;
    }
}