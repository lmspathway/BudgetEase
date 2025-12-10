using BudgetEase.Models;

namespace BudgetEase.Services;

/// <summary>
/// High-level service that aggregates data required to render the main dashboard.
/// </summary>
public interface IDashboardService
{
    Task<DashboardSummary> GetSummaryAsync(
        Guid userId,
        DateOnly referenceMonth,
        CancellationToken cancellationToken = default);
}


