using hhreg.business;
using hhreg.business.domain;

public interface ITimeRepository {
    DayEntry GetOrCreateDay(string day, string? justification = null);
    void CreateTime(long dayEntryId, string time);
    void CreateTime(long dayEntryId, params string[] timeList);
}

public class TimeRepository : ITimeRepository
{
    private readonly IUnitOfWork _unitOfWork;

    public TimeRepository(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public DayEntry GetOrCreateDay(string day, string? justification = null)
    {
        var query = @"select * from DayEntry where Day = @day;";
        var existingDay = _unitOfWork.QuerySingleOrDefault<DayEntry>(query, new { day });
        
        if (existingDay == null) {
            _unitOfWork.Execute(
                    @"insert into DayEntry (Day, Justification) values (@day, @justification);", new { day, justification });

            existingDay = _unitOfWork.QuerySingleOrDefault<DayEntry>(query, new { day });
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
}