using AutoFixture.Kernel;

namespace hhreg.tests.infrastructure;

public abstract class BaseBuilder<T> : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(T)) {
            return DoCreate(context)!;
        }

        return new NoSpecimen();
    }

    public abstract T DoCreate(ISpecimenContext context);
}