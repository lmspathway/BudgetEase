using BudgetEase.Models;

namespace BudgetEase.Services;

/// <summary>
/// Domain service responsible for validating and persisting transactions.
/// UI components should talk to this service instead of using the DbContext directly.
/// </summary>
public interface ITransactionService
{
    Task<Transaction> CreateAsync(
        Guid userId,
        decimal amount,
        DateTime date,
        TransactionType type,
        Guid? categoryId,
        string? description,
        CancellationToken cancellationToken = default);

    Task<Transaction?> GetByIdAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transaction>> GetRecentAsync(
        Guid userId,
        int count = 5,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transaction>> GetByMonthAsync(
        Guid userId,
        DateOnly month,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns transactions filtered by optional date range, category and type.
    /// Results are ordered descending by date and creation time.
    /// </summary>
    Task<IReadOnlyList<Transaction>> SearchAsync(
        Guid userId,
        DateTime? from,
        DateTime? to,
        Guid? categoryId,
        TransactionType? type,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid userId,
        Transaction transaction,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken = default);
}


