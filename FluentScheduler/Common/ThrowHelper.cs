using System;
using System.Diagnostics.CodeAnalysis;

namespace FluentScheduler;

// helper methods for performing simple validations and that should throw an exception
internal static class ThrowHelper
{
    // throws if the given collection is empty
    internal static void ThrowIfEmpty(TimeSpan[] values, string paramName)
    {
        ThrowIfNull(values, paramName);

        if (values.Length == 0)
            throw new ArgumentException($"\"{paramName}\" cannot be empty.");
    }

    // equivalent of public static void ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other) which is not
    // available for .NET 4.8
    internal static void ThrowIfGreaterThan(int value, int other, string paramName)
    {
        if (value > other)
            throw new ArgumentOutOfRangeException(
                paramName, value, $"'{paramName}' must be less than or equal to '{other}'."
            );
    }

    // throws if the given hours and minutes are outside 00:00 to 23:59
    internal static void ThrowIfOutOfMilitaryTimeRange(
        int hour, int minute, string hourParamName, string minuteParamName)
    {
        ThrowIfNegative(hour, hourParamName);
        ThrowIfNegative(minute, minuteParamName);

        ThrowIfGreaterThan(hour, 23, hourParamName);
        ThrowIfGreaterThan(minute, 59, minuteParamName);
    }

    // throws if the hours and minutes component of the timespan are outside 00:00 to 23:59
    internal static void ThrowIfOutOfMilitaryTimeRange(TimeSpan value, string paramName)
    {
        ThrowIfNegative(value, paramName);

        ThrowIfGreaterThan(value.Hours, 23, $"{paramName}.Hours");
        ThrowIfGreaterThan(value.Minutes, 59, $"{paramName}.Minutes");
    }

    // a version of the existing helper that operates on an array instead
    internal static void ThrowIfOutOfMilitaryTimeRange(TimeSpan[] values, string paramName)
    {
        ThrowIfNull(values, paramName);

        Array.ForEach(values, v => ThrowIfOutOfMilitaryTimeRange(v, paramName));
    }

    // equivalent of ArgumentOutOfRangeException.ThrowIfNegative(value) which is not available for .NET 4.8
    internal static void ThrowIfNegative(int value, string paramName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must be a non-negative value.");
    }

    // throws if the given time is negative
    internal static void ThrowIfNegative(TimeSpan value, string paramName)
    {
        if (value < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(paramName, value, "The given time must be a non-negative value.");
    }

    // equivalent of ArgumentNullException.ThrowIfNull(value) which is not available for .NET 4.8
    internal static void ThrowIfNull(object value, string paramName)
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
    }

    // equivalent of ArgumentException.ThrowIfNullOrWhiteSpace(value) which is not available for .NET 4.8
    internal static void ThrowIfNullOrWhiteSpace(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("The value cannot be an empty or whitespace string.", paramName);
    }

    // throws if the given value is not present in the given enum T
    [SuppressMessage("Usage", "CA2263", Justification = "The generic overload is actually not available.")]
    internal static void ThrowIfNotDefinedInEnum<T>(T value, string paramName) where T : struct, Enum
    {
        if (!Enum.IsDefined(typeof(T), value))
            throw new ArgumentOutOfRangeException(paramName, value, "Enumeration value out of range.");
    }

    // a version of the existing helper that operates on an array instead
    internal static void ThrowIfNotDefinedInEnum<T>(T[] values, string paramName) where T : struct, Enum
    {
        ThrowIfNull(values, paramName);

        Array.ForEach(values, v => ThrowIfNotDefinedInEnum(v, paramName));
    }
}