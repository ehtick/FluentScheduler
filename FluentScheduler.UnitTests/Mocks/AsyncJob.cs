namespace FluentScheduler.UnitTests.Mocks;

using System.Threading.Tasks;

internal class AsyncJob : IAsyncJob
{
    public static int Calls { get; private set; }

    public async Task ExecuteAsync()
    {
        ++Calls;
        await Task.CompletedTask;
    }
}
