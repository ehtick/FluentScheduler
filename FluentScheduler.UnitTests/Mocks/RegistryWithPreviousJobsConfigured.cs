namespace FluentScheduler.UnitTests.Mocks;

using System;

internal class RegistryWithPreviousJobsConfigured : Registry
{
    public RegistryWithPreviousJobsConfigured()
    {
        Schedule(() => Console.WriteLine("Hi"));
        Schedule<StronglyTypedTestJob>();
        NonReentrantAsDefault();
    }
}
