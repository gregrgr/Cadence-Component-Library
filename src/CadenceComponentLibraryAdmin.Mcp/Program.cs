using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.CadenceBridge.Queue;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using CadenceComponentLibraryAdmin.Mcp.Mcp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<CadenceAutomationOptions>(builder.Configuration.GetSection("CadenceAutomation"));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IMcpLibraryWorkflowService, McpLibraryWorkflowService>();
builder.Services.AddScoped<ICadenceBuildJobQueue, FileSystemCadenceJobQueue>();
builder.Services.AddScoped<ICadenceJobQueue, FileSystemCadenceJobQueue>();
builder.Services.AddScoped<ICadenceJobSimulator, DevelopmentCadenceJobSimulator>();
builder.Services.AddScoped<LibraryMcpToolCatalog>();

// Placeholder adapter: this keeps the console project buildable until the official
// Model Context Protocol C# SDK is wired into the repository.
builder.Services.AddSingleton<IMcpServerAdapter, PlaceholderMcpServerAdapter>();

using var host = builder.Build();
using var scope = host.Services.CreateScope();
var adapter = scope.ServiceProvider.GetRequiredService<IMcpServerAdapter>();
await adapter.RunAsync(CancellationToken.None);
