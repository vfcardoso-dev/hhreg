using Hhreg.Business.Domain;

namespace Hhreg.Business.Utilities
{
    public static class Calculations
    {
        public static double GetTotalMinutes(IEnumerable<string> timeEntries, DayType dayType, double entryToleranceInMinutes, 
            double workdayInMinutes)
        {
            var summary = TimeSpan.Zero;
            var tolerance = TimeSpan.FromMinutes(entryToleranceInMinutes);
            var workday = TimeSpan.FromMinutes(workdayInMinutes);

            if (dayType != DayType.Work) return workdayInMinutes;

            for (int i = 0, j = 1; j < timeEntries.OrderBy(x => x).Count(); i = j + 1, j += 2)
            {
                var second = TimeSpan.Parse(timeEntries.ElementAt(j));
                var first = TimeSpan.Parse(timeEntries.ElementAt(i));
                
                summary = summary.Add(second.Subtract(first));
            }

            var summaryWithTolerance = summary <= (workday - tolerance) || summary >= (workday + tolerance)
                    ? summary
                    : workday;

            return summaryWithTolerance.TotalMinutes;
        }
    }
}
