using System.Text.Json;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Web.Controllers;

[Authorize(Roles = "Admin,Librarian,EEReviewer,Designer")]
[Route("AiIntake")]
public sealed class AiIntakeController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMcpLibraryWorkflowService _workflowService;
    private readonly IAiDatasheetExtractionService _aiExtractionService;
    private readonly IDatasheetTextExtractor _datasheetTextExtractor;

    public AiIntakeController(
        ApplicationDbContext dbContext,
        IMcpLibraryWorkflowService workflowService,
        IAiDatasheetExtractionService aiExtractionService,
        IDatasheetTextExtractor datasheetTextExtractor)
    {
        _dbContext = dbContext;
        _workflowService = workflowService;
        _aiExtractionService = aiExtractionService;
        _datasheetTextExtractor = datasheetTextExtractor;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(AiDatasheetExtractionStatus? status, int page = 1, int pageSize = 20)
    {
        var query = _dbContext.AiDatasheetExtractions.AsNoTracking().AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AiIntakeListItemViewModel
            {
                Id = x.Id,
                Manufacturer = x.Manufacturer,
                ManufacturerPartNumber = x.ManufacturerPartNumber,
                Confidence = x.Confidence,
                Status = x.Status,
                CandidateId = x.CandidateId,
                ExternalImportId = x.ExternalImportId,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        return View(new AiIntakeIndexViewModel
        {
            Status = status,
            Result = new PagedResult<AiIntakeListItemViewModel>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            }
        });
    }

    [HttpGet("CreateFromExternalImport/{externalImportId:long}")]
    public async Task<IActionResult> CreateFromExternalImport(long externalImportId)
    {
        var externalImport = await _dbContext.ExternalComponentImports
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == externalImportId);

        if (externalImport is null)
        {
            return NotFound();
        }

        return View(new AiIntakeCreateViewModel
        {
            ExternalImportId = externalImportId,
            Manufacturer = externalImport.Manufacturer,
            ManufacturerPartNumber = externalImport.ManufacturerPN ?? externalImport.Name,
            DatasheetUrlOrPath = externalImport.DatasheetUrl ?? externalImport.ManualUrl,
            ExtractionJson = BuildExtractionDraftJson(externalImport),
            SymbolSpecJson = BuildSymbolSpecDraftJson(externalImport),
            FootprintSpecJson = BuildFootprintSpecDraftJson(externalImport)
        });
    }

    [HttpPost("CreateFromExternalImport/{externalImportId:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromExternalImport(long externalImportId, AiIntakeCreateViewModel model)
    {
        var externalImport = await _dbContext.ExternalComponentImports
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == externalImportId);

        if (externalImport is null)
        {
            return NotFound();
        }

        ValidateJson(model.ExtractionJson, nameof(model.ExtractionJson));
        ValidateJson(model.SymbolSpecJson, nameof(model.SymbolSpecJson));
        ValidateJson(model.FootprintSpecJson, nameof(model.FootprintSpecJson));

        if (!ModelState.IsValid)
        {
            model.Manufacturer ??= externalImport.Manufacturer;
            model.ManufacturerPartNumber ??= externalImport.ManufacturerPN ?? externalImport.Name;
            model.DatasheetUrlOrPath ??= externalImport.DatasheetUrl ?? externalImport.ManualUrl;
            return View(model);
        }

        var result = await _workflowService.CreateExtractionDraftAsync(
            new Application.DTOs.DatasheetCreateExtractionDraftRequest(
                externalImport.CandidateId,
                externalImport.Id,
                model.DatasheetUrlOrPath,
                model.ExtractionJson,
                model.SymbolSpecJson,
                model.FootprintSpecJson));

        TempData["SuccessMessage"] = $"AI extraction draft #{result.ExtractionId} created.";
        return RedirectToAction(nameof(Details), new { id = result.ExtractionId });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Details(long id)
    {
        var extraction = await _dbContext.AiDatasheetExtractions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (extraction is null)
        {
            return NotFound();
        }

        var evidenceItems = await _dbContext.AiExtractionEvidenceItems
            .AsNoTracking()
            .Where(x => x.AiDatasheetExtractionId == id)
            .OrderBy(x => x.FieldPath)
            .ThenBy(x => x.SourcePage)
            .ToListAsync();

        var jobs = await _dbContext.CadenceBuildJobs
            .AsNoTracking()
            .Include(x => x.Artifacts)
            .Where(x => x.AiDatasheetExtractionId == id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        ExternalComponentImport? externalImport = null;
        OnlineCandidate? candidate = null;

        if (extraction.ExternalImportId.HasValue)
        {
            externalImport = await _dbContext.ExternalComponentImports
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == extraction.ExternalImportId.Value);
        }

        if (extraction.CandidateId.HasValue)
        {
            candidate = await _dbContext.OnlineCandidates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == extraction.CandidateId.Value);
        }

        var canApprove = User.IsInRole("Admin") || User.IsInRole("Librarian");
        return View(new AiIntakeDetailsViewModel
        {
            Extraction = extraction,
            ExternalImport = externalImport,
            Candidate = candidate,
            EvidenceItems = evidenceItems,
            Jobs = jobs,
            CanApproveForBuild = canApprove,
            CanBuildSymbol = canApprove && extraction.Status == AiDatasheetExtractionStatus.ApprovedForBuild,
            CanBuildFootprint = canApprove && extraction.Status == AiDatasheetExtractionStatus.ApprovedForBuild
        });
    }

    [HttpGet("{id:long}/Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var extraction = await _dbContext.AiDatasheetExtractions.FirstOrDefaultAsync(x => x.Id == id);
        if (extraction is null)
        {
            return NotFound();
        }

        return View(new AiIntakeEditViewModel
        {
            Id = extraction.Id,
            Manufacturer = extraction.Manufacturer,
            ManufacturerPartNumber = extraction.ManufacturerPartNumber,
            ExtractionJson = extraction.ExtractionJson,
            SymbolSpecJson = extraction.SymbolSpecJson,
            FootprintSpecJson = extraction.FootprintSpecJson,
            Status = extraction.Status
        });
    }

    [HttpPost("{id:long}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, AiIntakeEditViewModel model)
    {
        var extraction = await _dbContext.AiDatasheetExtractions.FirstOrDefaultAsync(x => x.Id == id);
        if (extraction is null)
        {
            return NotFound();
        }

        ValidateJson(model.ExtractionJson, nameof(model.ExtractionJson));
        ValidateJson(model.SymbolSpecJson, nameof(model.SymbolSpecJson));
        ValidateJson(model.FootprintSpecJson, nameof(model.FootprintSpecJson));

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        extraction.ExtractionJson = model.ExtractionJson;
        extraction.SymbolSpecJson = model.SymbolSpecJson;
        extraction.FootprintSpecJson = model.FootprintSpecJson;
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "AI extraction updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:long}/ApproveForBuild")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveForBuild(long id)
    {
        if (!(User.IsInRole("Admin") || User.IsInRole("Librarian")))
        {
            return Forbid();
        }

        await _workflowService.ApproveForBuildAsync(id);
        TempData["SuccessMessage"] = "Extraction approved for build.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:long}/RunExtraction")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunExtraction(long id, CancellationToken cancellationToken)
    {
        var extraction = await _dbContext.AiDatasheetExtractions
            .Include(x => x.EvidenceItems)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (extraction is null)
        {
            return NotFound();
        }

        var textResult = await _datasheetTextExtractor.ExtractTextAsync(
            new Application.DTOs.DatasheetTextExtractionRequest(extraction.Id, extraction.DatasheetAssetPath),
            cancellationToken);

        var result = await _aiExtractionService.RunExtractionAsync(
            new Application.DTOs.AiDatasheetExtractionRunRequest(
                extraction.Id,
                extraction.Manufacturer,
                extraction.ManufacturerPartNumber,
                extraction.DatasheetAssetPath,
                textResult.ExtractedText,
                extraction.ExtractionJson,
                extraction.SymbolSpecJson,
                extraction.FootprintSpecJson),
            cancellationToken);

        if (result.ValidationErrors.Count > 0)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.ValidationErrors);
            if (textResult.Warnings.Count > 0)
            {
                TempData["WarningMessage"] = string.Join(" ", textResult.Warnings);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        extraction.ExtractionJson = result.ExtractionJson;
        extraction.SymbolSpecJson = result.SymbolSpecJson;
        extraction.FootprintSpecJson = result.FootprintSpecJson;
        extraction.Confidence = result.Confidence;
        extraction.Status = result.Status;

        _dbContext.AiExtractionEvidenceItems.RemoveRange(extraction.EvidenceItems);
        extraction.EvidenceItems = result.Evidence
            .Select(x => new AiExtractionEvidence
            {
                AiDatasheetExtractionId = extraction.Id,
                FieldPath = x.FieldPath,
                ValueText = x.ValueText,
                Unit = x.Unit,
                SourcePage = x.SourcePage,
                SourceTable = x.SourceTable,
                SourceFigure = x.SourceFigure,
                Confidence = x.Confidence,
                ReviewerDecision = x.ReviewerDecision,
                ReviewerNote = x.ReviewerNote
            })
            .ToList();

        await _dbContext.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = $"AI extraction completed with confidence {result.Confidence:0.00}.";
        var allWarnings = textResult.Warnings.Concat(result.Warnings).ToList();
        if (allWarnings.Count > 0)
        {
            TempData["WarningMessage"] = string.Join(" ", allWarnings);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:long}/BuildSymbol")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BuildSymbol(long id)
    {
        if (!(User.IsInRole("Admin") || User.IsInRole("Librarian")))
        {
            return Forbid();
        }

        var result = await _workflowService.EnqueueCaptureSymbolJobAsync(new Application.DTOs.CadenceEnqueueJobRequest(id));
        TempData["SuccessMessage"] = $"Capture symbol job #{result.JobId} queued.";
        return RedirectToAction(nameof(Jobs), new { id });
    }

    [HttpPost("{id:long}/BuildFootprint")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BuildFootprint(long id)
    {
        if (!(User.IsInRole("Admin") || User.IsInRole("Librarian")))
        {
            return Forbid();
        }

        var result = await _workflowService.EnqueueAllegroFootprintJobAsync(new Application.DTOs.CadenceEnqueueJobRequest(id));
        TempData["SuccessMessage"] = $"Allegro footprint job #{result.JobId} queued.";
        return RedirectToAction(nameof(Jobs), new { id });
    }

    [HttpGet("{id:long}/Jobs")]
    public async Task<IActionResult> Jobs(long id)
    {
        var extraction = await _dbContext.AiDatasheetExtractions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (extraction is null)
        {
            return NotFound();
        }

        var jobs = await _dbContext.CadenceBuildJobs
            .AsNoTracking()
            .Include(x => x.Artifacts)
            .Where(x => x.AiDatasheetExtractionId == id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return View(new AiIntakeJobsViewModel
        {
            Extraction = extraction,
            Jobs = jobs
        });
    }

    [HttpGet("{id:long}/Verification")]
    public async Task<IActionResult> Verification(long id)
    {
        var extraction = await _dbContext.AiDatasheetExtractions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (extraction is null)
        {
            return NotFound();
        }

        var report = await _dbContext.LibraryVerificationReports
            .AsNoTracking()
            .Where(x => x.AiDatasheetExtractionId == id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();

        return View(new AiIntakeVerificationViewModel
        {
            Extraction = extraction,
            Report = report
        });
    }

    private void ValidateJson(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            ModelState.AddModelError(fieldName, "JSON is required.");
            return;
        }

        try
        {
            using var _ = JsonDocument.Parse(value);
        }
        catch (JsonException ex)
        {
            ModelState.AddModelError(fieldName, $"Invalid JSON: {ex.Message}");
        }
    }

    private static string BuildExtractionDraftJson(ExternalComponentImport import)
    {
        return JsonSerializer.Serialize(new
        {
            source = import.SourceName,
            importId = import.Id,
            manufacturer = import.Manufacturer,
            manufacturerPartNumber = import.ManufacturerPN ?? import.Name,
            description = import.Description,
            datasheetUrl = import.DatasheetUrl,
            packageName = import.PackageName,
            notes = "Draft generated from External Import. Review before build."
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string BuildSymbolSpecDraftJson(ExternalComponentImport import)
    {
        return JsonSerializer.Serialize(new
        {
            symbolName = import.SymbolName ?? import.Name,
            symbolShapeJson = import.SymbolShapeJson,
            symbolRawJson = import.SymbolRawJson,
            boundingBox = new
            {
                x = import.SymbolBBoxX,
                y = import.SymbolBBoxY
            }
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string BuildFootprintSpecDraftJson(ExternalComponentImport import)
    {
        return JsonSerializer.Serialize(new
        {
            footprintName = import.FootprintName ?? import.PackageName,
            packageName = import.PackageName,
            footprintShapeJson = import.FootprintShapeJson,
            footprintRawJson = import.FootprintRawJson,
            boundingBox = new
            {
                x = import.FootprintBBoxX,
                y = import.FootprintBBoxY
            },
            model3D = new
            {
                uuid = import.Model3DUuid,
                title = import.Model3DName
            }
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
