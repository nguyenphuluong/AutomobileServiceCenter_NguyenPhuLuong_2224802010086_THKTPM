using ASC.Web.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ASC.Web.Data
{
    public class IdentitySeed : IIdentitySeed
    {
        public async Task Seed(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<ApplicationSettings> options)
        {
            var rolesText = options.Value?.Roles;

            if (string.IsNullOrWhiteSpace(rolesText))
                throw new Exception("AppSettings:Roles chưa được cấu hình trong appsettings.json");

            var roles = rolesText
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole
                    {
                        Name = role
                    });
                }
            }

            // admin
            if (await userManager.FindByEmailAsync(options.Value.AdminEmail) == null)
            {
                var admin = new IdentityUser
                {
                    UserName = options.Value.AdminEmail,
                    Email = options.Value.AdminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, options.Value.AdminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // engineer
            if (await userManager.FindByEmailAsync(options.Value.EngineerEmail) == null)
            {
                var engineer = new IdentityUser
                {
                    UserName = options.Value.EngineerEmail,
                    Email = options.Value.EngineerEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(engineer, options.Value.EngineerPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(engineer, "Engineer");
                }
            }
        }
    }
}