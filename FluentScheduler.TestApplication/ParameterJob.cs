namespace FluentScheduler.TestApplication;

using static Serilog.Log;

internal class ParameterJob : IJob
{
    public string Parameter { get; set; }

    public void Execute() => Logger.Information($"Parameter: executed with parameter \"{Parameter}\"");
}
