# Codex Prompt: Hooshyaran.ir Persian RTL CMS-lite Product Catalog

```text
I have a domain and repository for a new website:

Domain:
https://hooshyaran.ir

Target repository:
https://github.com/raminamoly/hooshyaran.ir

Reference/template repository:
https://github.com/raminamoly/PortalCmsLite

Important context:
Hooshyaran.ir should be implemented as a Persian RTL CMS-lite product catalog for enterprise AI products.
Use PortalCmsLite as the reference where possible, but do not depend on it blindly. First inspect PortalCmsLite carefully. If it contains useful structure, layout, routing, components, styling, CMS/content patterns, or conventions, reuse and adapt them. If PortalCmsLite is empty, minimal, or incomplete, create a clean lightweight portal structure in hooshyaran.ir based on the same intended idea: reusable layout, content-driven pages, SEO metadata, RTL support, and easy future expansion.

Very important:
Do not start coding immediately. First inspect both repositories and create a short implementation plan. Then implement.

Business goal:
Hooshyaran.ir will be a Persian B2B product catalog website for enterprise AI solutions. The site should present AI products in a professional, trustworthy, SEO-friendly way for businesses, government organizations, CEOs, CTOs, CIOs, IT managers, and digital transformation teams.

Current products to include:

1. LLMOPS
Repository:
https://github.com/raminamoly/LLMOPS

2. WorkGraph-AI
Repository:
https://github.com/raminamoly/WorkGraph-AI

Confidentiality rule:
This is a public marketing website. Do not reveal internal product business logic, source code internals, exact infrastructure, algorithms, secrets, environment variables, deployment-sensitive details, private stack details, security-sensitive architecture, or private implementation details.

Use only marketing-level descriptions:
- What problem the product solves
- Who it is for
- Business value
- Public-facing features
- Enterprise use cases
- Benefits for managers and organizations
- Call-to-action text
- SEO title and meta description

Core website concept:
Hooshyaran.ir is not just a landing page. It should be a CMS-lite product catalog. The implementation should make it easy to add more products, solutions, use cases, industries, FAQs, and blog posts later without redesigning the whole website.

Recommended content model:
- Products
- Product categories
- Solutions
- Use cases
- Industries
- Blog posts / knowledge base articles
- FAQ items
- CTA blocks
- SEO metadata
- Navigation items
- Footer links

Minimum version 1 pages:
- Home page
- Products listing page
- Product detail page for LLMOPS
- Product detail page for WorkGraph-AI
- About page
- Contact / Request Demo page
- Simple Blog / Knowledge Base structure if suitable

Website requirements:
- Persian language
- RTL layout
- SEO-friendly
- Mobile responsive
- Professional B2B SaaS / enterprise visual style
- Clean, simple, extendable structure
- Content-driven where reasonable
- Reusable product cards
- Reusable layout sections
- Reusable CTA section
- Reusable SEO metadata pattern
- Good Persian typography
- No over-engineering

Suggested brand direction:
Brand name in Persian:
هوش‌یاران

Main positioning:
هوش‌یاران؛ کاتالوگ راهکارهای هوش مصنوعی سازمانی برای کسب‌وکارها و نهادهای بزرگ

Hero headline:
هوش مصنوعی سازمانی، امن و قابل کنترل

Hero subtitle:
محصولات هوش‌یاران به سازمان‌ها کمک می‌کنند مدل‌های زبانی، اتوماسیون هوشمند و دستیارهای AI را به‌صورت کاربردی، امن و قابل توسعه وارد فرایندهای کاری خود کنند.

Primary CTA:
درخواست جلسه معرفی

Secondary CTA:
مشاهده محصولات

Homepage sections:
1. Hero section
2. Enterprise AI pain points
3. Hooshyaran solution overview
4. Product catalog preview
5. Why Hooshyaran
6. Enterprise AI use cases
7. Industries / target organizations
8. Blog or knowledge preview if suitable
9. Final CTA
10. Footer

Product: LLMOPS
Persian title:
LLMOPS | مدیریت سازمانی مدل‌های زبانی

Short description:
راهکاری برای مدیریت، پایش و بهره‌برداری سازمانی از مدل‌های زبانی و سرویس‌های هوش مصنوعی.

Marketing focus:
- Managing AI usage in organizations
- Monitoring consumption and performance
- Reducing uncontrolled AI usage
- Helping IT and AI teams manage LLM-based services
- Enterprise readiness for AI operations

Do not expose technical stack or deployment details.

Product: WorkGraph-AI
Persian title:
WorkGraph-AI | اتوماسیون هوشمند جریان‌های کاری

Short description:
راهکاری برای طراحی، مدیریت و اجرای جریان‌های کاری هوشمند با کمک AI در سازمان‌ها.

Marketing focus:
- Smart workflow automation
- Reducing repetitive work
- Improving coordination between teams
- Human + AI decision support
- Making business processes more transparent and controllable

Do not expose internal graph logic, algorithms, source code, or technical internals.

SEO requirements:
Each important page should have:
- Persian page title
- Meta description
- SEO-friendly URL slug
- Heading hierarchy
- Internal links
- Product structured content if appropriate

Main SEO keywords:
- هوش مصنوعی سازمانی
- راهکار هوش مصنوعی برای سازمان‌ها
- هوش مصنوعی برای کسب و کار
- اتوماسیون هوشمند سازمانی
- دستیار هوش مصنوعی سازمانی
- مدیریت مدل‌های زبانی
- LLMOPS فارسی
- پیاده سازی هوش مصنوعی در سازمان
- راهکار AI برای شرکت‌ها
- هوش مصنوعی امن برای سازمان
- Enterprise AI
- LLMOps
- AI Workflow Automation

Suggested URL slugs:
/
/products
/products/llmops
/products/workgraph-ai
/solutions
/use-cases
/about
/contact
/blog

Design direction:
- Persian RTL SaaS / enterprise design
- Clean product cards
- Strong hero section
- Professional blue/teal technology palette
- Clear CTA buttons
- Trust-building sections
- Good spacing
- Mobile-first responsive behavior
- Avoid flashy consumer-style design

Suggested colors:
Primary: #213448
Secondary: #547792
Accent: #00B8A9
Light Background: #F7FAFC
Dark Text: #1F2937
Muted Text: #6B7280

Implementation approach:
1. Inspect PortalCmsLite first.
2. Inspect the current hooshyaran.ir repository.
3. Identify whether PortalCmsLite has usable structure.
4. If usable, follow its architecture and style conventions.
5. If minimal, create a clean CMS-lite portal structure from scratch in hooshyaran.ir.
6. Keep content separated from UI where practical.
7. Create reusable components/sections for product cards, CTA blocks, page headers, SEO metadata, and layout.
8. Add initial product data for LLMOPS and WorkGraph-AI.
9. Implement Persian RTL pages.
10. Add SEO metadata.
11. Ensure responsive design.
12. Run build/test commands if available.
13. Provide a final report with changed files and how to run locally.

Expected final report:
- What you found in PortalCmsLite
- Whether you reused it or created a CMS-lite structure from scratch
- Files created/changed
- Pages implemented
- SEO metadata added
- How to run locally
- Any assumptions or limitations

Do not include private implementation details in public website content.
Do not add fake customer names, fake certifications, fake case studies, or unsupported claims.
Keep everything professional, realistic, and enterprise-ready.
```
