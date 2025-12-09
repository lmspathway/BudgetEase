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
    private readonly IAuthenticationService _authenticationService;

    public BudgetEaseAuthStateProvider(
        IHttpContextAccessor httpContextAccessor,
        IAuthenticationService authenticationService,
        ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _authenticationService = authenticationService;
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

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext == null)
        {
            // No HttpContext available - return anonymous
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        // Authenticate the request using the Identity cookie scheme
        // This ensures the cookie is read and HttpContext.User is populated
        var authenticateResult = await _authenticationService.AuthenticateAsync(
            httpContext, 
            IdentityConstants.ApplicationScheme);

        if (authenticateResult?.Succeeded == true && authenticateResult.Principal?.Identity?.IsAuthenticated == true)
        {
            // User is authenticated via Identity cookie - return their ClaimsPrincipal
            return new AuthenticationState(authenticateResult.Principal);
        }

        // Check HttpContext.User as fallback (should be populated by middleware)
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            return new AuthenticationState(httpContext.User);
        }

        // User is not authenticated - return anonymous
        // AuthorizeRouteView will handle redirect to /login
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}


