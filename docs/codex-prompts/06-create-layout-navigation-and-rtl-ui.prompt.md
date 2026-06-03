# Prompt 06 — Create Layout, Navigation and RTL UI Foundation

```text
Continue from Prompt 05.

Before changing anything, read:
- docs/implementation/01-deep-plan.md
- docs/codex-prompts/00-master-plan.md
- assets/fonts/README.md

Goal:
Create the main Persian RTL layout, navigation, footer, shared sections, and UI foundation for Hooshyaran.ir.

Requirements:
1. Set the HTML document language to Persian:
   lang="fa"
2. Set the document direction:
   dir="rtl"
3. Create a reusable main layout.
4. Add a professional header.
5. Add a professional footer.
6. Add reusable UI components/partials where suitable.
7. Ensure mobile responsiveness.
8. Ensure good Persian typography.
9. Use the brand logo from wwwroot/assets/brand.

Header navigation:
- خانه
- محصولات
- راهکارها
- کاربردها
- بلاگ
- درباره ما
- تماس با ما

Header CTA:
درخواست دمو

Footer sections:
- معرفی کوتاه هوش‌یاران
- محصولات
- راهکارها
- بلاگ
- تماس

Footer short text:
هوش‌یاران، کاتالوگ راهکارهای هوش مصنوعی سازمانی برای کسب‌وکارها و نهادهای بزرگ.

Reusable sections/components:
- PageHero
- SectionTitle
- ProductCard
- FeatureGrid
- CtaSection
- FaqSection
- SeoMetadata helper if needed

Visual direction:
- Enterprise SaaS
- Dark hero allowed
- Light content sections
- Blue/teal foundation
- Orange/green accents inspired by the logo
- Clean cards
- Good spacing
- No childish robot imagery

Accessibility:
- Use semantic HTML.
- Use meaningful alt text for logo/images.
- Ensure visible focus states.
- Ensure contrast is acceptable.
- Buttons and links must be clear.

Do not implement final page content yet except layout-level placeholder content.

After changes:
1. Run build.
2. Check that temporary home page uses the layout.
3. Report all changed files.
4. Report any responsive or RTL decisions.
```
