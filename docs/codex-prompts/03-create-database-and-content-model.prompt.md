# Prompt 03 — Create Database and CMS-lite Content Model

```text
Continue from Prompt 02.

Before changing anything, read:
- docs/implementation/01-deep-plan.md
- docs/codex-prompts/00-master-plan.md

Goal:
Create the database layer and CMS-lite content model for Hooshyaran.ir.

Requirements:
1. Add Entity Framework Core if it is not already added.
2. Use a development-friendly database provider.
   - Prefer SQL Server LocalDB if the project is Windows/Visual Studio focused.
   - SQLite is acceptable if simpler and documented clearly.
3. Create a DbContext named HooshyaranDbContext.
4. Add clean entities for CMS-lite content.

Minimum entities:
- ProductCategory
- Product
- BlogArticle
- StaticPage or PageContent
- FaqItem
- CtaBlock
- SeoMetadata

Recommended fields:
ProductCategory:
- Id
- Title
- Slug
- Description
- SortOrder
- IsActive

Product:
- Id
- CategoryId
- Name
- PersianTitle
- Slug
- ShortDescription
- LongDescription
- ProblemsSolved
- Benefits
- PublicFeatures
- TargetAudience
- UseCases
- HeroImagePath
- LogoPath
- CtaText
- IsFeatured
- SortOrder
- IsActive
- SeoTitle
- SeoDescription
- SeoKeywords

BlogArticle:
- Id
- Title
- Slug
- Summary
- Body
- AuthorName
- PublishedAt
- IsPublished
- SeoTitle
- SeoDescription
- SeoKeywords

FaqItem:
- Id
- Question
- Answer
- PageKey
- SortOrder
- IsActive

CtaBlock:
- Id
- Key
- Title
- Description
- ButtonText
- ButtonUrl
- IsActive

Important content rule:
Store public marketing content only. Do not include confidential product logic, stack, deployment details, secrets, or source-code-level explanations.

Tasks:
1. Add EF Core packages.
2. Create Data folder and DbContext.
3. Create entities.
4. Configure relationships and indexes, especially Slug fields.
5. Add connection string.
6. Add migration if the project supports migrations.
7. Add a simple database initialization path for development.
8. Ensure the project builds.

Do not seed full content yet. Seeding will be done in Prompt 05.

After changes:
- Run build.
- Report packages added.
- Report entities created.
- Report database provider selected.
- Report migration commands if needed.
```
