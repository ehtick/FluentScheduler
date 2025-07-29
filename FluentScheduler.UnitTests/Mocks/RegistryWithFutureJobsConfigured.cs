namespace FluentScheduler.UnitTests.Mocks;

using System;

internal class RegistryWithFutureJobsConfigured : Registry
{
    public RegistryWithFutureJobsConfigured()
    {
        NonReentrantAsDefault();
        Schedule(() => Console.WriteLine("Hi"));
        Schedule<StronglyTypedTestJob>();
    }
}
