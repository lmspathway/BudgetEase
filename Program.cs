using BudgetEase.Components;
using BudgetEase.Data;
using BudgetEase.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<BudgetEaseDbContext>(options =>
{
    // By default use the configured "DefaultConnection". In development this points to SQLite,
    // and in production it is expected to point to SQL Server / Azure SQL.
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                          ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    options.UseSqlite(connectionString);
});

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireDigit = true;
    })
    .AddEntityFrameworkStores<BudgetEaseDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services
    .AddAuthentication()
    .AddIdentityCookies();

builder.Services.AddAuthorization();
builder.Services.AddHttpClient();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Ensure SQLite database and schema are created at startup.
// For now we use EnsureCreated to keep the setup simple (no migrations required yet).
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BudgetEaseDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapPost("/auth/login", async (
        HttpContext httpContext,
        SignInManager<ApplicationUser> signInManager,
        LoginRequest request) =>
    {
        var result = await signInManager.PasswordSignInAsync(
            request.Email,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return Results.Ok();
        }

        if (result.IsLockedOut)
        {
            return Results.BadRequest("Your account is temporarily locked. Please try again later.");
        }

        return Results.BadRequest("Invalid email or password.");
    });

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public record LoginRequest(string Email, string Password, bool RememberMe);
