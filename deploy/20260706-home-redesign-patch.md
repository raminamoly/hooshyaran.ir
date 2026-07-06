# Home redesign server patch

Date:
2026-07-06

Scope:
Home page conversion and clarity improvements.

Production URL:
https://hooshyaran.ir

No database patch is required.

Files expected in the server patch:

- Hooshyaran.Web.dll
- Hooshyaran.Web.exe
- Hooshyaran.Web.deps.json
- Hooshyaran.Web.runtimeconfig.json
- Hooshyaran.Web.staticwebassets.endpoints.json
- wwwroot/css/site.css
- wwwroot/js/site.js
- wwwroot/uploads/media/imported/content/llmops-private-ai-architecture-hero-v2-thumb.webp
- wwwroot/uploads/media/imported/content/employee-monitor-ai-dashboard-thumb.webp
- wwwroot/uploads/media/imported/content/complaint-management-ai-dashboard-thumb.webp
- wwwroot/uploads/media/imported/content/ai-bi-natural-manager-hero-final-thumb.webp

Deploy steps:

1. Stop the IIS site or application pool.
2. Back up the current site folder.
3. Copy the patch files over the existing site folder.
4. Do not overwrite the production `appsettings.json` from any local source.
5. Start the application pool.
6. Run the smoke checklist below.

Smoke checklist:

- https://hooshyaran.ir/
- https://hooshyaran.ir/products
- https://hooshyaran.ir/blog
- https://hooshyaran.ir/ai-integrator
- https://hooshyaran.ir/request-demo
- https://hooshyaran.ir/robots.txt
- https://hooshyaran.ir/sitemap.xml

Expected result:

- Main routes return 200.
- Home page has no horizontal overflow on desktop or 375px mobile.
- Home page images return 200.
- The primary home hero message mentions the enterprise AI platform.
- The secondary home hero CTA points to `/ai-integrator`.
