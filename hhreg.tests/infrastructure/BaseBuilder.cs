using AutoFixture.Kernel;

namespace hhreg.tests;

public abstract class BaseBuilder<T> : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(T)) {
            return (object)DoCreate(context)!;
        }

        return new NoSpecimen();
    }

    public abstract T DoCreate(ISpecimenContext context);
}