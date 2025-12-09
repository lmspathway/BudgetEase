using System;

namespace BudgetEase.Models;

public enum TransactionType
{
    Expense = 0,
    Income = 1
}

public class Transaction
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public TransactionType Type { get; set; }

    public Guid? CategoryId { get; set; }

    public Category? Category { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


