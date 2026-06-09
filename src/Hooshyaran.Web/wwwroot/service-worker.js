const CACHE_NAME = 'hooshyaran-public-v1';
const CORE_ASSETS = [
  '/',
  '/products',
  '/solutions',
  '/use-cases',
  '/blog',
  '/about',
  '/contact',
  '/request-demo',
  '/css/fonts.css',
  '/css/site.css',
  '/js/site.js',
  '/assets/brand/favicon.svg',
  '/assets/brand/hooshyaran-logo-small.png',
  '/assets/content/home-enterprise-ai-hero.jpg',
  '/assets/content/article-ai-workflow-automation.jpg',
  '/assets/content/article-llmops-enterprise-control.jpg',
  '/assets/content/article-private-ai-governance.jpg',
  '/assets/content/complaint-management-ai-dashboard.jpg',
  '/assets/content/employee-monitor-ai-dashboard.jpg',
  '/assets/fonts/iransansx-fa-num/IRANSansXFaNum-Regular.woff2',
  '/assets/fonts/iransansx-fa-num/IRANSansXFaNum-DemiBold.woff2',
  '/assets/fonts/iransansx-fa-num/IRANSansXFaNum-Bold.woff2'
];

const STATIC_FILE_PATTERN = /\.(?:css|js|jpg|jpeg|png|svg|webp|woff2|ico)$/i;

const cacheFreshResponse = async (cache, request) => {
  const response = await fetch(request);

  if (response.ok) {
    await cache.put(request, response.clone());
  }

  return response;
};

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then((cache) => Promise.allSettled(
        CORE_ASSETS.map((asset) => cacheFreshResponse(cache, new Request(asset, { cache: 'reload' })))
      ))
      .then(() => self.skipWaiting())
  );
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys()
      .then((keys) => Promise.all(keys
        .filter((key) => key !== CACHE_NAME)
        .map((key) => caches.delete(key))))
      .then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', (event) => {
  const { request } = event;
  const url = new URL(request.url);

  if (request.method !== 'GET' || url.origin !== self.location.origin) {
    return;
  }

  event.respondWith(
    caches.open(CACHE_NAME).then(async (cache) => {
      const cached = await cache.match(request);

      if (request.mode === 'navigate') {
        const refresh = cacheFreshResponse(cache, request).catch(() => cached || cache.match('/'));
        return cached || refresh;
      }

      if (STATIC_FILE_PATTERN.test(url.pathname)) {
        if (cached) {
          event.waitUntil(cacheFreshResponse(cache, request).catch(() => undefined));
          return cached;
        }

        return cacheFreshResponse(cache, request);
      }

      return fetch(request);
    })
  );
});
