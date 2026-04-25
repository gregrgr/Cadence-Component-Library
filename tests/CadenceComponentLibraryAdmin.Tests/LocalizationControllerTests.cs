using CadenceComponentLibraryAdmin.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class LocalizationControllerTests
{
    [Fact]
    public void SetLanguage_StoresSelectedCultureCookie()
    {
        var controller = CreateController();

        var result = controller.SetLanguage("zh-CN", "/CompanyParts");

        var redirect = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("/CompanyParts", redirect.Url);
        var setCookie = controller.Response.Headers.SetCookie.ToString();
        Assert.Contains(CookieRequestCultureProvider.DefaultCookieName, setCookie);
        Assert.Contains("c%3Dzh-CN%7Cuic%3Dzh-CN", setCookie);
    }

    [Fact]
    public void SetLanguage_RejectsExternalReturnUrl()
    {
        var controller = CreateController();

        var result = controller.SetLanguage("zh-CN", "https://example.com/phish");

        var redirect = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("/", redirect.Url);
    }

    [Fact]
    public void SetLanguage_UnsupportedCultureFallsBackToEnglish()
    {
        var controller = CreateController();

        controller.SetLanguage("fr-FR", "/");

        var setCookie = controller.Response.Headers.SetCookie.ToString();
        Assert.Contains("c%3Den-US%7Cuic%3Den-US", setCookie);
    }

    private static LocalizationController CreateController()
    {
        var controller = new LocalizationController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return controller;
    }
}
