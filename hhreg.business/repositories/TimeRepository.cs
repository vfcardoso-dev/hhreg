using hhreg.business;
using hhreg.business.domain;
using Microsoft.Data.Sqlite;

public interface ITimeRepository {
    DayEntry? GetDayEntry(string day);
    DayEntry GetOrCreateDay(string day, string? justification = null, DayType? dayType = DayType.Work);
    void CreateTime(long dayEntryId, string time);
    void CreateTime(long dayEntryId, params string[] timeList);
    void OverrideDayEntry(long dayEntryId, string? justification = null, DayType? dayType = DayType.Work, 
        params string[] timeList);
}

public class TimeRepository : ITimeRepository
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _dayEntryQuery = "select * from DayEntry where Day = @day";
    private readonly string _timeEntryQuery = "select * from TimeEntry where DayEntryId = @dayEntryId order by Time";

    public TimeRepository(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public DayEntry? GetDayEntry(string day)
    {
        var dayEntry = _unitOfWork.QuerySingleOrDefault<DayEntry>(_dayEntryQuery, new { day });
        if (dayEntry == null) return null;

        var timeEntries = _unitOfWork.Query<TimeEntry>(_timeEntryQuery, new { dayEntryId = dayEntry.Id }).ToList();
        dayEntry.TimeEntries = timeEntries;

        return dayEntry;
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