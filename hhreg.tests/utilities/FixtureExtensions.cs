using AutoFixture;
using AutoFixture.Kernel;

namespace hhreg.tests;

public static class FixtureExtensions
{
    public static int CreateBetween(this ISpecimenBuilder builder, int start, int end)
    {
        return new Random().Next(start, end);
    }

    public static T CreateAnyBut<T>(this ISpecimenBuilder builder, T itemToExcept)
    {
        return builder.Create<Generator<T>>().Where(x => x!.Equals(itemToExcept) == false).First();
    }
}