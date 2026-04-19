using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Infrastructure.Services;

public sealed class FileCheckService : IFileCheckService
{
    private readonly ApplicationDbContext _dbContext;

    public FileCheckService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public bool FileExists(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            var normalizedPath = path;

            if (!Path.IsPathRooted(normalizedPath) && !normalizedPath.StartsWith(@"\\", StringComparison.Ordinal))
            {
                normalizedPath = Path.Combine(Directory.GetCurrentDirectory(), normalizedPath);
            }

            return File.Exists(normalizedPath);
        }
        catch
        {
            return false;
        }
    }

    public async Task<FileCheckSummary> CheckReleasePartsAsync()
    {
        var summary = new FileCheckSummary();

        var symbols = await _dbContext.SymbolFamilies
            .AsNoTracking()
            .Select(x => new { x.SymbolFamilyCode, x.OlbPath })
            .ToListAsync();

        foreach (var symbol in symbols)
        {
            summary.TotalChecked++;

            if (!FileExists(symbol.OlbPath))
            {
                summary.MissingCount++;
                summary.Issues.Add(new FileCheckIssue
                {
                    FileType = "OLB",
                    OwnerType = "SymbolFamily",
                    OwnerKey = symbol.SymbolFamilyCode,
                    Path = symbol.OlbPath,
                    Status = "Missing"
                });
            }
        }

        var footprints = await _dbContext.FootprintVariants
            .AsNoTracking()
            .Select(x => new
            {
                x.FootprintName,
                x.PsmPath,
                x.DraPath,
                x.StepPath
            })
            .ToListAsync();

        foreach (var footprint in footprints)
        {
            AddIssue(summary, "PSM", "FootprintVariant", footprint.FootprintName, footprint.PsmPath);
            AddIssue(summary, "DRA", "FootprintVariant", footprint.FootprintName, footprint.DraPath);
            AddIssue(summary, "STEP", "FootprintVariant", footprint.FootprintName, footprint.StepPath);
        }

        return summary;
    }

    private void AddIssue(FileCheckSummary summary, string fileType, string ownerType, string ownerKey, string? path)
    {
        summary.TotalChecked++;

        if (!FileExists(path))
        {
            summary.MissingCount++;
            summary.Issues.Add(new FileCheckIssue
            {
                FileType = fileType,
                OwnerType = ownerType,
                OwnerKey = ownerKey,
                Path = path,
                Status = "Missing"
            });
        }
    }
}
