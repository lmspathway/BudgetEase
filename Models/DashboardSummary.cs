using System;
using System.Collections.Generic;

namespace BudgetEase.Models;

/// <summary>
/// Aggregated data used to render the main dashboard for a given month.
/// </summary>
public sealed class DashboardSummary
{
    public DateOnly ReferenceMonth { get; init; }

    // Current period totals
    public decimal CurrentIncome { get; init; }
    public decimal CurrentExpense { get; init; }
    public decimal CurrentNet => CurrentIncome - CurrentExpense;

    // Previous period totals (same length, previous month)
    public decimal PreviousIncome { get; init; }
    public decimal PreviousExpense { get; init; }
    public decimal PreviousNet => PreviousIncome - PreviousExpense;

    // Month-over-month deltas (in percent, null when previous is zero)
    public decimal? IncomeChangePercent { get; init; }
    public decimal? ExpenseChangePercent { get; init; }
    public decimal? NetChangePercent { get; init; }

    public IReadOnlyList<CategoryBreakdownItem> CategoryBreakdown { get; init; } =
        Array.Empty<CategoryBreakdownItem>();

    public IReadOnlyList<RecentTransactionItem> RecentTransactions { get; init; } =
        Array.Empty<RecentTransactionItem>();
}

/// <summary>
/// Represents how much was spent in a given category within the reference period.
/// </summary>
public sealed class CategoryBreakdownItem
{
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string? ColorHex { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal PercentageOfTotal { get; init; }
}

/// <summary>
/// Lightweight projection used to show the latest transactions on the dashboard.
/// </summary>
public sealed class RecentTransactionItem
{
    public Guid Id { get; init; }
    public DateTime Date { get; init; }
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public string? CategoryName { get; init; }
    public string? Description { get; init; }
}


