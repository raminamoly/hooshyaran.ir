# Prompt 04 — Integrate Brand Logo and Fonts

```text
Continue from Prompt 03.

Before changing anything, read:
- docs/implementation/01-deep-plan.md
- docs/codex-prompts/00-master-plan.md
- assets/fonts/README.md

Goal:
Integrate the Hooshyaran visual identity into the web project.

Available brand assets in repo:
1. assets/brand/hooshyaran-rm-logo-concept.svg
2. assets/brand/logo-mo-1024.png.base64

The uploaded logo was an orange/green RM-style logo on black background. Use this visual identity carefully, but keep the website professional and enterprise-ready.

Tasks:
1. Copy or reference the SVG concept logo into:
   src/Hooshyaran.Web/wwwroot/assets/brand/hooshyaran-rm-logo.svg
2. Try to decode assets/brand/logo-mo-1024.png.base64 into:
   src/Hooshyaran.Web/wwwroot/assets/brand/logo-mo.png
   If the base64 file is not valid, do not block the project. Use the SVG logo and report the issue honestly.
3. Create brand CSS variables in the main stylesheet.
4. Add logo usage in header and footer.
5. Add favicon placeholder or simple SVG favicon based on the brand colors.
6. Add font-family strategy.

Font requirements:
The user uploaded AnjomanFaNum WOFF files, but do not assume license status.

Use this CSS stack:
font-family: 'AnjomanFaNum', 'Vazirmatn', Tahoma, system-ui, sans-serif;

Only add @font-face for AnjomanFaNum if the WOFF files exist inside the project and are legally allowed to be used. If not, use a fallback and document how to add the font later.

Visual style:
- Persian RTL enterprise SaaS style.
- Dark navy / black hero is allowed because the logo has black background.
- Use orange and green only as accents, not as overwhelming colors.
- Maintain readability and accessibility.

Suggested CSS variables:
--color-primary: #213448;
--color-secondary: #547792;
--color-accent: #00B8A9;
--color-orange: #F28A00;
--color-green: #22C000;
--color-bg: #F7FAFC;
--color-text: #1F2937;
--color-muted: #6B7280;

Do not create full website pages yet.
Focus only on brand asset integration and base layout styling support.

After changes:
1. Run build.
2. Confirm logo renders in the temporary layout.
3. Report how fonts are handled.
4. Report all changed files.
```
