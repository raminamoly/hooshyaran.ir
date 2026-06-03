# Prompt 02 — Create Visual Studio Solution and Web Project

```text
Continue from Prompt 01.

Before changing anything, read:
docs/implementation/01-deep-plan.md

Goal:
Create the initial Visual Studio-compatible solution and ASP.NET Core website project for Hooshyaran.ir.

Requirements:
1. Create a solution named Hooshyaran.sln.
2. Create the main web project under:
   src/Hooshyaran.Web
3. Prefer ASP.NET Core MVC or Razor Pages for SEO-friendly server-rendered Persian pages.
4. Use .NET 10 if installed. If .NET 10 is not available, use .NET 8 LTS and explain why.
5. The project must be easy to open in Visual Studio.
6. Add clean folder structure for a CMS-lite product catalog.

Suggested folders inside Hooshyaran.Web:
- Controllers or Pages
- Views if MVC is used
- Models
- ViewModels
- Data
- Services
- Content
- wwwroot/assets
- wwwroot/css
- wwwroot/js

Initial technical requirements:
- Enable static files.
- Add Persian RTL layout foundation.
- Add environment-aware configuration.
- Add appsettings.json and appsettings.Development.json.
- Add basic error handling.
- Add a basic Home route returning a temporary Persian page.

Do not implement full website pages yet.
Do not create database models yet unless required for compilation.
Do not add admin panel yet.

After changes:
1. Run restore/build if possible.
2. Fix compile errors.
3. Report created files.
4. Report how to open and run the solution in Visual Studio.
```
