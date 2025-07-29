namespace FluentScheduler.UnitTests.Mocks;

using System;

internal class StronglyTypedTestJob : IJob
{
    public void Execute() => Console.WriteLine("Hi");
}