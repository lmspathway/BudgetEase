using BudgetEase.Components;
using BudgetEase.Data;
using BudgetEase.Models;
using BudgetEase.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
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
    .AddSignInManager()
    .AddEntityFrameworkStores<BudgetEaseDbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    })
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.SlidingExpiration = true;
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.Cookie.Name = ".AspNetCore.Identity.Application";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Em dev, permite HTTP
    });

// HttpContext access for AuthenticationStateProvider
builder.Services.AddHttpContextAccessor();

// Blazor authentication: read from Identity cookie via HttpContext
// Using RevalidatingServerAuthenticationStateProvider for proper Blazor Server integration
builder.Services.AddScoped<BudgetEaseAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<BudgetEaseAuthStateProvider>());

// Authorization + cascading auth state for Blazor
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Minimal API endpoint for login using Identity + cookies.
app.MapPost("/auth/login", async (
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
            // Cookie de autenticação foi criado pelo SignInManager no HttpContext
            // O cookie será enviado automaticamente na resposta HTTP
            return Results.Ok();
        }

        if (result.IsLockedOut)
        {
            return Results.BadRequest("Your account is temporarily locked. Please try again later.");
        }

        return Results.BadRequest("Invalid email or password.");
    });

// Minimal API endpoint for logout using Identity + cookies.
app.MapPost("/auth/logout", async (SignInManager<ApplicationUser> signInManager) =>
    {
        // Fazer logout (remove o cookie de sessão)
        await signInManager.SignOutAsync();
        
        // Cookie foi removido pelo SignInManager no HttpContext
        // O cookie será removido automaticamente na resposta HTTP
        return Results.Ok();
    });

app.Run();

public record LoginRequest(string Email, string Password, bool RememberMe);
