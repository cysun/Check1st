using Check1st.Models;
using Check1st.Security;
using Check1st.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment;
var configuration = builder.Configuration;
var services = builder.Services;

// In production, this app will sit behind a Nginx reverse proxy with HTTPS
if (!environment.IsDevelopment())
    builder.WebHost.UseUrls("http://localhost:5006");

builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

// Configure Services

services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB (default 30MB)
});

services.AddControllersWithViews();

services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    // options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies(options => { });

services.AddIdentityCore<User>()
    .AddRoles<IdentityRole>() // need to be before AddEntityFrameworkStores
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddSignInManager();

services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";
});

services.AddAuthorization(options =>
{
    options.AddPolicy("IsAdmin", policy => policy.RequireClaim("Admin"));
});

services.AddAuthorization(options =>
{
    options.AddPolicy(Constants.Policy.IsAdmin, policy => policy.RequireRole(Constants.Role.Admin.ToString()));
    options.AddPolicy(Constants.Policy.IsAdminOrTeacher, policy =>
        policy.RequireRole(Constants.Role.Admin.ToString(), Constants.Role.Teacher.ToString()));
    options.AddPolicy(Constants.Policy.CanReadConsultation, policy =>
        policy.AddRequirements(new CanReadConsultationRequirement()));
});

services.AddScoped<IAuthorizationHandler, CanReadConsultationHandler>();

services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

services.Configure<AISettings>(configuration.GetSection("AI"));
services.AddSingleton<AIService>();

services.AddAutoMapper(config => config.AddProfile<MapperProfile>());

services.AddScoped<FileService>();
services.AddScoped<AssignmentService>();
services.AddScoped<ConsultationService>();

// Build App

var app = builder.Build();

// Configure Middleware Pipeline

app.UseForwardedHeaders();

if (!environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Run App

app.Run();
