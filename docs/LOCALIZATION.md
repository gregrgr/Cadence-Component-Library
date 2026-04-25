# Localization

The Web application supports English and Simplified Chinese UI culture switching.

## Supported cultures

- `en-US`
- `zh-CN`

English remains the default culture. The language switcher in the page header posts to `LocalizationController.SetLanguage`, stores the selected culture in the standard ASP.NET Core request-culture cookie, and redirects back to the current local URL.

## Resource files

Shared layout and navigation strings use:

- `src/CadenceComponentLibraryAdmin.Web/SharedResource.cs`
- `src/CadenceComponentLibraryAdmin.Web/Resources/SharedResource.zh-CN.resx`

Add page-specific resources as pages are translated. Keep domain statuses, approval gates, and workflow state names explicit and consistent with the existing business rules.

## Rules

- Do not localize by hard-coding culture checks in controllers.
- Prefer `IStringLocalizer<SharedResource>` for shared UI strings.
- Keep server-side authorization and validation messages authoritative even when UI text is translated.
- Do not translate persisted enum/string values in the database; translate only display text.
