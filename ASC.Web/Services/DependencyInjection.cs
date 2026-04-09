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
            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<DbContext, ApplicationDbContext>();

            services.Configure<ApplicationSettings>(
                config.GetSection("ApplicationSettings"));

            return services;
        }

        public static IServiceCollection AddDependencyGroup(this IServiceCollection services)
        {
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.AddScoped<IIdentitySeed, IdentitySeed>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<INavigationCacheOperations, NavigationCacheOperations>();

            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddTransient<IEmailSender, AuthMessageSender>();
            return services;
        }
    }
}