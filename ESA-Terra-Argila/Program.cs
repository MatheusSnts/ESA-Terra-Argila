using ESA_Terra_Argila.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using ESA_Terra_Argila.Resources.ErrorDescribers;
using ESA_Terra_Argila.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;
using ESA_Terra_Argila.Policies.AuthorizationRequirements;
using ESA_Terra_Argila.Policies;
using Microsoft.AspNetCore.Authorization;



var builder = WebApplication.CreateBuilder(args);

// SERVICES
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddScoped<AdminDashboardService>();
builder.Services.AddScoped<SupplierDashboardService>();
builder.Services.AddScoped<VendorDashboardService>();

builder.Services.AddScoped<IUserActivityService, UserActivityService>();




builder.Services.AddTransient<IEmailSender, EmailSender>();


builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<PortugueseIdentityErrorDescriber>()
    .AddDefaultUI();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AcceptedByAdmin", policy =>
        policy.Requirements.Add(new AcceptedByAdminRequirement()));
});

builder.Services.AddScoped<IAuthorizationHandler, AcceptedByAdminHandler>();

builder.Services.Configure<IdentityOptions>(options =>
{
// Configurações de Lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;
});

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

builder.Services.AddRazorPages();

builder.Services.AddHttpClient();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    await context.Database.MigrateAsync();

    // SEEDERS
    await Seeder.SeedRoles(roleManager);
    await Seeder.SeedUsersAsync(userManager);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();


