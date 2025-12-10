using BudgetEase.Data;
using BudgetEase.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetEase.Services;

/// <inheritdoc />
public class CategoryService : ICategoryService
{
    private readonly BudgetEaseDbContext _dbContext;

    public CategoryService(BudgetEaseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Category>> GetForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(c => c.IsDefaultGlobal || c.UserId == userId)
            .OrderBy(c => c.UserId == null ? 0 : 1) // global defaults first
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category> CreateAsync(
        Guid userId,
        string name,
        string? colorHex,
        CancellationToken cancellationToken = default)
    {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.", nameof(name));
        }

        var exists = await _dbContext.Categories
            .AnyAsync(
                c => c.UserId == userId && c.Name.ToLower() == name.ToLower(),
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("You already have a category with this name.");
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            ColorHex = NormalizeColor(colorHex),
            IsDefaultGlobal = false
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return category;
    }

    public async Task RenameAsync(
        Guid userId,
        Guid categoryId,
        string newName,
        CancellationToken cancellationToken = default)
    {
        newName = (newName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Category name is required.", nameof(newName));
        }

        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

        if (category is null || category.UserId != userId || category.IsDefaultGlobal)
        {
            throw new InvalidOperationException("Category not found or cannot be modified.");
        }

        var exists = await _dbContext.Categories
            .AnyAsync(
                c => c.Id != categoryId &&
                     c.UserId == userId &&
                     c.Name.ToLower() == newName.ToLower(),
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("You already have another category with this name.");
        }

        category.Name = newName;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid userId,
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

        if (category is null || category.UserId != userId || category.IsDefaultGlobal)
        {
            // Either not found, belongs to another user, or is a global default.
            return;
        }

        if (category.Transactions.Any())
        {
            // For now, we simply prevent deletion when in use.
            // In the future we could support re-assigning transactions.
            throw new InvalidOperationException("This category has transactions and cannot be deleted.");
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string? NormalizeColor(string? colorHex)
    {
        if (string.IsNullOrWhiteSpace(colorHex))
        {
            return null;
        }

        colorHex = colorHex.Trim();

        if (!colorHex.StartsWith('#'))
        {
            colorHex = "#" + colorHex;
        }

        // keep it simple and trust further validation to the UI
        return colorHex.Length <= 16 ? colorHex : colorHex[..16];
    }
}


