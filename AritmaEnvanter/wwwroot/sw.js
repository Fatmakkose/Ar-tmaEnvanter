const cacheName = 'aritma-envanter-v1';
const assets = [
  '/',
  '/css/site.css',
  '/js/site.js',
  '/images/maskilogo.png'
];

self.addEventListener('install', e => {
  e.waitUntil(
    caches.open(cacheName).then(cache => {
      cache.addAll(assets);
    })
  );
});

self.addEventListener('fetch', e => {
  e.respondWith(
    caches.match(e.request).then(response => {
      return response || fetch(e.request);
    })
  );
});
