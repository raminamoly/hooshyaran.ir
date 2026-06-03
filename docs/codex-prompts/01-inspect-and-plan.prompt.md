# Prompt 01 — Inspect Repositories and Create Deep Plan

```text
You are working on the Hooshyaran.ir website project.

Target repo:
https://github.com/raminamoly/hooshyaran.ir

Reference repo:
https://github.com/raminamoly/PortalCmsLite

Do not write application code in this step.
Do not create the Visual Studio project yet.
This step is for inspection and planning only.

Goal:
Create a deep implementation plan for building Hooshyaran.ir as a Persian RTL CMS-lite product catalog for enterprise AI products.

First inspect:
1. The current target repo structure.
2. The PortalCmsLite repo structure.
3. Existing docs inside docs/brainstorming and docs/codex-prompts.
4. Existing assets in assets/brand and assets/fonts.

Important:
Use PortalCmsLite as a reference only where it is useful. If it is empty or minimal, do not force dependency on it. Instead, create a clean plan for a lightweight ASP.NET Core CMS-lite website.

Output a planning document at:
docs/implementation/01-deep-plan.md

The plan must include:
1. Recommended Visual Studio solution structure.
2. Recommended project type: ASP.NET Core MVC, Razor Pages, or Blazor, and why.
3. Recommended database provider for development and production.
4. CMS-lite content model.
5. Page list and routing.
6. Product content plan for LLMOPS and WorkGraph-AI.
7. Blog/article plan for 5 full SEO articles.
8. Logo and font integration plan.
9. SEO plan.
10. Build and test strategy.
11. Risks and assumptions.

Preferred direction:
- ASP.NET Core website suitable for Visual Studio.
- Persian RTL.
- SEO-friendly server-rendered pages.
- Database-backed CMS-lite content.
- No admin panel in v1 unless very simple.
- Seed data for products and articles.

Do not reveal internal product logic or technical stack of LLMOPS and WorkGraph-AI in public content.

After finishing, report:
- What you found in PortalCmsLite.
- What you found in hooshyaran.ir.
- What you recommend.
- The path of the plan file created.
```
