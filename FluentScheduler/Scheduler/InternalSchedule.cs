namespace FluentScheduler;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

// the internal/private side of the Schedule class, in a separate dedicated class purely for readability
[SuppressMessage("Design", "CA1001", Justification = "The CancellationTokenSource is being disposed when stopping.")]
internal class InternalSchedule
{
    // the job given by the library user
    private readonly Func<CancellationToken, Task> _job;

    // the calculator for the next run, internal access to allow unit test mocking
    internal ITimeCalculator _calculator;

    // the task when a job is running, or null when not running
    private Task _task;

    // the source of cancellation token of a job execution, a new one is created on Start() and disposed on Stop()
    private CancellationTokenSource _tokenSource;

    // similar ctor to the public schedule class, the only different being taking an already instantiated calculator
    internal InternalSchedule(Func<CancellationToken, Task> job, ITimeCalculator calculator)
    {
        _job = job;
        SetScheduling(calculator);
    }

    // the computed next run date and time, it can be null due the library supporting scheduling for running only once
    internal DateTime? NextRun { get; private set; }

    // some operations should not be performed while the job is running
    internal object RunningLock { get; } = new object();

    // event handler provided by the library user, can be null
    internal event EventHandler<JobStartedEventArgs> JobStarted;

    // event handler provided by the library user, can be null
    internal event EventHandler<JobEndedEventArgs> JobEnded;

    // clears both next run and the calculator for the schedule, leaving an empty schedule with only the job set
    internal void ResetScheduling()
    {
        NextRun = null;
        _calculator.Reset();
    }

    // changes the scheduling of this schedule
    internal void SetScheduling(ITimeCalculator calculator)
    {
        NextRun = null;
        _calculator = calculator;
    }

    // shorthand for throwing an exception if the job is currently running
    internal void ShouldNotBeRunning()
    {
        if (Running())
            throw new InvalidOperationException("The scheduling cannot not be changed while the schedule is running.");
    }

    // returns true if the job is running
    internal bool Running()
    {
        Debug.Assert(
            (_task == null && _tokenSource == null) ||
            (_task != null && _tokenSource != null)
        );

        return _task != null;
    }

    // starts the schedule
    internal void Start()
    {
        if (Running())
            return;

        CalculateNextRun(_calculator.Now());

        _tokenSource = new CancellationTokenSource();
        _task = Run(_tokenSource.Token);
    }

    // stops the schedule in a non-blocking fashion or in a blocking one with an optional timeout
    internal void Stop(bool block, int? timeout = null)
    {
        if (timeout.HasValue)
            ArgumentOutOfRangeException.ThrowIfNegative(timeout.Value);

        if (!Running())
            return;

        _tokenSource.Cancel();

        try
        {
            if (block)
                _task.Wait(timeout ?? Timeout.Infinite);
        }
        finally
        {
            _tokenSource.Dispose();

            _task = null;
            _tokenSource = null;
        }
    }

    // use UTC time instead of the machine's time (UtcNow instead of Now)
    internal void UseUtc() => _calculator.Now = () => DateTime.UtcNow;

    // computes and sets the next run
    private void CalculateNextRun(DateTime last) => NextRun = _calculator.Calculate(last);

    // the main method of scheduling, runs user's job, raise events, computes next run, and sleeps
    [SuppressMessage("Design", "CA1031", Justification = "It's OK to catch a general exception here because it comes " +
        "from user code and not from the library itself.")]
    [SuppressMessage("Reliability", "CA2016", Justification = "The cancellation token is not being passed forward to " +
        "ContinueWith precisely to avoid a TaskCancelledException.")]
    private async Task Run(CancellationToken token)
    {
        // checking if it's supposed to run
        // it assumes that CalculateNextRun has been called previously from somewhere else
        if (!NextRun.HasValue)
            return;

        // calculating delay
        var delay = NextRun.Value - _calculator.Now();

        // delaying until it's time to run or a cancellation was requested
        // the empty ContinueWith() swallows TaskCanceledException
        // not locking the UI with ConfigureAwait(false)
        await Task.Delay(delay < TimeSpan.Zero ? TimeSpan.Zero : delay, token)
            .ContinueWith(_ => { })
            .ConfigureAwait(false);

        // checking if a cancellation was requested
        if (token.IsCancellationRequested)
            return;

        // used on both JobStarted and JobEnded events
        var startTime = _calculator.Now();

        // calculating the next run
        // used on both JobEnded event and for the next run of this method
        CalculateNextRun(startTime);

        // raising JobStarted event
        JobStarted?.Invoke(this, new JobStartedEventArgs(startTime));

        // used on JobEnded event
        Exception exception = null;

        try
        {
            // running the job
            // not locking the UI with ConfigureAwait(false)
            await _job(token).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // catching the exception if any
            exception = e;
        }

        // used on JobEnded event
        var endTime = _calculator.Now();

        // raising JobEnded event
        JobEnded?.Invoke(this, new JobEndedEventArgs(exception, startTime, endTime, NextRun));

        // if this schedule was stopped while the job was running
        if (token.IsCancellationRequested)
            return;

        // recursive call
        // note that the NextRun was already calculated in this run
        _task = Run(token);
    }
}
