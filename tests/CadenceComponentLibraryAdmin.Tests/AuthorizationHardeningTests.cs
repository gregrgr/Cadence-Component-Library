using CadenceComponentLibraryAdmin.Infrastructure.Services;
using CadenceComponentLibraryAdmin.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class AuthorizationHardeningTests
{
    [Fact]
    public async Task Designer_CannotApproveApprovalQueueItem()
    {
        await using var dbContext = IdentityManagementTestHelper.CreateDbContext();
        var controller = new ApprovalQueueController(
            dbContext,
            new CompanyPartService(dbContext, new NoOpChangeLogService()),
            new NoOpChangeLogService());
        IdentityManagementTestHelper.AttachControllerContext(controller, "designer-id", "designer@test.local", "Designer");

        var result = await controller.ApproveCompanyPart(123);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Viewer_CannotMutatePackageFamily()
    {
        await using var dbContext = IdentityManagementTestHelper.CreateDbContext();
        var controller = new PackageFamiliesController(dbContext, new PackageFamilyService(dbContext));
        IdentityManagementTestHelper.AttachControllerContext(controller, "viewer-id", "viewer@test.local", "Viewer");

        var result = await controller.Create(new CadenceComponentLibraryAdmin.Domain.Entities.PackageFamily());

        Assert.IsType<ForbidResult>(result);
    }
}
