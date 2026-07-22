# پچ تنظیم‌پذیر شدن HTTPS redirect

تاریخ آماده‌سازی:

```text
2026-07-15
```

## هدف

این پچ redirectهای زیر را از حالت hard-code خارج می‌کند:

```text
HTTP to HTTPS
HSTS
www.hooshyaran.ir to hooshyaran.ir
```

## فایل‌های برنامه در پچ

فایل‌های publish شده‌ای که باید در ریشه فولدر سایت IIS جایگزین شوند:

```text
Hooshyaran.Web.dll
Hooshyaran.Web.exe
Hooshyaran.Web.deps.json
Hooshyaran.Web.runtimeconfig.json
```

فایل زیر اختیاری است و فقط برای دیباگ بهتر کاربرد دارد:

```text
Hooshyaran.Web.pdb
```

## تنظیمات سرور

فایل production زیر را کورکورانه جایگزین نکنید:

```text
appsettings.json
```

در صورت نیاز، این بخش‌ها را به فایل production موجود اضافه یا اصلاح کنید:

```json
"Security": {
  "EnableHttpsRedirection": true,
  "EnableHsts": true
},
"CanonicalHost": {
  "Enabled": true,
  "SourceHost": "www.hooshyaran.ir",
  "TargetHost": "hooshyaran.ir",
  "Scheme": "https",
  "Permanent": true
}
```

برای خاموش کردن redirect از HTTP به HTTPS:

```json
"Security": {
  "EnableHttpsRedirection": false,
  "EnableHsts": false
}
```

برای خاموش کردن redirect دامنه canonical:

```json
"CanonicalHost": {
  "Enabled": false
}
```

## SQL

این پچ تغییر دیتابیس ندارد.

## نتیجه بررسی محلی

اسکریپت smoke انتشار Production اجرا شد و موفق بود.

مسیر publish تست‌شده:

```text
C:\publish\hooshyaran-smoke-https-dynamic
```

دستور اجرا:

```powershell
.\scripts\smoke-production-publish.ps1 -PublishPath C:\publish\hooshyaran-smoke-https-dynamic -BaseUrl http://127.0.0.1:5229
```

نتیجه:

```text
Smoke test passed
```

## مراحل نصب روی سرور

1. از فولدر فعلی سایت بکاپ بگیرید.
2. سایت یا app pool را در IIS متوقف کنید.
3. فایل‌های داخل پوشه `publish-files` را در ریشه سایت IIS جایگزین کنید.
4. فایل `appsettings.json` production را حفظ کنید.
5. در صورت نیاز، تنظیمات بالا را به `appsettings.json` production اضافه کنید.
6. app pool را روشن کنید.

## چک بعد از نصب

این مسیرها باید پاسخ 200 بدهند:

```text
https://hooshyaran.ir/
https://hooshyaran.ir/products
https://hooshyaran.ir/blog
https://hooshyaran.ir/ai-integrator
https://hooshyaran.ir/request-demo
https://hooshyaran.ir/robots.txt
https://hooshyaran.ir/sitemap.xml
```
