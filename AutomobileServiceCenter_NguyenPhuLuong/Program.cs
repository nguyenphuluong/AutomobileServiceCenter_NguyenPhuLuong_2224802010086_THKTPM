using ASC.Web.Configuration;
using ASC.Web.Data;
using ASC.DataAccess;
using ASC.DataAccess.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// nếu UnitOfWork / Repository dùng DbContext tổng quát thì cần dòng này
builder.Services.AddScoped<DbContext, ApplicationDbContext>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<ApplicationSettings>(
    builder.Configuration.GetSection("ApplicationSettings"));

builder.Services.AddScoped<IIdentitySeed, IdentitySeed>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// seed dữ liệu từ appsettings.json lên database
using (var scope = app.Services.CreateScope())
{
    var identitySeed = scope.ServiceProvider.GetRequiredService<IIdentitySeed>();

    await identitySeed.Seed(
        scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>(),
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
        scope.ServiceProvider.GetRequiredService<IOptions<ApplicationSettings>>()
    );
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();