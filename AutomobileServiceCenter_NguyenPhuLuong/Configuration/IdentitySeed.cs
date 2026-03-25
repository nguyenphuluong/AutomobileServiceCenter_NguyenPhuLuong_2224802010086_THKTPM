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
            // tạo tất cả role trong appsettings
            var roles = options.Value.Roles.Split(new char[] { ',' });

            foreach (var role in roles)
            {
                try
                {
                    if (!roleManager.RoleExistsAsync(role).Result)
                    {
                        IdentityRole identityRole = new IdentityRole
                        {
                            Name = role
                        };

                        IdentityResult roleResult = await roleManager.CreateAsync(identityRole);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            // tạo admin nếu chưa có
            if (userManager.FindByEmailAsync(options.Value.AdminEmail).Result == null)
            {
                IdentityUser user = new IdentityUser
                {
                    UserName = options.Value.AdminEmail,
                    Email = options.Value.AdminEmail,
                    LockoutEnabled = false
                };

                IdentityResult result = await userManager.CreateAsync(
                    user,
                    options.Value.AdminPassword
                );

                if (result.Succeeded)
                {
                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Name", options.Value.AdminName));
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            // tạo engineer nếu chưa có
            if (userManager.FindByEmailAsync(options.Value.EngineerEmail).Result == null)
            {
                IdentityUser user = new IdentityUser
                {
                    UserName = options.Value.EngineerEmail,
                    Email = options.Value.EngineerEmail,
                    LockoutEnabled = false
                };

                IdentityResult result = await userManager.CreateAsync(
                    user,
                    options.Value.EngineerPassword
                );

                if (result.Succeeded)
                {
                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Name", options.Value.EngineerName));
                    await userManager.AddToRoleAsync(user, "Engineer");
                }
            }
        }
    }
}