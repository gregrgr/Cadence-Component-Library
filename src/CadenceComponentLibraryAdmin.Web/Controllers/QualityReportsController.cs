using CadenceComponentLibraryAdmin.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CadenceComponentLibraryAdmin.Web.Controllers;

[Authorize]
public sealed class QualityReportsController : Controller
{
    private readonly IQualityReportService _qualityReportService;

    public QualityReportsController(IQualityReportService qualityReportService)
    {
        _qualityReportService = qualityReportService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _qualityReportService.BuildSummaryAsync();
        return View(model);
    }

    public async Task<IActionResult> Export(string code)
    {
        var summary = await _qualityReportService.BuildSummaryAsync();
        var section = summary.Sections.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));

        if (section is null)
        {
            return NotFound();
        }

        var lines = new List<string>
        {
            "PrimaryKey,Title,Detail"
        };

        foreach (var item in section.Items)
        {
            lines.Add(string.Join(",",
                EscapeCsv(item.PrimaryKey),
                EscapeCsv(item.Title),
                EscapeCsv(item.Detail)));
        }

        var content = string.Join(Environment.NewLine, lines);
        var fileName = $"{section.Code}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(System.Text.Encoding.UTF8.GetBytes(content), "text/csv", fileName);
    }

    private static string EscapeCsv(string? value)
    {
        var text = value ?? string.Empty;
        if (text.Contains(',') || text.Contains('"') || text.Contains('\n') || text.Contains('\r'))
        {
            return $"\"{text.Replace("\"", "\"\"")}\"";
        }

        return text;
    }
}
