using BudgetEase.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BudgetEase.Data;

public class BudgetEaseDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public BudgetEaseDbContext(DbContextOptions<BudgetEaseDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureUserSettings(modelBuilder);
        ConfigureCategory(modelBuilder);
        ConfigureTransaction(modelBuilder);

        SeedDefaultCategories(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ApplicationUser>();

        entity.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(128);
    }

    private static void ConfigureUserSettings(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserSettings>();

        entity.HasOne(us => us.User)
            .WithOne(u => u.Settings)
            .HasForeignKey<UserSettings>(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.Property(us => us.Currency)
            .IsRequired()
            .HasMaxLength(16);

        entity.Property(us => us.Theme)
            .IsRequired()
            .HasMaxLength(32);

        entity.Property(us => us.TimeZone)
            .IsRequired()
            .HasMaxLength(128);
    }

    private static void ConfigureCategory(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Category>();

        entity.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(64);

        entity.Property(c => c.ColorHex)
            .HasMaxLength(16);

        entity.HasOne(c => c.User)
            .WithMany(u => u.Categories)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(c => new { c.UserId, c.Name })
            .IsUnique();
    }

    private static void ConfigureTransaction(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Transaction>();

        entity.Property(t => t.Amount)
            .HasColumnType("decimal(18,2)");

        entity.Property(t => t.Description)
            .HasMaxLength(512);

        entity.HasOne(t => t.User)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(t => new { t.UserId, t.Date });
    }

    private static void SeedDefaultCategories(ModelBuilder modelBuilder)
    {
        var foodId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var billsId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var transportId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = foodId,
                UserId = null,
                Name = "Food",
                ColorHex = "#F97316",
                IsDefaultGlobal = true
            },
            new Category
            {
                Id = billsId,
                UserId = null,
                Name = "Bills",
                ColorHex = "#3B82F6",
                IsDefaultGlobal = true
            },
            new Category
            {
                Id = transportId,
                UserId = null,
                Name = "Transport",
                ColorHex = "#10B981",
                IsDefaultGlobal = true
            }
        );
    }
}


