using System.Text;
using CadenceComponentLibraryAdmin.Application.DTOs;
using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CadenceComponentLibraryAdmin.Web.Controllers;

[Authorize]
public sealed class ChangeLogsController : Controller
{
    private readonly IChangeLogService _changeLogService;

    public ChangeLogsController(IChangeLogService changeLogService)
    {
        _changeLogService = changeLogService;
    }

    public async Task<IActionResult> Index(string? companyPn, ChangeType? changeType, DateTime? changedFrom, DateTime? changedTo, int page = 1, int pageSize = 20)
    {
        var query = new ChangeLogQuery
        {
            CompanyPN = companyPn,
            ChangeType = changeType,
            ChangedFrom = changedFrom,
            ChangedTo = changedTo
        };

        var items = await _changeLogService.QueryAsync(query);
        ViewBag.Query = query;
        return View(new PagedResult<CadenceComponentLibraryAdmin.Domain.Entities.PartChangeLog>
        {
            Items = items.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = items.Count
        });
    }

    public async Task<IActionResult> Export(string? companyPn, ChangeType? changeType, DateTime? changedFrom, DateTime? changedTo)
    {
        var query = new ChangeLogQuery
        {
            CompanyPN = companyPn,
            ChangeType = changeType,
            ChangedFrom = changedFrom,
            ChangedTo = changedTo
        };

        var items = await _changeLogService.QueryAsync(query);
        var lines = new List<string>
        {
            "CompanyPN,ChangeType,OldValue,NewValue,Reason,ChangedBy,ChangedAt,ReleaseName"
        };

        foreach (var item in items)
        {
            lines.Add(string.Join(",",
                EscapeCsv(item.CompanyPN),
                EscapeCsv(item.ChangeType.ToString()),
                EscapeCsv(item.OldValue),
                EscapeCsv(item.NewValue),
                EscapeCsv(item.Reason),
                EscapeCsv(item.ChangedBy),
                EscapeCsv(item.ChangedAt.ToString("yyyy-MM-dd HH:mm:ss")),
                EscapeCsv(item.ReleaseName)));
        }

        return File(Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines)), "text/csv", $"ChangeLogs_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
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
