using ASC.Web.Models;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace ASC.Web.Data
{
    public class NavigationCacheOperations : INavigationCacheOperations
    {
        private readonly IDistributedCache _cache;
        private readonly IWebHostEnvironment _environment;
        private readonly string _navigationCacheName = "NavigationCache";

        public NavigationCacheOperations(
            IDistributedCache cache,
            IWebHostEnvironment environment)
        {
            _cache = cache;
            _environment = environment;
        }

        public async Task CreateNavigationCacheAsync()
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "Navigation", "Navigation.json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Navigation.json not found.", filePath);
            }

            var json = await File.ReadAllTextAsync(filePath);

            await _cache.SetStringAsync(_navigationCacheName, json);
        }

        public async Task<NavigationMenu> GetNavigationCacheAsync()
        {
            var json = await _cache.GetStringAsync(_navigationCacheName);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new NavigationMenu();
            }

            return JsonConvert.DeserializeObject<NavigationMenu>(json) ?? new NavigationMenu();
        }
    }
}