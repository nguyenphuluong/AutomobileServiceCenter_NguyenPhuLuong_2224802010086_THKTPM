using ASC.Business;
using ASC.Business.Interfaces;
using ASC.DataAccess;
using ASC.DataAccess.Interfaces;
using ASC.Web.Areas.Configuration.Models;
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
            // Seed / identity services
            services.AddScoped<IIdentitySeed, IdentitySeed>();

            // DbContext mapping cho UnitOfWork đang nhận DbContext
            services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

            // Data access / business
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IMasterDataOperations, MasterDataOperations>();

            // AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            // Cache / session
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddHttpContextAccessor();

            // Navigation
            services.AddScoped<INavigationCacheOperations, NavigationCacheOperations>();

            // Email
            services.AddTransient<IEmailSender, AuthMessageSender>();

            return services;
        }
    }
}