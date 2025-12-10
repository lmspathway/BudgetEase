using BudgetEase.Data;
using BudgetEase.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetEase.Services;

/// <inheritdoc />
public class DashboardService : IDashboardService
{
    private readonly BudgetEaseDbContext _dbContext;

    public DashboardService(BudgetEaseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummary> GetSummaryAsync(
        Guid userId,
        DateOnly referenceMonth,
        CancellationToken cancellationToken = default)
    {
        var (currentStart, currentEnd) = GetMonthBounds(referenceMonth);
        var (previousStart, previousEnd) = GetMonthBounds(referenceMonth.AddMonths(-1));

        // Load current month transactions (with categories) once, for both totals and breakdown.
        var currentMonthTransactions = await _dbContext.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.UserId == userId &&
                        t.Date >= currentStart &&
                        t.Date < currentEnd)
            .ToListAsync(cancellationToken);

        var previousMonthTransactions = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId &&
                        t.Date >= previousStart &&
                        t.Date < previousEnd)
            .ToListAsync(cancellationToken);

        var currentIncome = currentMonthTransactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var currentExpense = currentMonthTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        var previousIncome = previousMonthTransactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var previousExpense = previousMonthTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        var currentNet = currentIncome - currentExpense;
        var previousNet = previousIncome - previousExpense;

        var incomeChange = CalculateChangePercent(previousIncome, currentIncome);
        var expenseChange = CalculateChangePercent(previousExpense, currentExpense);
        var netChange = CalculateChangePercent(previousNet, currentNet);

        var categoryBreakdown = BuildCategoryBreakdown(currentMonthTransactions);

        var recentTransactions = currentMonthTransactions
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new RecentTransactionItem
            {
                Id = t.Id,
                Date = t.Date,
                Amount = t.Amount,
                Type = t.Type,
                CategoryName = t.Category?.Name,
                Description = t.Description
            })
            .ToList();

        return new DashboardSummary
        {
            ReferenceMonth = referenceMonth,
            CurrentIncome = currentIncome,
            CurrentExpense = currentExpense,
            PreviousIncome = previousIncome,
            PreviousExpense = previousExpense,
            IncomeChangePercent = incomeChange,
            ExpenseChangePercent = expenseChange,
            NetChangePercent = netChange,
            CategoryBreakdown = categoryBreakdown,
            RecentTransactions = recentTransactions
        };
    }

    private static (DateTime Start, DateTime End) GetMonthBounds(DateOnly month)
    {
        var start = new DateTime(month.Year, month.Month, 1);
        var end = start.AddMonths(1);
        return (start, end);
    }

    private static decimal? CalculateChangePercent(decimal previous, decimal current)
    {
        if (previous == 0)
        {
            return null;
        }

        var delta = current - previous;
        return Math.Round(delta / previous * 100m, 1);
    }

    private static IReadOnlyList<CategoryBreakdownItem> BuildCategoryBreakdown(
        IReadOnlyCollection<Transaction> currentMonthTransactions)
    {
        var expenses = currentMonthTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .ToList();

        var totalExpense = expenses.Sum(t => t.Amount);
        if (totalExpense <= 0)
        {
            return Array.Empty<CategoryBreakdownItem>();
        }

        var grouped = expenses
            .GroupBy(t => new
            {
                t.CategoryId,
                Name = t.Category?.Name ?? "Uncategorized",
                Color = t.Category?.ColorHex
            })
            .Select(g =>
            {
                var total = g.Sum(x => x.Amount);
                var percentage = totalExpense == 0
                    ? 0
                    : Math.Round(total / totalExpense * 100m, 1);

                return new CategoryBreakdownItem
                {
                    CategoryId = g.Key.CategoryId ?? Guid.Empty,
                    CategoryName = g.Key.Name,
                    ColorHex = g.Key.Color,
                    TotalAmount = total,
                    PercentageOfTotal = percentage
                };
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToList();

        return grouped;
    }
}


