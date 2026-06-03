# Font Guidance

The user uploaded an `AnjomanFaNum` font package containing WOFF files, but font binaries are not committed here.

When implementing the website:

1. Use `AnjomanFaNum` only if the font files are available in your local working directory and the license allows use in this project.
2. If the font is not available or license status is unclear, use safe Persian fallbacks:
   - `Vazirmatn`
   - `Tahoma`
   - `system-ui`
3. The website must remain fully readable even without the custom font.
4. Add font loading through CSS only after confirming the font files exist in the project.

Suggested CSS stack:

```css
font-family: 'AnjomanFaNum', 'Vazirmatn', Tahoma, system-ui, sans-serif;
```
