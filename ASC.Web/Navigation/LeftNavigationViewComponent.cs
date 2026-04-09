using ASC.Utilities;
using ASC.Web.Data;
using ASC.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ASC.Web.Navigation
{
    [ViewComponent(Name = "ASC.Web.Navigation.LeftNavigation")]
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
            var currentUser = HttpContext.Session.GetSession<CurrentUser>("CurrentUser");

            if (currentUser == null || currentUser.Roles == null || currentUser.Roles.Length == 0)
            {
                menu.MenuItems = new List<NavigationMenuItem>();
                return View(menu);
            }

            var roles = currentUser.Roles.ToList();

            menu.MenuItems = menu.MenuItems
                .Where(x => x.UserRoles != null && x.UserRoles.Any(r => roles.Contains(r)))
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
                        .Where(n => n.UserRoles != null && n.UserRoles.Any(r => roles.Contains(r)))
                        .OrderBy(n => n.Sequence)
                        .ToList() ?? new List<NavigationMenuItem>()
                })
                .ToList();

            return View(menu);
        }
    }
}