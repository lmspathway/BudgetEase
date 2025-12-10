using BudgetEase.Data;
using BudgetEase.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetEase.Services;

/// <inheritdoc />
public class TransactionService : ITransactionService
{
    private readonly BudgetEaseDbContext _dbContext;

    public TransactionService(BudgetEaseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Transaction> CreateAsync(
        Guid userId,
        decimal amount,
        DateTime date,
        TransactionType type,
        Guid? categoryId,
        string? description,
        CancellationToken cancellationToken = default)
    {
        ValidateAmount(amount);
        ValidateDate(date);

        Category? category = null;
        if (categoryId is not null)
        {
            category = await _dbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

            if (category is null ||
                (category.UserId is not null && category.UserId != userId && !category.IsDefaultGlobal))
            {
                throw new InvalidOperationException("The selected category is not available for this user.");
            }
        }

        var now = DateTime.UtcNow;

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = decimal.Round(amount, 2),
            Date = date,
            Type = type,
            CategoryId = category?.Id,
            Description = string.IsNullOrWhiteSpace(description)
                ? null
                : description!.Length > 512 ? description[..512] : description,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return transaction;
    }

    public async Task<Transaction?> GetByIdAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> GetRecentAsync(
        Guid userId,
        int count = 5,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            count = 5;
        }

        return await _dbContext.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> GetByMonthAsync(
        Guid userId,
        DateOnly month,
        CancellationToken cancellationToken = default)
    {
        var (start, end) = GetMonthBounds(month);

        return await _dbContext.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Date >= start && t.Date < end)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> SearchAsync(
        Guid userId,
        DateTime? from,
        DateTime? to,
        Guid? categoryId,
        TransactionType? type,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (from is not null)
        {
            query = query.Where(t => t.Date >= from.Value.Date);
        }

        if (to is not null)
        {
            var inclusiveEnd = to.Value.Date.AddDays(1);
            query = query.Where(t => t.Date < inclusiveEnd);
        }

        if (categoryId is not null)
        {
            query = query.Where(t => t.CategoryId == categoryId);
        }

        if (type is not null)
        {
            query = query.Where(t => t.Type == type);
        }

        return await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Guid userId,
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        if (transaction.UserId != userId)
        {
            throw new InvalidOperationException("You can only update your own transactions.");
        }

        ValidateAmount(transaction.Amount);
        ValidateDate(transaction.Date);

        var existing = await _dbContext.Transactions
            .FirstOrDefaultAsync(t => t.Id == transaction.Id && t.UserId == userId, cancellationToken);

        if (existing is null)
        {
            throw new InvalidOperationException("Transaction not found.");
        }

        existing.Amount = decimal.Round(transaction.Amount, 2);
        existing.Date = transaction.Date;
        existing.Type = transaction.Type;
        existing.CategoryId = transaction.CategoryId;
        existing.Description = string.IsNullOrWhiteSpace(transaction.Description)
            ? null
            : transaction.Description!.Length > 512
                ? transaction.Description[..512]
                : transaction.Description;
        existing.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Transactions
            .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId, cancellationToken);

        if (existing is null)
        {
            return;
        }

        _dbContext.Transactions.Remove(existing);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }

        if (amount > 1_000_000_000m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount is unrealistically large.");
        }
    }

    private static void ValidateDate(DateTime date)
    {
        // For now do not allow future-dated transactions.
        // This can be relaxed later via a flag/confirmation.
        if (date.Date > DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException("Transactions cannot be recorded in the future.");
        }
    }

    private static (DateTime Start, DateTime End) GetMonthBounds(DateOnly month)
    {
        var start = new DateTime(month.Year, month.Month, 1);
        var end = start.AddMonths(1);
        return (start, end);
    }
}


