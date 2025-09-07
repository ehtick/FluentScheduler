namespace FluentScheduler;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Operations that can be performed on multiple schedules at once.
/// </summary>
public static class ScheduleGroup
{
    /// <summary>
    /// Adds a listener for the event raised when the job starts.
    /// </summary>
    /// <param name="schedules">Schedules to operate on</param>
    /// <param name="handler">Event handler for the job start</param>
    public static void ListenJobStarted(
        this IEnumerable<Schedule> schedules, EventHandler<JobStartedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(schedules);
        ArgumentNullException.ThrowIfNull(handler);

        ForEach(
            [.. schedules], // forcing evaluation of a potential deferred execution
            false, // no need to parallelize the iterations
            i => i.JobStarted += handler
        );
    }

    /// <summary>
    /// Adds a listener for the event raised when the job ends.
    /// </summary>
    /// <param name="schedules">Schedules to operate on</param>
    /// <param name="handler">Event handler for the job end</param>
    public static void ListenJobEnded(
        this IEnumerable<Schedule> schedules, EventHandler<JobEndedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(schedules);
        ArgumentNullException.ThrowIfNull(handler);

        ForEach(
            [.. schedules], // forcing evaluation of a potential deferred execution
            false, // no need to parallelize the iterations
            i => i.JobEnded += handler
        );
    }

    /// <summary>
    /// Removes a listener for the event raised when the job starts.
    /// </summary>
    /// <param name="schedules">Schedules to operate on</param>
    /// <param name="handler">Event handler for the job start</param>
    public static void UnlistenJobStarted(
        this IEnumerable<Schedule> schedules, EventHandler<JobStartedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(schedules);
        ArgumentNullException.ThrowIfNull(handler);

        ForEach(
            [.. schedules], // forcing evaluation of a potential deferred execution
            false, // no need to parallelize the iterations
            i => i.JobStarted -= handler
        );
    }

    /// <summary>
    /// Removes a listener for the event raised when the job ends.
    /// </summary>
    /// <param name="schedules">Schedules to operate on</param>
    /// <param name="handler">Event handler for the job end</param>
    public static void UnlistenJobEnded(
        this IEnumerable<Schedule> schedules, EventHandler<JobEndedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(schedules);
        ArgumentNullException.ThrowIfNull(handler);

        ForEach(
            [.. schedules], // forcing evaluation of a potential deferred execution
            false, // no need to parallelize the iterations
            i => i.JobEnded -= handler
        );
    }


    /// <summary>
    /// Resets the scheduling of the schedules. You must not call this method if any of the schedules is running.
    /// </summary>
    /// <param name="schedules">Schedules to operate on</param>
    public static void ResetScheduling(this IEnumerable<Schedule> schedules)
    {
        ArgumentNullException.ThrowIfNull(schedules);

        ForEach(
            [.. schedules], // forcing evaluation of a potential deferred execution
            false, // no need to parallelize the iterations
            i => i.ShouldNotBeRunning(),
            i => i.ResetScheduling()
        );
    }

    /// <summary>
    /// Changes the scheduling of the schedules. You must not call this method if any of the schedules is running.
    /// </summary>
    /// <param name="schedules">Schedules to operate on</param>
    /// <param name="specifier">Scheduling of this schedule</param>
    public static void SetScheduling(this IEnumerable<Schedule> schedules, Action<RunSpecifier> specifier)
    {
        ArgumentNullException.ThrowIfNull(schedules);
        ArgumentNullException.ThrowIfNull(specifier);

        ForEach(
            [.. schedules], // forcing evaluation of a potential deferred execution
            false, // no need to parallelize the iterations
            i => i.ShouldNotBeRunning(),
            i => i.SetScheduling(new FluentTimeCalculator(specifier))
        );
    }

    /// <summary>
    /// Starts the schedules that are not already running.
    /// </summary>
    /// <param name="schedules">Schedules to operate on</param>
    public static void Start(this IEnumerable<Schedule> schedules)
    {
        ArgumentNullException.ThrowIfNull(schedules);

        ForEach(
            [.. schedules], // forcing evaluation of a potential deferred execution
            false, // no need to parallelize the iterations
            i => i.Start()
        );
    }

    /// <summary>
    /// Stops the schedules that are running. This call does not block.
    /// </summary>
    /// <param name="schedules">Schedules to operate on</param>
    public static void Stop(this IEnumerable<Schedule> schedules)
    {
        ArgumentNullException.ThrowIfNull(schedules);

        ForEach(
            [.. schedules], // forcing evaluation of a potential deferred execution
            false, // no need to parallelize the iterations
            i => i.Stop(false)
        );
    }

    /// <summary>
    /// Stops the schedules that are running. This call blocks (it waits for the running jobs to end its execution).
    /// </summary>
    /// <param name="schedules">Schedules to operate on</param>
    public static void StopAndBlock(this IEnumerable<Schedule> schedules)
    {
        ArgumentNullException.ThrowIfNull(schedules);

        ForEach(
            [.. schedules], // forcing evaluation of a potential deferred execution
            true, // running the iterations in parallel
            i => i.Stop(true)
        );
    }

    /// <summary>
    /// Stops the schedules that are running. This call blocks (it waits for the running job to end its execution).
    /// </summary>
    /// <param name="schedules">Schedules to operate on</param>
    /// <param name="timeout">Milliseconds to wait</param>
    public static void StopAndBlock(this IEnumerable<Schedule> schedules, int timeout)
    {
        ArgumentNullException.ThrowIfNull(schedules);
        ArgumentOutOfRangeException.ThrowIfNegative(timeout);

        ForEach(
            [.. schedules], // forcing evaluation of a potential deferred execution
            true, // running the iterations in parallel
            i => i.Stop(true, timeout)
        );
    }

    /// <summary>
    /// Stops the schedules that are running. This call blocks (it waits for the running job to end its execution).
    /// </summary>
    /// <param name="schedules">Schedules to operate on</param>
    /// <param name="timeout">Time to wait</param>
    public static void StopAndBlock(this IEnumerable<Schedule> schedules, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(schedules);
        ThrowHelper.ThrowIfNegative(timeout);

        ForEach(
            [.. schedules], // forcing evaluation of a potential deferred execution
            true, // running the iterations in parallel
            i => i.Stop(true, timeout.Milliseconds)
        );
    }

    /// <summary>
    /// Checks if all the schedules are currently running.
    /// </summary>
    /// <param name="schedules">Schedules to operate on</param>
    /// <returns>True if all of the schedules are running, false otherwise</returns>
    public static bool AllRunning(this IEnumerable<Schedule> schedules)
    {
        ArgumentNullException.ThrowIfNull(schedules);

        return Select(
            [.. schedules], // forcing evaluation of a potential deferred execution
            i => i.Running()
        ).All(r => r);
    }

    /// <summary>
    /// Checks if all the schedules are currently not running.
    /// </summary>
    /// <param name="schedules">Schedules to operate on</param>
    /// <returns>True if all of the schedules are stopped, false otherwise.</returns>
    public static bool AllStopped(this IEnumerable<Schedule> schedules)
    {
        ArgumentNullException.ThrowIfNull(schedules);

        return Select(
            [.. schedules], // forcing evaluation of a potential deferred execution
            i => i.Running()
        ).All(r => !r);
    }

    /// <summary>
    /// Checks if any of the schedules is currently running.
    /// </summary>
    /// <param name="schedules">Schedules to operate on</param>
    /// <returns>True if any of the schedules is running, false otherwise.</returns>
    public static bool AnyRunning(this IEnumerable<Schedule> schedules)
    {
        ArgumentNullException.ThrowIfNull(schedules);

        return Select(
            [.. schedules], // forcing evaluation of a potential deferred execution
            i => i.Running()
        ).Any(r => r);
    }

    /// <summary>
    /// Checks if any of the schedules is currently not running.
    /// </summary>
    /// <param name="schedules">Schedules to operate on</param>
    /// <returns>True if any of the schedules are stopped, false otherwise</returns>
    public static bool AnyStopped(this IEnumerable<Schedule> schedules)
    {
        ArgumentNullException.ThrowIfNull(schedules);

        return Select(
            [.. schedules], // forcing evaluation of a potential deferred execution
            i => i.Running()
        ).Any(r => !r);
    }

    /// <summary>
    /// Finds the schedule that is the next to run and the its expected run date and time.
    /// </summary>
    /// <param name="schedules">Schedules to operate on</param>
    /// <returns>The schedule and its next run date and time</returns>
    public static (Schedule, DateTime)? NextRun(this IEnumerable<Schedule> schedules)
    {
        ArgumentNullException.ThrowIfNull(schedules);

        // getting any potential deferred execution out of the way
        var _schedules = schedules.ToArray();

        // nothing to do if the collection is empty
        if (!_schedules.Any())
            return null;

        // the index of the earliest next run found and an array of schedules' next run times
        var earliest = 0;
        var times = Select(_schedules, i => i.NextRun).ToArray();

        // finding the index of the earliest next run
        for (var i = 0; i < times.Length; ++i)
        {
            if (times[i] < times[earliest])
                earliest = i;
        }

        // if there's no next run we return null
        if (!times[earliest].HasValue)
            return null;

        // a tuple of the schedule and next run time of the earliest found next run
        return (_schedules[earliest], times[earliest].Value);
    }

    // acquire all running locks, runs the given action on the given schedules (in parallel or not), then release the
    // acquired locks
    private static void ForEach(
        Schedule[] schedules, bool parallel, params Action<InternalSchedule>[] toRun)
    {
        var internals = Internal(schedules);

        EnterLock(internals);

        try
        {
            foreach (var _toRun in toRun)
            {
                if (parallel)
                {
                    Parallel.ForEach(internals, _toRun);
                }
                else
                {
                    foreach (var i in internals)
                        _toRun(i);
                }
            }
        }
        finally
        {
            ExitLock(internals);
        }
    }

    // acquires all running locks, runs LINQ's Select() on the given schedules, then release the acquired locks
    private static T[] Select<T>(Schedule[] schedules, Func<InternalSchedule, T> selector)
    {
        var internals = Internal(schedules);

        EnterLock(internals);

        try
        {
            return [.. internals.Select(selector)];
        }
        finally
        {
            ExitLock(internals);
        }
    }

    // a shorthand for getting the internal schedules behind the given schedules, purely for readability
    private static InternalSchedule[] Internal(Schedule[] schedules) =>
        [.. schedules.Select(s => s.Internal)];

    // synchronously acquires all running locks of all schedules
    private static void EnterLock(InternalSchedule[] internals)
    {
        foreach (var i in internals)
            Monitor.Enter(i.RunningLock);
    }

    // synchronously releases all the running locks of all schedules
    private static void ExitLock(InternalSchedule[] internals)
    {
        foreach (var i in internals)
            Monitor.Exit(i.RunningLock);
    }
}