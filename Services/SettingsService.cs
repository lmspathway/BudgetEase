using BudgetEase.Data;
using BudgetEase.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetEase.Services;

/// <inheritdoc />
public class SettingsService : ISettingsService
{
    private static readonly string[] AllowedThemes = ["light", "dark", "system"];

    private readonly BudgetEaseDbContext _dbContext;

    public SettingsService(BudgetEaseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserSettings> GetOrCreateAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var settings = await _dbContext.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (settings is not null)
        {
            return settings;
        }

        settings = new UserSettings
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Currency = "USD",
            Theme = "system",
            TimeZone = "UTC"
        };

        _dbContext.UserSettings.Add(settings);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return settings;
    }

    public async Task<UserSettings> UpdateAsync(
        Guid userId,
        string currency,
        string theme,
        string timeZone,
        CancellationToken cancellationToken = default)
    {
        currency = NormalizeCurrency(currency);
        theme = NormalizeTheme(theme);
        timeZone = NormalizeTimeZone(timeZone);

        var settings = await GetOrCreateAsync(userId, cancellationToken);

        settings.Currency = currency;
        settings.Theme = theme;
        settings.TimeZone = timeZone;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return settings;
    }

    private static string NormalizeCurrency(string currency)
    {
        currency = (currency ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(currency) || currency.Length > 16)
        {
            return "USD";
        }

        return currency;
    }

    private static string NormalizeTheme(string theme)
    {
        theme = (theme ?? string.Empty).Trim().ToLowerInvariant();
        if (!AllowedThemes.Contains(theme))
        {
            return "system";
        }

        return theme;
    }

    private static string NormalizeTimeZone(string timeZone)
    {
        timeZone = (timeZone ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(timeZone) || timeZone.Length > 128)
        {
            return "UTC";
        }

        return timeZone;
    }
}


