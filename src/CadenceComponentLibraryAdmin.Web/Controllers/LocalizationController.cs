using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace CadenceComponentLibraryAdmin.Web.Controllers;

public sealed class LocalizationController : Controller
{
    private static readonly HashSet<string> SupportedCultures = new(StringComparer.OrdinalIgnoreCase)
    {
        "en-US",
        "zh-CN"
    };

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        if (!SupportedCultures.Contains(culture))
        {
            culture = "en-US";
        }

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });

        return LocalRedirect(IsSafeLocalUrl(returnUrl) ? returnUrl : "/");
    }

    private static bool IsSafeLocalUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return false;
        }

        return returnUrl[0] == '/'
            && (returnUrl.Length == 1 || (returnUrl[1] != '/' && returnUrl[1] != '\\'));
    }
}
