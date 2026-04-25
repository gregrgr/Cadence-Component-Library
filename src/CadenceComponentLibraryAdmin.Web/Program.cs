using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.CadenceBridge.Queue;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Infrastructure.Seed;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Headers;
using System.Reflection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);
var bootstrapAdminOptions = builder.Configuration.GetSection("BootstrapAdmin").Get<BootstrapAdminOptions>();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.AddRazorPages();
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));
builder.Services.Configure<ExternalImportOptions>(builder.Configuration.GetSection("ExternalImports"));
builder.Services.Configure<CadenceAutomationOptions>(builder.Configuration.GetSection("CadenceAutomation"));
builder.Services.Configure<AiExtractionOptions>(builder.Configuration.GetSection("AiExtraction"));

var fileStorageOptions = builder.Configuration.GetSection("FileStorage").Get<FileStorageOptions>() ?? new FileStorageOptions();
var dataProtectionRoot = fileStorageOptions.AppDataRoot ?? "storage/app-data";
var dataProtectionKeysPath = Path.Combine(dataProtectionRoot, "data-protection-keys");
Directory.CreateDirectory(dataProtectionKeysPath);

builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("CadenceComponentLibraryAdmin");

builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
    if (environment.IsDevelopment())
    {
        // Development auto-migration should stay convenient even when EF emits
        // a runtime-only pending-model warning that tooling does not reproduce.
        options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }
});
builder.Services.AddScoped<IAdminAuditService, AdminAuditService>();
builder.Services.AddScoped<IChangeLogService, ChangeLogService>();
builder.Services.AddScoped<IExternalImportService, ExternalImportService>();
builder.Services.AddScoped<ICadenceBuildJobQueue, FileSystemCadenceJobQueue>();
builder.Services.AddScoped<ICadenceJobQueue, FileSystemCadenceJobQueue>();
builder.Services.AddScoped<ICadenceJobSimulator, DevelopmentCadenceJobSimulator>();
builder.Services.AddScoped<IMcpLibraryWorkflowService, McpLibraryWorkflowService>();
builder.Services.AddScoped<IDatasheetTextExtractor, LocalPdfTextExtractor>();
builder.Services.AddScoped<IJsonSchemaValidationService, JsonSchemaValidationService>();
builder.Services.AddHttpClient<OpenAiCompatibleDatasheetExtractionService>();
builder.Services.AddHttpClient<ICodexCliRunner, CodexCliHttpBridgeRunner>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<AiExtractionOptions>>().Value.CodexCli;
    var timeoutSeconds = Math.Clamp(options.TimeoutSeconds + 30, 30, 1900);
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
});
builder.Services.AddScoped<CodexCliDatasheetExtractionService>();
builder.Services.AddScoped<StubAiDatasheetExtractionService>();
builder.Services.AddScoped<IAiDatasheetExtractionService>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<AiExtractionOptions>>().Value;
    var useCodexCli = options.CodexCli.Enabled ||
                      string.Equals(options.Mode, "CodexCli", StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(options.Mode, "Codex", StringComparison.OrdinalIgnoreCase);
    var useOpenAi = options.OpenAI.Enabled ||
                    string.Equals(options.Mode, "OpenAI", StringComparison.OrdinalIgnoreCase);

    if (useCodexCli)
    {
        return serviceProvider.GetRequiredService<CodexCliDatasheetExtractionService>();
    }

    return useOpenAi
        ? serviceProvider.GetRequiredService<OpenAiCompatibleDatasheetExtractionService>()
        : serviceProvider.GetRequiredService<StubAiDatasheetExtractionService>();
});
builder.Services.AddHttpClient<INlbnEasyEdaClient, NlbnEasyEdaClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ExternalImportOptions>>().Value;
    client.BaseAddress = new Uri(options.EasyEdaNlbn.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);

    var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
        ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
        ?? "dev";
    client.DefaultRequestHeaders.UserAgent.Clear();
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("CadenceComponentLibraryAdmin", version));
});
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
var supportedCultures = new[]
{
    new CultureInfo("en-US"),
    new CultureInfo("zh-CN")
};
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};
var applySchemaChangesOnStartup =
    app.Environment.IsDevelopment() ||
    app.Configuration.GetValue<bool>("Database:ApplySchemaChangesOnStartup");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRequestLocalization(localizationOptions);
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

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

await IdentitySeeder.SeedAsync(app.Services, app.Environment.IsDevelopment(), bootstrapAdminOptions);

app.Run();
