# Hooshyaran.ir — Master Plan for 10 Sequential Codex Prompts

This folder contains a step-by-step Codex workflow for building **Hooshyaran.ir** as a Persian RTL CMS-lite product catalog for enterprise AI solutions.

## Core Strategy

Hooshyaran.ir should be implemented as a **Persian RTL CMS-lite product catalog**, using `PortalCmsLite` as the reference where possible, but not depending on it blindly if the repo is empty or minimal.

The website should not be a single hardcoded landing page. It should be a small, extendable portal where new products, articles, product categories, use cases, and solution pages can be added later.

## Brand Direction

Brand name:

```text
هوش‌یاران
```

Main positioning:

```text
هوش‌یاران؛ کاتالوگ راهکارهای هوش مصنوعی سازمانی برای کسب‌وکارها و نهادهای بزرگ
```

Main homepage headline:

```text
هوش مصنوعی سازمانی، امن و قابل کنترل
```

Main CTA:

```text
درخواست جلسه معرفی
```

## Important Public-Content Rule

This is a public marketing website. Do not reveal:

- Internal product business logic
- Source code internals
- Exact infrastructure details
- Docker/YAML/server details
- Algorithms
- Prompt logic
- Secrets or environment variables
- Security-sensitive architecture
- Private implementation details
- Fake customer names or unsupported claims

Use only public marketing language.

## Uploaded Brand Assets

The uploaded logo is represented in this repo as:

```text
assets/brand/logo-mo-1024.png.base64
```

Codex should decode it into:

```text
src/Hooshyaran.Web/wwwroot/assets/brand/logo-mo.png
```

Font package uploaded by the user contains AnjomanFaNum WOFF files. Do not assume license status. Use the font only if available and legally allowed in the working environment. Otherwise, use a safe Persian fallback such as `Vazirmatn`, `Tahoma`, or `system-ui`.

## Recommended Project Approach

The preferred implementation is an ASP.NET Core website that can be opened and maintained in Visual Studio.

Suggested structure:

```text
Hooshyaran.sln
src/
  Hooshyaran.Web/
    Controllers or Pages
    Views or Razor pages
    wwwroot/
      assets/
      css/
      js/
    Data/
    Models/
    ViewModels/
    Services/
    Content/
```

Use .NET 10 if installed. If not, use .NET 8 LTS and clearly report the decision.

## CMS-lite Data Model

Minimum content entities:

- ProductCategory
- Product
- BlogArticle
- PageContent or StaticPage
- FaqItem
- CtaBlock
- SeoMetadata

The first implementation can seed content into the database. Admin panel is optional for later, not mandatory for the first launch.

## Sequential Prompt Files

Run these prompts one by one, in order:

1. `01-inspect-and-plan.prompt.md`
2. `02-create-visual-studio-project.prompt.md`
3. `03-create-database-and-content-model.prompt.md`
4. `04-integrate-brand-logo-and-fonts.prompt.md`
5. `05-create-cms-lite-content-seed.prompt.md`
6. `06-create-layout-navigation-and-rtl-ui.prompt.md`
7. `07-create-home-and-product-listing-pages.prompt.md`
8. `08-create-product-detail-pages-and-visuals.prompt.md`
9. `09-create-five-full-seo-articles.prompt.md`
10. `10-final-seo-accessibility-build-report.prompt.md`

## Rule for Codex

After each prompt:

1. Make only the requested changes.
2. Build the project if possible.
3. Report changed files.
4. Report any errors honestly.
5. Do not continue to the next step unless the current step is stable.
