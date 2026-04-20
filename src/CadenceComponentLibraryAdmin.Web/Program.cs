using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Infrastructure.Seed;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));

var fileStorageOptions = builder.Configuration.GetSection("FileStorage").Get<FileStorageOptions>() ?? new FileStorageOptions();
var dataProtectionRoot = fileStorageOptions.AppDataRoot ?? "storage/app-data";
var dataProtectionKeysPath = Path.Combine(dataProtectionRoot, "data-protection-keys");
Directory.CreateDirectory(dataProtectionKeysPath);

builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("CadenceComponentLibraryAdmin");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IChangeLogService, ChangeLogService>();
builder.Services.AddScoped<IPackageFamilyService, PackageFamilyService>();
builder.Services.AddScoped<ICompanyPartService, CompanyPartService>();
builder.Services.AddScoped<IPartAlternateService, PartAlternateService>();
builder.Services.AddScoped<IFileCheckService, FileCheckService>();
builder.Services.AddScoped<IQualityReportService, QualityReportService>();
builder.Services.AddScoped<ILibraryReleaseService, LibraryReleaseService>();
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();
var applySchemaChangesOnStartup =
    app.Environment.IsDevelopment() ||
    app.Configuration.GetValue<bool>("Database:ApplySchemaChangesOnStartup");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (applySchemaChangesOnStartup)
    {
        await DatabaseBootstrapper.InitializeAsync(dbContext);
    }
    else
    {
        await DatabaseBootstrapper.VerifyDatabaseStateAsync(dbContext);
    }
}

await IdentitySeeder.SeedAsync(app.Services, app.Environment.IsDevelopment());

app.Run();
