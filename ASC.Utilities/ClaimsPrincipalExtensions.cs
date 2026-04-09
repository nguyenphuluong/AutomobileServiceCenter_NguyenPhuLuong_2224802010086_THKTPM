using System.Security.Claims;

namespace ASC.Utilities
{
    public static class ClaimsPrincipalExtensions
    {
        public static CurrentUser GetCurrentUserDetails(this ClaimsPrincipal principal)
        {
            if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
            {
                return null;
            }

            var name = principal.FindFirst(ClaimTypes.Name)?.Value
                       ?? principal.Identity.Name
                       ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? string.Empty;

            var email = principal.FindFirst(ClaimTypes.Email)?.Value
                        ?? principal.Identity.Name
                        ?? string.Empty;

            var roles = principal.FindAll(ClaimTypes.Role)
                                 .Select(c => c.Value)
                                 .ToArray();

            var isActiveClaim = principal.FindFirst("IsActive")?.Value;
            var isActive = bool.TryParse(isActiveClaim, out bool parsedIsActive) && parsedIsActive;

            return new CurrentUser
            {
                Name = name,
                Email = email,
                Roles = roles,
                IsActive = isActive
            };
        }
    }
}