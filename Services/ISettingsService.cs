using BudgetEase.Models;

namespace BudgetEase.Services;

/// <summary>
/// Domain service to manage per-user settings such as currency, theme and timezone.
/// </summary>
public interface ISettingsService
{
    Task<UserSettings> GetOrCreateAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<UserSettings> UpdateAsync(
        Guid userId,
        string currency,
        string theme,
        string timeZone,
        CancellationToken cancellationToken = default);
}


