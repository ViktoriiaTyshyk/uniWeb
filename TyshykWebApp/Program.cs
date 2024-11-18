using Microsoft.EntityFrameworkCore;
using TyshykWebApp.Data;
using TyshykWebApp.Models;
using TyshykWebApp.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DBConn");

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<UserDBContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddDbContext<ComputationDBContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<User>(options => {
    options.SignIn.RequireConfirmedAccount = false;   // set true for release
    options.Password.RequiredLength = 4;
    options.Password.RequireDigit = true;
    }).AddEntityFrameworkStores<UserDBContext>();

builder.Services.AddSingleton<CalculationManager>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

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

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
