# hooshyaran.ir

Persian RTL CMS-lite product catalog for Hooshyaran enterprise AI solutions.

## Project

- Framework: ASP.NET Core 10 Razor Pages
- Database: SQLite with Entity Framework Core
- UI: Persian RTL, server-rendered, SEO-friendly pages
- Assets: local fonts, local scripts, local images, no public CDN dependency
- Main project: `src/Hooshyaran.Web`
- Solution: `Hooshyaran.sln`

## Run Locally

```powershell
dotnet restore Hooshyaran.sln
dotnet build Hooshyaran.sln
dotnet run --project src/Hooshyaran.Web --urls http://127.0.0.1:5109
```

The app applies EF Core migrations and seeds CMS-lite public marketing content on startup.

## Admin Login

Development seed credentials:

```text
Username: admin
Password: Admin@12345!
```

Change the default password before production use.

## Current UI Notes

- Home hero keeps its image-led landing layout.
- All non-home page heroes use a shared text-only galaxy/network background with subtle CSS animation.
- The top navigation does not include the demo-request button; demo CTAs remain inside page hero/content sections.
- Header logo uses a small optimized local image.

## Performance Notes

- External runtime dependencies were removed from public pages.
- GSAP and online map embeds were removed from the public layout.
- Content PNGs have optimized JPG versions used by pages and seed content.
- Static assets and HTML responses include cache headers.
- A local service worker caches the app shell and core assets for repeat visits/offline resilience.
- Local measurement on `http://127.0.0.1:5109/`: first homepage load was about `232ms`; cached reloads were about `49-53ms` with assets served from cache.

## Prompt Status

Implemented the prompt files currently present in `docs/codex-prompts`:

1. Inspect and plan
2. Create Visual Studio project
3. Create database and CMS-lite model
4. Integrate brand logo and fonts
5. Create CMS-lite seed content
6. Create layout, navigation, and RTL UI foundation

The master plan references prompts 07-10, but those files are not currently present in the repository.
