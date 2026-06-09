# Hooshyaran.ir Deep Implementation Plan

## Inspection Summary

### Target Repository: hooshyaran.ir

The target repository currently contains planning documents, Codex prompt files, and brand/font guidance. It does not yet contain a Visual Studio solution, ASP.NET Core project, database layer, or runnable website.

Existing useful assets:

- `docs/brainstorming/01-website-strategy-fa.md` defines the Persian B2B content strategy.
- `docs/codex-prompts/00-master-plan.md` defines the sequential implementation workflow.
- `docs/codex-prompts/00-uiux-theme-standard.md` defines the visual direction.
- `assets/brand/hooshyaran-rm-logo-concept.svg` is a usable SVG logo concept.
- `assets/brand/logo-mo-1024.png.base64` exists, but should be decoded during brand integration and treated as optional if invalid.
- `assets/fonts/README.md` confirms AnjomanFaNum binaries are not committed and must not be assumed licensed.

### Reference Repository: PortalCmsLite

PortalCmsLite is a small Persian RTL ASP.NET Core reference for public marketing websites. It is useful as an architectural direction, not as a complete codebase to copy blindly.

Useful reference decisions:

- ASP.NET Core 10.
- Razor Pages.
- One web project only.
- SQLite with Entity Framework Core.
- Server-rendered pages for SEO.
- Persian RTL-first UI.
- File-based navigation/profile/static content ideas.
- SQLite-backed articles and lead/contact submissions.

Hooshyaran.ir should reuse the spirit of PortalCmsLite: simple, server-rendered, content-driven, SEO-friendly, and easy to maintain in Visual Studio.

## Recommended Project Type

Use **ASP.NET Core Razor Pages**.

Reasons:

- Razor Pages are server-rendered and SEO-friendly.
- They are simpler than MVC for a small marketing/product catalog portal.
- They match the PortalCmsLite reference direction.
- They are easy to open, run, and maintain in Visual Studio.
- Page-focused routing fits the public pages: home, products, product detail, blog, about, contact.

## Recommended Solution Structure

```text
Hooshyaran.sln
src/
  Hooshyaran.Web/
    Content/
    Data/
    Models/
    Pages/
      Shared/
      Products/
      Blog/
    Services/
    ViewModels/
    wwwroot/
      assets/
        brand/
        fonts/
      css/
      js/
```

Use one web project in v1. Do not add Clean Architecture, CQRS, microservices, or product application logic.

## Runtime and Framework

Use **.NET 10** if installed. If .NET 10 is not available in a later environment, use .NET 8 LTS and report the decision.

## Database Provider

Use **SQLite for development and v1 production**.

Reasons:

- Matches PortalCmsLite direction.
- Simple setup for a CMS-lite marketing website.
- Portable for Visual Studio development.
- Suitable for the expected initial content size.

If a future deployment requires SQL Server, the EF Core model should remain provider-friendly enough to migrate.

## CMS-lite Content Model

Minimum entities:

- `ProductCategory`
- `Product`
- `BlogArticle`
- `StaticPage`
- `FaqItem`
- `CtaBlock`
- `SeoMetadata`

Recommended supporting entity:

- `LeadSubmission` for contact/demo requests if contact form persistence is implemented in a later prompt.

Content rules:

- Store public marketing content only.
- Do not store source-code-level explanations, infrastructure details, deployment details, prompt logic, secrets, algorithms, or internal product behavior.
- Use stable slugs for all public content.

## Pages and Routing

Version 1 public routes:

```text
/
/products
/products/llmops
/products/workgraph-ai
/solutions
/use-cases
/blog
/about
/contact
/request-demo
```

Prompt 02 should only create a temporary home page and foundation.

Prompt 06 should create layout-level navigation and reusable UI components, without final page content beyond placeholders.

## Product Content Plan

### LLMOPS

Public positioning:

```text
LLMOPS | مدیریت سازمانی مدل‌های زبانی
```

Focus:

- مدیریت استفاده سازمانی از AI
- پایش مصرف و عملکرد
- کاهش استفاده پراکنده و بدون کنترل از ابزارهای هوش مصنوعی
- کمک به تیم‌های فناوری و AI
- آماده‌سازی سازمان برای بهره‌برداری گسترده‌تر از مدل‌های زبانی

### WorkGraph-AI

Public positioning:

```text
WorkGraph-AI | اتوماسیون هوشمند جریان‌های کاری
```

Focus:

- اتوماسیون هوشمند فرایندها
- کاهش کارهای تکراری
- افزایش هماهنگی بین تیم‌ها
- ترکیب تصمیم‌گیری انسانی و AI
- شفاف‌تر و قابل کنترل‌تر کردن فرایندهای سازمانی

## Blog Plan for 5 SEO Articles

Future Prompt 09 should create five complete Persian SEO articles:

1. هوش مصنوعی سازمانی چیست و چه کاربردی برای کسب‌وکارها دارد؟
2. تفاوت استفاده شخصی و سازمانی از هوش مصنوعی
3. چرا سازمان‌ها به مدیریت مدل‌های زبانی نیاز دارند؟
4. LLMOPS چیست و چرا برای سازمان‌ها مهم است؟
5. اتوماسیون هوشمند فرایندها با AI چگونه انجام می‌شود؟

Article pages should include title, summary, body, publication date, SEO title, SEO description, internal links, and Article structured data where appropriate.

## Logo and Font Integration Plan

Logo:

- Copy `assets/brand/hooshyaran-rm-logo-concept.svg` into `src/Hooshyaran.Web/wwwroot/assets/brand/hooshyaran-rm-logo.svg`.
- Try to decode `assets/brand/logo-mo-1024.png.base64` into `src/Hooshyaran.Web/wwwroot/assets/brand/logo-mo.png`.
- If decode fails, use the SVG logo and report the issue honestly.

Fonts:

- Use this CSS stack:

```css
font-family: 'AnjomanFaNum', 'Vazirmatn', Tahoma, system-ui, sans-serif;
```

- Do not add `@font-face` for AnjomanFaNum unless WOFF files exist in the project and the license allows use.
- The site must remain readable with the fallback stack.

## SEO Plan

Every important page should have:

- Persian title.
- Meta description.
- Canonical URL support.
- Open Graph basics.
- One clear `h1`.
- Logical heading hierarchy.
- SEO-friendly English slugs.
- Internal links to products, contact, and blog.

Structured data to consider in later prompts:

- `Organization`
- `WebSite`
- `Product`
- `BreadcrumbList`
- `Article`
- `FAQPage`

## Build and Test Strategy

After each prompt:

1. Restore packages.
2. Build the solution.
3. Fix compile errors before continuing.
4. Keep changes scoped to the active prompt.

Recommended commands:

```powershell
dotnet restore Hooshyaran.sln
dotnet build Hooshyaran.sln
dotnet run --project src/Hooshyaran.Web
```

Manual checks:

- Home page renders in Persian.
- `html` uses `lang="fa"` and `dir="rtl"`.
- Header and footer render the logo.
- Navigation works on desktop and mobile widths.
- Focus states are visible.
- Reduced motion is respected for decorative animation.

## Risks and Assumptions

- Prompt files `07` through `10` are listed in the master plan but are not currently present in the repository.
- The uploaded PNG base64 logo may be invalid and should not block implementation.
- AnjomanFaNum WOFF files are not committed; use fallback fonts unless legal font files are later added.
- No admin panel is planned for v1.
- Content must stay public and marketing-level only.
- The first implementation should prioritize a stable, extensible website foundation over advanced CMS features.

## Recommendation

Build Hooshyaran.ir as a single ASP.NET Core 10 Razor Pages project with SQLite and EF Core, seeded CMS-lite content, Persian RTL layout, brand assets, SEO metadata foundations, and reusable page components. Use PortalCmsLite as a reference for simplicity and direction, but create a clean project tailored to the Hooshyaran product catalog.
