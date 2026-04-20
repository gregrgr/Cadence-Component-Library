using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CadenceComponentLibraryAdmin.Web.Controllers;

[Authorize]
public sealed class ExternalImportsController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IExternalImportService _externalImportService;
    private readonly INlbnEasyEdaClient _nlbnEasyEdaClient;
    private readonly ExternalImportOptions _options;

    public ExternalImportsController(
        ApplicationDbContext dbContext,
        IExternalImportService externalImportService,
        INlbnEasyEdaClient nlbnEasyEdaClient,
        IOptions<ExternalImportOptions> options)
    {
        _dbContext = dbContext;
        _externalImportService = externalImportService;
        _nlbnEasyEdaClient = nlbnEasyEdaClient;
        _options = options.Value;
    }

    public async Task<IActionResult> Index(
        string? search,
        string? lcscId,
        string? manufacturer,
        string? manufacturerPn,
        string? symbol,
        string? footprint,
        string? model3D,
        ExternalImportStatus? importStatus,
        bool hasDatasheet = false,
        bool hasStep = false,
        bool has3D = false,
        bool hasThumbnail = false,
        bool duplicateWarning = false,
        int page = 1,
        int pageSize = 20)
    {
        var query = _dbContext.ExternalComponentImports
            .Include(x => x.FootprintRenderAsset)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                (x.Name != null && x.Name.Contains(search)) ||
                (x.Description != null && x.Description.Contains(search)) ||
                (x.SearchKeyword != null && x.SearchKeyword.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(lcscId))
        {
            query = query.Where(x => x.LcscId != null && x.LcscId.Contains(lcscId));
        }

        if (!string.IsNullOrWhiteSpace(manufacturer))
        {
            query = query.Where(x => x.Manufacturer != null && x.Manufacturer.Contains(manufacturer));
        }

        if (!string.IsNullOrWhiteSpace(manufacturerPn))
        {
            query = query.Where(x => x.ManufacturerPN != null && x.ManufacturerPN.Contains(manufacturerPn));
        }

        if (!string.IsNullOrWhiteSpace(symbol))
        {
            query = query.Where(x =>
                (x.SymbolName != null && x.SymbolName.Contains(symbol)) ||
                (x.SymbolShapeJson != null && x.SymbolShapeJson.Contains(symbol)));
        }

        if (!string.IsNullOrWhiteSpace(footprint))
        {
            query = query.Where(x =>
                (x.FootprintName != null && x.FootprintName.Contains(footprint)) ||
                (x.PackageName != null && x.PackageName.Contains(footprint)));
        }

        if (!string.IsNullOrWhiteSpace(model3D))
        {
            query = query.Where(x => x.Model3DName != null && x.Model3DName.Contains(model3D));
        }

        if (importStatus.HasValue)
        {
            query = query.Where(x => x.ImportStatus == importStatus.Value);
        }

        if (hasDatasheet)
        {
            query = query.Where(x => x.DatasheetAssetId.HasValue || x.DatasheetUrl != null);
        }

        if (hasStep)
        {
            query = query.Where(x => x.StepAssetId.HasValue || x.StepUrl != null);
        }

        if (has3D)
        {
            query = query.Where(x => x.Model3DUuid != null || x.Model3DName != null);
        }

        if (hasThumbnail)
        {
            query = query.Where(x => x.FootprintPreviewAssetId.HasValue || x.FootprintRenderAssetId.HasValue);
        }

        if (duplicateWarning)
        {
            query = query.Where(x => x.DuplicateWarning != null && x.DuplicateWarning != string.Empty);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.LastImportedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ExternalImportListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Manufacturer = x.Manufacturer,
                ManufacturerPN = x.ManufacturerPN,
                LcscId = x.LcscId,
                PackageName = x.PackageName ?? x.FootprintName,
                Model3DName = x.Model3DName,
                HasDatasheet = x.DatasheetAssetId.HasValue || x.DatasheetUrl != null,
                HasStep = x.StepAssetId.HasValue || x.StepUrl != null,
                HasObj = x.ObjAssetId.HasValue,
                HasSymbolRaw = x.SymbolShapeJson != null,
                HasFootprintRaw = x.FootprintShapeJson != null,
                HasRawJson = x.EasyEdaRawJson != null || x.FullRawJson != null,
                HasPreview = x.FootprintPreviewAssetId.HasValue || x.FootprintRenderAssetId.HasValue,
                Has3DModel = x.Model3DUuid != null || x.Model3DName != null,
                DuplicateWarning = x.DuplicateWarning,
                ImportStatus = x.ImportStatus,
                PreviewAssetId = x.FootprintPreviewAssetId ?? x.FootprintRenderAssetId
            })
            .ToListAsync();

        return View(new ExternalImportsIndexViewModel
        {
            Result = new PagedResult<ExternalImportListItemViewModel>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            },
            Search = search,
            LcscId = lcscId,
            Manufacturer = manufacturer,
            ManufacturerPn = manufacturerPn,
            Symbol = symbol,
            Footprint = footprint,
            Model3D = model3D,
            ImportStatus = importStatus,
            HasDatasheet = hasDatasheet,
            HasStep = hasStep,
            Has3D = has3D,
            HasThumbnail = hasThumbnail,
            DuplicateWarning = duplicateWarning,
            ImportForm = new ExternalImportFromLcscInputModel
            {
                GeneratePreview = _options.EasyEdaNlbn.GeneratePreviewByDefault
            },
            BatchImportForm = new ExternalImportBatchInputModel
            {
                GeneratePreview = _options.EasyEdaNlbn.GeneratePreviewByDefault,
                MaxParallelImports = _options.EasyEdaNlbn.MaxParallelImports
            }
        });
    }

    public async Task<IActionResult> Details(long id)
    {
        var import = await _dbContext.ExternalComponentImports
            .Include(x => x.Candidate)
            .Include(x => x.FootprintRenderAsset)
            .Include(x => x.DatasheetAsset)
            .Include(x => x.ManualAsset)
            .Include(x => x.StepAsset)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (import is null)
        {
            return NotFound();
        }

        var assets = await _dbContext.ExternalComponentAssets
            .Where(x => x.ExternalComponentImportId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return View(new ExternalImportDetailsViewModel
        {
            Import = import,
            Assets = assets
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Librarian,EEReviewer")]
    public async Task<IActionResult> CreateCandidate(long id)
    {
        var candidate = await _externalImportService.CreateCandidateAsync(id, User.Identity?.Name ?? "system");
        TempData["SuccessMessage"] = $"Online Candidate #{candidate.Id} created from external import.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Librarian,EEReviewer")]
    public async Task<IActionResult> ImportFromLcsc(
        [Bind(Prefix = "ImportForm")] ExternalImportFromLcscInputModel input,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.LcscId))
        {
            TempData["ErrorMessage"] = "LCSC ID is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var import = await _nlbnEasyEdaClient.ImportByLcscIdAsync(
                input.LcscId,
                new NlbnImportOptions(input.DownloadStep, input.DownloadObj, input.GeneratePreview),
                User.Identity?.Name ?? "system",
                cancellationToken);

            TempData["SuccessMessage"] = $"Imported {import.LcscId} into staging.";
            return RedirectToAction(nameof(Details), new { id = import.Id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Librarian,EEReviewer")]
    public async Task<IActionResult> BatchImportFromLcsc(
        [Bind(Prefix = "BatchImportForm")] ExternalImportBatchInputModel input,
        CancellationToken cancellationToken)
    {
        var lcscIds = (input.LcscIds ?? string.Empty)
            .Split(['\r', '\n', ',', ';', '\t', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (lcscIds.Count == 0)
        {
            TempData["ErrorMessage"] = "Please enter at least one LCSC ID.";
            return RedirectToAction(nameof(Index));
        }

        var maxParallel = Math.Clamp(
            input.MaxParallelImports ?? _options.EasyEdaNlbn.MaxParallelImports,
            1,
            Math.Max(1, _options.EasyEdaNlbn.MaxParallelImports));

        var semaphore = new SemaphoreSlim(maxParallel, maxParallel);
        var importedIds = new List<string>();
        var errors = new List<string>();

        var tasks = lcscIds.Select(async lcscId =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                await _nlbnEasyEdaClient.ImportByLcscIdAsync(
                    lcscId,
                    new NlbnImportOptions(input.DownloadStep, false, input.GeneratePreview),
                    User.Identity?.Name ?? "system",
                    cancellationToken);

                lock (importedIds)
                {
                    importedIds.Add(lcscId);
                }
            }
            catch (Exception ex)
            {
                lock (errors)
                {
                    errors.Add($"{lcscId}: {ex.Message}");
                }

                if (!input.ContinueOnError)
                {
                    throw;
                }
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        try
        {
            await Task.WhenAll(tasks);
        }
        catch when (input.ContinueOnError)
        {
            // Errors are already collected above.
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Batch import stopped: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = $"Batch import finished. Imported {importedIds.Count} item(s).";
        if (errors.Count > 0)
        {
            TempData["ErrorMessage"] = string.Join(Environment.NewLine, errors.Take(5));
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Asset(long id)
    {
        var asset = await _dbContext.ExternalComponentAssets.FirstOrDefaultAsync(x => x.Id == id);
        if (asset is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(asset.Url) && string.IsNullOrWhiteSpace(asset.StoragePath))
        {
            return Redirect(asset.Url);
        }

        if (string.IsNullOrWhiteSpace(asset.StoragePath))
        {
            return NotFound();
        }

        var basePath = Path.IsPathRooted(_options.StorageRoot ?? string.Empty)
            ? _options.StorageRoot!
            : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), _options.StorageRoot ?? "App_Data/ExternalImports"));
        var fullPath = Path.GetFullPath(Path.Combine(basePath, asset.StoragePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!fullPath.StartsWith(Path.GetFullPath(basePath), StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest();
        }

        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }

        return PhysicalFile(fullPath, asset.ContentType ?? "application/octet-stream", asset.OriginalFileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Librarian,EEReviewer")]
    public async Task<IActionResult> Reject(long id)
    {
        await _externalImportService.RejectImportAsync(id, User.Identity?.Name ?? "system");
        TempData["SuccessMessage"] = "External import rejected.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
