using System.Security.Claims;
using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using CadenceComponentLibraryAdmin.Web.Controllers;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class ApprovalQueueControllerTests
{
    [Fact]
    public async Task Index_ReturnsPendingCompanyParts()
    {
        await using var dbContext = CreateDbContext();
        SeedCompanyPart(dbContext, "CP-PENDING", ApprovalStatus.PendingReview);
        SeedCompanyPart(dbContext, "CP-APPROVED", ApprovalStatus.Approved);

        var controller = CreateController(dbContext);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ApprovalQueueViewModel>(viewResult.Model);
        var pendingCompanyPart = Assert.Single(model.PendingCompanyParts);
        Assert.Equal("CP-PENDING", pendingCompanyPart.CompanyPN);
    }

    private static ApprovalQueueController CreateController(ApplicationDbContext dbContext)
    {
        var controller = new ApprovalQueueController(
            dbContext,
            new CompanyPartService(dbContext, new NoOpChangeLogService()),
            new NoOpChangeLogService());

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, "reviewer@local.test"),
                new Claim(ClaimTypes.Role, "Librarian")
            ], "TestAuth"))
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        controller.TempData = new TempDataDictionary(httpContext, new TestTempDataProvider());
        return controller;
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static void SeedCompanyPart(ApplicationDbContext dbContext, string companyPn, ApprovalStatus approvalStatus)
    {
        dbContext.FootprintVariants.Add(new FootprintVariant
        {
            FootprintName = $"FPT-{companyPn}",
            PackageFamilyCode = "PKG",
            PsmPath = $"Footprints/{companyPn}.psm",
            DraPath = $"Footprints/{companyPn}.dra",
            VariantType = "Production",
            Status = FootprintStatus.Released
        });

        dbContext.CompanyParts.Add(new CompanyPart
        {
            CompanyPN = companyPn,
            PartClass = "Passive",
            Description = companyPn,
            SymbolFamilyCode = "SYM",
            PackageFamilyCode = "PKG",
            DefaultFootprintName = $"FPT-{companyPn}",
            ApprovalStatus = approvalStatus,
            LifecycleStatus = LifecycleStatus.Active
        });

        dbContext.SaveChanges();
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
