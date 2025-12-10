using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace BudgetEase.Services;

/// <summary>
/// Authentication state provider that reads from the Identity cookie via HttpContext.
/// Uses RevalidatingServerAuthenticationStateProvider base class for proper Blazor Server integration.
/// This provider ensures that the authentication cookie is properly read and the user state is correctly detected.
/// </summary>
public class BudgetEaseAuthStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BudgetEaseAuthStateProvider(
        IHttpContextAccessor httpContextAccessor,
        IAuthenticationService authenticationService,
        ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    protected override Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        // Revalidate by checking HttpContext.User (populated from cookie by middleware)
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            // User has been authenticated by the cookie middleware, just use that principal.
            return Task.FromResult(new AuthenticationState(httpContext.User));
        }

        // No user or not authenticated – return anonymous principal.
        return Task.FromResult(
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }
}


