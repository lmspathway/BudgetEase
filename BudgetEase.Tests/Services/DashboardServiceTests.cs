using System;
using System.Threading.Tasks;
using BudgetEase.Data;
using BudgetEase.Models;
using BudgetEase.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BudgetEase.Tests.Services;

public sealed class DashboardServiceTests
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

    // Test: dashboard summary aggregates totals and category breakdown correctly.
    public async Task GetSummaryAsync_ComputesTotalsAndCategoryBreakdown()
    {
        using var ctx = CreateContext();
        var service = new DashboardService(ctx);
        var userId = Guid.NewGuid();

        var food = new Category { Id = Guid.NewGuid(), Name = "Food", UserId = userId, ColorHex = "#F97316" };
        var bills = new Category { Id = Guid.NewGuid(), Name = "Bills", UserId = userId, ColorHex = "#3B82F6" };
        ctx.Categories.AddRange(food, bills);

        // Reference month: December 2025
        var month = new DateOnly(2025, 12, 1);
        var dec1 = new DateTime(2025, 12, 1);
        var dec2 = new DateTime(2025, 12, 2);

        // Current month transactions
        ctx.Transactions.AddRange(
            new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = 4000m,
                Date = dec1,
                Type = TransactionType.Income,
                CategoryId = null,
                Description = "Salary",
                CreatedAt = dec1,
                UpdatedAt = dec1
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = 1500m,
                Date = dec2,
                Type = TransactionType.Expense,
                CategoryId = bills.Id,
                Description = "Rent",
                CreatedAt = dec2,
                UpdatedAt = dec2
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = 500m,
                Date = dec2,
                Type = TransactionType.Expense,
                CategoryId = food.Id,
                Description = "Groceries",
                CreatedAt = dec2,
                UpdatedAt = dec2
            });

        // Previous month income/expense
        ctx.Transactions.AddRange(
            new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = 3000m,
                Date = new DateTime(2025, 11, 1),
                Type = TransactionType.Income,
                CreatedAt = new DateTime(2025, 11, 1),
                UpdatedAt = new DateTime(2025, 11, 1)
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = 1000m,
                Date = new DateTime(2025, 11, 2),
                Type = TransactionType.Expense,
                CreatedAt = new DateTime(2025, 11, 2),
                UpdatedAt = new DateTime(2025, 11, 2)
            });

        await ctx.SaveChangesAsync();

        var summary = await service.GetSummaryAsync(userId, month);

        Assert.Equal(4000m, summary.CurrentIncome);
        Assert.Equal(2000m, summary.CurrentExpense);
        Assert.Equal(3000m, summary.PreviousIncome);
        Assert.Equal(1000m, summary.PreviousExpense);

        // Net
        Assert.Equal(2000m, summary.CurrentNet);
        Assert.Equal(2000m, summary.PreviousNet);

        // Category breakdown
        Assert.Equal(2, summary.CategoryBreakdown.Count);
        var billsItem = Assert.Single(summary.CategoryBreakdown, c => c.CategoryName == "Bills");
        Assert.Equal(1500m, billsItem.TotalAmount);
        Assert.Equal(75m, billsItem.PercentageOfTotal);

        var foodItem = Assert.Single(summary.CategoryBreakdown, c => c.CategoryName == "Food");
        Assert.Equal(500m, foodItem.TotalAmount);
        Assert.Equal(25m, foodItem.PercentageOfTotal);
    }
}


