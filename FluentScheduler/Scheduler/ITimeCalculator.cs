namespace FluentScheduler;

using System;

// computes the next job run of a schedule
internal interface ITimeCalculator
{
    // a mockable DateTime.Now
	Func<DateTime> Now { get; set; }

    // resets any stored state in the calculator
	void Reset();

    // returns the next run based on the given last run
	DateTime? Calculate(DateTime last);
}