namespace FluentScheduler.TestApplication;

using Serilog;
using System;
using static Serilog.Log;
using static System.Threading.Thread;

internal static class Program
{
    private static void Main()
    {
        InitializeLogger();

        var schedules = new[] {
            Welcome(),

            NonReentrant(),
            Faulty(),

            FiveMinutes(),
            TenMinutes(),
            Hour(),
            Day(),
            Weekday(),
            Week(),

            Sunday(),
            Monday(),
            Tuesday(),
            Thursday(),
            Friday(),
            Saturday(),
        };

        schedules.ListenJobStarted(JobStartedHandler);
        schedules.ListenJobEnded(JobEndedHandler);

        schedules.Start();
        Sleep(-1);
    }

    private static Schedule Welcome() =>
        new(
            () => Logger.Information("3, 2, 1, live!"),
            run => run.Now()
        );

    private static Schedule NonReentrant() =>
        new(
            () =>
            {
                Logger.Information("NonReentrant: sleeping a minute");
                Sleep(TimeSpan.FromMinutes(1));
            },
            run => run.Every(1).Seconds()
        );

    private static void InitializeLogger()
    {
        var outputTemplate = "[{Timestamp:HH:mm:ss}] {Message}{NewLine}";

        Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(outputTemplate: outputTemplate)
            .WriteTo.File("logs/.txt", outputTemplate: outputTemplate, rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    private static Schedule Faulty() =>
        new(
            () => throw new InvalidOperationException("Exception from faulty job"),
            run => run.Now().AndEvery(20).Minutes()
        );

    private static Schedule FiveMinutes() =>
        new(
            () => Logger.Information("FiveMinutes: five minutes has passed"),
            run => run.OnceAt(DateTime.Now.AddMinutes(5)).AndEvery(5).Minutes()
        );

    private static Schedule TenMinutes() =>
        new(
            () => Logger.Information("TenMinutes: ten minutes has passed"),
            run => run.Every(10).Minutes()
        );

    private static Schedule Hour() =>
        new(
            () => Logger.Information("Hour: a hour has passed"),
            run => run.Every(1).Hours()
        );

    private static Schedule Day() =>
        new(
            () => Logger.Information("Day: a day has passed"),
            run => run.Every(1).Days()
        );

    private static Schedule Weekday() =>
        new(
            () => Logger.Information("Weekday: a new weekday has started"),
            run => run.EveryWeekday()
        );

    private static Schedule Sunday() =>
        new(
            () => Logger.Information("Sunday: a new sunday has started "),
            run => run.Every(DayOfWeek.Sunday)
        );

    private static Schedule Monday() =>
        new(
            () => Logger.Information("Monday: a new monday has started "),
            run => run.Every(DayOfWeek.Monday)
        );

    private static Schedule Tuesday() =>
        new(
            () => Logger.Information("Tuesday: a new tuesday has started "),
            run => run.Every(DayOfWeek.Tuesday)
        );

    private static Schedule Thursday() =>
        new(
            () => Logger.Information("Thursday: a new thursday has started "),
            run => run.Every(DayOfWeek.Thursday)
        );

    private static Schedule Friday() =>
        new(
            () => Logger.Information("Friday: a new friday has started "),
            run => run.Every(DayOfWeek.Friday)
        );

    private static Schedule Saturday() =>
        new(
            () => Logger.Information("Saturday: a new saturday has started "),
            run => run.Every(DayOfWeek.Saturday)
        );

    private static Schedule Week() =>
        new(
            () => Logger.Information("Week: a new week has started"),
            run => run.Every(1).Weeks()
        );

    private static void JobStartedHandler(object sender, JobStartedEventArgs ea) =>
        Logger.Information("JobStarted");

    private static void JobEndedHandler(object sender, JobEndedEventArgs ea) =>
        Logger.Information(ea.Exception is null ? $"JobEnded" : $"JobEnded: {ea.Exception}");
}
