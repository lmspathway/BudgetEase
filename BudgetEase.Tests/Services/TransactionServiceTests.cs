using System;
using System.Threading.Tasks;
using BudgetEase.Data;
using BudgetEase.Models;
using BudgetEase.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BudgetEase.Tests.Services;

public sealed class TransactionServiceTests
{
    private static BudgetEaseDbContext CreateContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<BudgetEaseDbContext>()
            .UseSqlite(connection)
            .Options;

        var ctx = new BudgetEaseDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    // Test: creating a transaction with zero or negative amount should throw.
    public async Task CreateAsync_Throws_WhenAmountNotPositive()
    {
        using var ctx = CreateContext();
        var service = new TransactionService(ctx);
        var userId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.CreateAsync(userId, 0m, DateTime.UtcNow, TransactionType.Expense, null, "test"));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.CreateAsync(userId, -10m, DateTime.UtcNow, TransactionType.Expense, null, "test"));
    }

    // Test: creating a transaction with a future date should throw.
    public async Task CreateAsync_Throws_WhenDateInFuture()
    {
        using var ctx = CreateContext();
        var service = new TransactionService(ctx);
        var userId = Guid.NewGuid();
        var future = DateTime.UtcNow.AddDays(1);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(userId, 100m, future, TransactionType.Expense, null, "future"));
    }

    // Test: creating a valid transaction should persist it correctly.
    public async Task CreateAsync_PersistsTransaction()
    {
        using var ctx = CreateContext();
        var service = new TransactionService(ctx);
        var userId = Guid.NewGuid();
        var date = new DateTime(2025, 12, 10);

        var tx = await service.CreateAsync(
            userId,
            123.45m,
            date,
            TransactionType.Income,
            null,
            "Salary");

        Assert.NotEqual(Guid.Empty, tx.Id);

        var loaded = await service.GetByIdAsync(userId, tx.Id);
        Assert.NotNull(loaded);
        Assert.Equal(123.45m, loaded!.Amount);
        Assert.Equal(TransactionType.Income, loaded.Type);
        Assert.Equal(date, loaded.Date);
        Assert.Equal("Salary", loaded.Description);
    }

    // Test: search respects date range, category and type filters.
    public async Task SearchAsync_FiltersByDateCategoryAndType()
    {
        using var ctx = CreateContext();
        var service = new TransactionService(ctx);
        var userId = Guid.NewGuid();

        // Seed one category for filtering.
        var cat = new Category { Id = Guid.NewGuid(), Name = "Food", UserId = userId };
        ctx.Categories.Add(cat);
        await ctx.SaveChangesAsync();

        // In range & matches filters
        await service.CreateAsync(userId, 50m, new DateTime(2025, 12, 1), TransactionType.Expense, cat.Id, "Groceries");
        // Different type
        await service.CreateAsync(userId, 100m, new DateTime(2025, 12, 2), TransactionType.Income, cat.Id, "Refund");
        // Outside date range
        await service.CreateAsync(userId, 20m, new DateTime(2025, 11, 30), TransactionType.Expense, cat.Id, "Old");

        var from = new DateTime(2025, 12, 1);
        var to = new DateTime(2025, 12, 31);

        var result = await service.SearchAsync(
            userId,
            from,
            to,
            cat.Id,
            TransactionType.Expense);

        Assert.Single(result);
        Assert.Equal("Groceries", result[0].Description);
    }
}


