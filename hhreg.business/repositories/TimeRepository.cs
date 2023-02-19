using hhreg.business;
using hhreg.business.domain;
using Microsoft.Data.Sqlite;

public interface ITimeRepository {
    DayEntry? GetDayEntry(string day);
    IList<DayEntry> GetDayEntries(string startDay, string endDay);
    IList<DayEntry> GetInvalidDayEntries();
    DayEntry GetOrCreateDay(string day, string? justification = null, DayType? dayType = DayType.Work);
    void CreateTime(long dayEntryId, string time);
    void CreateTime(long dayEntryId, params string[] timeList);
    void OverrideDayEntry(long dayEntryId, string? justification = null, DayType? dayType = DayType.Work, 
        params string[] timeList);
}

public class TimeRepository : ITimeRepository
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly string _dayEntryByIdQuery = @"
        select * from DayEntry where Day = @day;";
    private readonly string _dayEntriesBetweenQuery = @"
        select * from DayEntry where Day between @startDay and @endDay order by Day;";
    private readonly string _dayEntriesByIdsQuery = @"
        select * from DayEntry where Id in @ids order by Day;";
    private readonly string _timeEntryByDayEntryIdQuery = @"
        select * from TimeEntry where DayEntryId = @dayEntryId order by Time;";
    private readonly string _timeEntriesByDayEntryIdsQuery = @"
        select * from TimeEntry where DayEntryId in @dayEntryIds order by Time;";
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
        var dayEntry = _unitOfWork.QuerySingleOrDefault<DayEntry>(_dayEntryByIdQuery, new { day });
        if (dayEntry == null) return null;

        var timeEntries = _unitOfWork.Query<TimeEntry>(_timeEntryByDayEntryIdQuery, new { dayEntryId = dayEntry.Id }).ToList();
        dayEntry.TimeEntries = timeEntries;

        return dayEntry;
    }

    public IList<DayEntry> GetDayEntries(string startDay, string endDay)
    {
        var dayEntries = _unitOfWork.Query<DayEntry>(_dayEntriesBetweenQuery, new { startDay, endDay }).ToList();
        var dayEntryIds = dayEntries.Select(x => x.Id).ToArray();

        var timeEntries = _unitOfWork.Query<TimeEntry>(_timeEntriesByDayEntryIdsQuery, new { dayEntryIds = dayEntryIds }).ToList();

        foreach(var dayEntry in dayEntries) 
        {
            dayEntry.TimeEntries = timeEntries.Where(x => x.DayEntryId == dayEntry.Id).ToList();
        }

        return dayEntries;
    }

    public IList<DayEntry> GetInvalidDayEntries()
    {
        var invalidDayEntryIds = _unitOfWork.Query<long>(_dayEntryIdsWithOddTimeEntriesQuery).ToList();
        var invalidDayEntries = _unitOfWork.Query<DayEntry>(_dayEntriesByIdsQuery, new { ids = invalidDayEntryIds }).ToList();

        var timeEntries = _unitOfWork.Query<TimeEntry>(_timeEntriesByDayEntryIdsQuery, new { dayEntryIds = invalidDayEntryIds }).ToList();

        foreach(var dayEntry in invalidDayEntries) 
        {
            dayEntry.TimeEntries = timeEntries.Where(x => x.DayEntryId == dayEntry.Id).ToList();
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
        var query = @"insert into TimeEntry (Time, DayEntryId) values (@time, @dayEntryId);";
        var parameters = new { time = time, dayEntryId = dayEntryId };

        _unitOfWork.Execute(query, parameters);
    }

    public void CreateTime(long dayEntryId, params string[] timeList)
    {
        var query = @"insert into TimeEntry (Time, DayEntryId) values (@time, @dayEntryId);";
        var parameters = timeList.Select(time => new { time, dayEntryId }).ToList<object>();

        _unitOfWork.BulkExecute(query, parameters);
    }

    public void OverrideDayEntry(long dayEntryId, string? justification = null, DayType? dayType = DayType.Work, params string[] timeList) 
    {
        var cmdList = new List<SqliteCommand>{
            _unitOfWork.CreateSqlCommand(@"update DayEntry set Justification = @justification, DayType = @dayType where Id = @dayEntryId;",
                new Dictionary<string, object?> {
                    {"@justification", justification}, {"@dayType", dayType}, {"@dayEntryId", dayEntryId}
                }
            ),
            _unitOfWork.CreateSqlCommand(@"delete from TimeEntry where DayEntryId = @dayEntryId;",
                new Dictionary<string, object?> {{"@dayEntryId", dayEntryId}}
            )
        };

        cmdList.AddRange(
            timeList.Select(time => {
                return _unitOfWork.CreateSqlCommand(@"insert into TimeEntry (Time, DayEntryId) values (@time, @dayEntryId);",
                    new Dictionary<string, object?> {{"@time", time},{"@dayEntryId", dayEntryId}}
                );
            })
        );

        _unitOfWork.BulkExecute(cmdList);
    }
}