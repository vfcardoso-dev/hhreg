using Hhreg.Business.Domain;

namespace Hhreg.Business.Utilities
{
    public static class Calculations
    {
        public static double GetTotalMinutes(IEnumerable<string> timeEntries, DayType dayType, double entryToleranceInMinutes, double workdayInMinutes)
        {
            var summary = TimeSpan.Zero;
            var expectedHalfWorkday = TimeSpan.FromMinutes(workdayInMinutes / 2);
            var tolerance = TimeSpan.FromMinutes(entryToleranceInMinutes);

            if (dayType != DayType.Work) return workdayInMinutes;

            for (int i = 0, j = 1; j < timeEntries.OrderBy(x => x).Count(); i = j + 1, j += 2)
            {
                var second = TimeSpan.Parse(timeEntries.ElementAt(j));
                var first = TimeSpan.Parse(timeEntries.ElementAt(i));
                var diff = second.Subtract(first);
                var diffWithTolerance = diff < (expectedHalfWorkday - tolerance) || diff >= expectedHalfWorkday
                    ? diff 
                    : expectedHalfWorkday;

                summary = summary.Add(diffWithTolerance);
            }

            return summary.TotalMinutes;
        }
    }
}
