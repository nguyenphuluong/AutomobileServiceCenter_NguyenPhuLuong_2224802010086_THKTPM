using ASC.DataAccess.Interfaces;
using ASC.Web.Configuration;
using ASC.Web.Data;
using ASC.Web.Hubs;
using ASC.Web.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

#region Services

builder.Services
    .AddConfig(builder.Configuration)
    .AddDependencyGroup();

builder.Services.AddSignalR();

#endregion

var app = builder.Build();

#region Seed Identity

using (var scope = app.Services.CreateScope())
{
    var identitySeed =
        scope.ServiceProvider.GetRequiredService<IIdentitySeed>();

    await identitySeed.Seed(
        scope.ServiceProvider
            .GetRequiredService<UserManager<IdentityUser>>(),

        scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>(),

        scope.ServiceProvider
            .GetRequiredService<IOptions<ApplicationSettings>>()
    );
}

#endregion

#region Navigation Cache

using (var scope = app.Services.CreateScope())
{
    var navigationCacheOperations =
        scope.ServiceProvider
            .GetRequiredService<INavigationCacheOperations>();

    await navigationCacheOperations
        .CreateNavigationCacheAsync();
}

#endregion

#region Error Handling

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

#endregion

#region Middleware

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

#endregion

#region SignalR

app.MapHub<ServiceMessagesHub>(
    "/serviceMessageHub");

#endregion

#region Routes

app.MapControllerRoute(
    name: "areas",
    pattern:
    "{area:exists}/{controller=Dashboard}/{action=Dashboard}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern:
    "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

#endregion

#region MasterData Cache

using (var scope = app.Services.CreateScope())
{
    var masterDataCacheOperations =
        scope.ServiceProvider
            .GetRequiredService<IMasterDataCacheOperations>();

    await masterDataCacheOperations
        .CreateMasterDataCacheAsync();
}

#endregion

app.Run();