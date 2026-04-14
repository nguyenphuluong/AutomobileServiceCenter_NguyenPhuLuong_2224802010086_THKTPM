using ASC.DataAccess;
using ASC.DataAccess.Interfaces;
using ASC.Web.Configuration;
using ASC.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASC.Web.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
                throw new Exception("Connection string not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.Configure<ApplicationSettings>(config.GetSection("AppSettings"));

            services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    IConfigurationSection googleAuthSection = config.GetSection("Authentication:Google");
                    options.ClientId = googleAuthSection["ClientId"] ?? "";
                    options.ClientSecret = googleAuthSection["ClientSecret"] ?? "";
                });

            services.AddControllersWithViews();
            services.AddRazorPages();

            return services;
        }

        public static IServiceCollection AddDependencyGroup(this IServiceCollection services)
        {
            // KHÔNG add Identity lần 2 ở đây nữa

            services.AddScoped<IIdentitySeed, IdentitySeed>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddHttpContextAccessor();
            services.AddSingleton<INavigationCacheOperations, NavigationCacheOperations>();

            services.AddTransient<IEmailSender, AuthMessageSender>();

            return services;
        }
    }
}