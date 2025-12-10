using BudgetEase.Models;

namespace BudgetEase.Services;

/// <summary>
/// Domain service for managing categories (both global defaults and user-specific ones).
/// </summary>
public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Category> CreateAsync(
        Guid userId,
        string name,
        string? colorHex,
        CancellationToken cancellationToken = default);

    Task RenameAsync(
        Guid userId,
        Guid categoryId,
        string newName,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid userId,
        Guid categoryId,
        CancellationToken cancellationToken = default);
}


