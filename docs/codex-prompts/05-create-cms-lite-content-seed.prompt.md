# Prompt 05 — Create CMS-lite Seed Content

```text
Continue from Prompt 04.

Before changing anything, read:
- docs/implementation/01-deep-plan.md
- docs/codex-prompts/00-master-plan.md
- docs/brainstorming/01-website-strategy-fa.md

Goal:
Create seed content for the Hooshyaran.ir CMS-lite database.

Important:
This is public website content. Use only marketing-level descriptions. Do not reveal source code internals, infrastructure, Docker, deployment details, algorithms, prompts, security internals, or private stack details.

Seed the following content:

1. Product categories:
- زیرساخت و عملیات هوش مصنوعی
- اتوماسیون هوشمند فرایندها
- دستیارهای سازمانی
- مدیریت دانش
- تحلیل داده و تصمیم‌سازی

2. Product: LLMOPS
Persian title:
LLMOPS | مدیریت سازمانی مدل‌های زبانی

Short description:
راهکاری برای مدیریت، پایش و بهره‌برداری سازمانی از مدل‌های زبانی و سرویس‌های هوش مصنوعی.

Marketing focus:
- مدیریت استفاده سازمانی از AI
- پایش مصرف و عملکرد
- کاهش استفاده پراکنده و بدون کنترل از ابزارهای هوش مصنوعی
- کمک به تیم‌های فناوری و AI
- آماده‌سازی سازمان برای بهره‌برداری گسترده‌تر از مدل‌های زبانی

3. Product: WorkGraph-AI
Persian title:
WorkGraph-AI | اتوماسیون هوشمند جریان‌های کاری

Short description:
راهکاری برای طراحی، مدیریت و اجرای جریان‌های کاری هوشمند با کمک AI در سازمان‌ها.

Marketing focus:
- اتوماسیون هوشمند فرایندها
- کاهش کارهای تکراری
- افزایش هماهنگی بین تیم‌ها
- ترکیب تصمیم‌گیری انسانی و AI
- شفاف‌تر و قابل کنترل‌تر کردن فرایندهای سازمانی

4. Homepage CTA blocks:
- درخواست جلسه معرفی
- مشاهده محصولات
- مشاوره سازمانی

5. FAQ items for homepage and product pages.

6. Static page content for:
- About
- Contact / Request Demo

Requirements:
1. Create a clean seeding service or DbInitializer.
2. Seed content only if it does not already exist.
3. Use stable slugs.
4. Add SEO title, meta description, and keywords for each product.
5. Keep text Persian and RTL-friendly.
6. Keep content professional and B2B-focused.

Suggested slugs:
- llmops
- workgraph-ai
- enterprise-ai
- ai-workflow-automation

After changes:
1. Run database migration/update if needed.
2. Run build.
3. Report seeded entities.
4. Report any assumptions.
```
