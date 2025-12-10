using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetEase.Tests.Services;

/// <summary>
/// Minimal assertion helpers so the example tests compile without bringing in the full xUnit package.
/// These are NOT a replacement for a real test framework, but enough for local sanity checks.
/// </summary>
internal static class Assert
{
    public static async Task ThrowsAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException)
        {
            return;
        }

        throw new Exception($"Expected exception of type {typeof(TException).Name}, but no exception was thrown.");
    }

    public static void NotEqual<T>(T notExpected, T actual)
    {
        if (Equals(notExpected, actual))
        {
            throw new Exception($"Expected value to differ from '{notExpected}', but both were equal.");
        }
    }

    public static void Equal<T>(T expected, T actual)
    {
        if (!Equals(expected, actual))
        {
            throw new Exception($"Expected '{expected}', got '{actual}'.");
        }
    }

    public static void NotNull(object? value)
    {
        if (value is null)
        {
            throw new Exception("Expected value to be non-null.");
        }
    }

    public static void Single<T>(IReadOnlyList<T> list)
    {
        if (list.Count != 1)
        {
            throw new Exception($"Expected a single element, but found {list.Count}.");
        }
    }

    public static T Single<T>(IEnumerable<T> source, Func<T, bool> predicate)
    {
        var matches = source.Where(predicate).ToList();
        if (matches.Count != 1)
        {
            throw new Exception($"Expected a single matching element, but found {matches.Count}.");
        }

        return matches[0];
    }
}


