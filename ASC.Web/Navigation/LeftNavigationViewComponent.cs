using ASC.Utilities;
using ASC.Web.Data;
using ASC.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASC.Web.Navigation
{
    [ViewComponent(Name = "LeftNavigation")]
    public class LeftNavigationViewComponent : ViewComponent
    {
        private readonly INavigationCacheOperations _navigationCacheOperations;

        public LeftNavigationViewComponent(INavigationCacheOperations navigationCacheOperations)
        {
            _navigationCacheOperations = navigationCacheOperations;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menu = await _navigationCacheOperations.GetNavigationCacheAsync();

            var roles = GetCurrentUserRoles();

            if (roles.Count == 0)
            {
                menu.MenuItems = new List<NavigationMenuItem>();
                return View(menu);
            }

            menu.MenuItems = menu.MenuItems
                .Where(x =>
                    HasAnyRole(x.UserRoles, roles) ||
                    HasAnyNestedItemWithRole(x.NestedItems, roles))
                .OrderBy(x => x.Sequence)
                .Select(x => new NavigationMenuItem
                {
                    DisplayName = x.DisplayName,
                    MaterialIcon = x.MaterialIcon,
                    Link = x.Link,
                    IsNested = x.IsNested,
                    Sequence = x.Sequence,
                    UserRoles = x.UserRoles,
                    NestedItems = x.NestedItems?
                        .Where(n => HasAnyRole(n.UserRoles, roles))
                        .OrderBy(n => n.Sequence)
                        .ToList() ?? new List<NavigationMenuItem>()
                })
                .ToList();

            return View(menu);
        }

        private List<string> GetCurrentUserRoles()
        {
            var roles = new List<string>();

            // Cách 1: Lấy role từ session CurrentUser nếu project có set session
            var currentUser = HttpContext.Session.GetSession<CurrentUser>("CurrentUser");

            if (currentUser?.Roles != null && currentUser.Roles.Length > 0)
            {
                roles.AddRange(currentUser.Roles);
            }

            // Cách 2: Nếu session chưa có role, lấy trực tiếp từ Claims/Identity
            roles.AddRange(HttpContext.User.Claims
                .Where(c =>
                    c.Type == ClaimTypes.Role ||
                    c.Type == "role" ||
                    c.Type == "roles")
                .Select(c => c.Value));

            return roles
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool HasAnyRole(IEnumerable<string>? itemRoles, List<string> userRoles)
        {
            if (itemRoles == null || !itemRoles.Any())
            {
                return false;
            }

            return itemRoles.Any(itemRole =>
                userRoles.Any(userRole =>
                    string.Equals(itemRole, userRole, StringComparison.OrdinalIgnoreCase)));
        }

        private static bool HasAnyNestedItemWithRole(
            IEnumerable<NavigationMenuItem>? nestedItems,
            List<string> userRoles)
        {
            if (nestedItems == null || !nestedItems.Any())
            {
                return false;
            }

            return nestedItems.Any(n => HasAnyRole(n.UserRoles, userRoles));
        }
    }
}