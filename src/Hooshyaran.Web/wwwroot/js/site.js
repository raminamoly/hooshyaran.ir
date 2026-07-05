const navToggle = document.querySelector('.nav-toggle');
const primaryNav = document.querySelector('#primary-navigation');
const siteHeader = document.querySelector('[data-site-header]');

if (navToggle && primaryNav) {
  navToggle.addEventListener('click', () => {
    const isOpen = navToggle.getAttribute('aria-expanded') === 'true';
    navToggle.setAttribute('aria-expanded', String(!isOpen));
    document.body.classList.toggle('nav-open', !isOpen);
  });

  primaryNav.addEventListener('click', (event) => {
    if (event.target instanceof HTMLAnchorElement) {
      navToggle.setAttribute('aria-expanded', 'false');
      document.body.classList.remove('nav-open');
    }
  });
}

const syncHeaderState = () => {
  if (!siteHeader) {
    return;
  }

  siteHeader.classList.toggle('site-header--scrolled', window.scrollY > 18);
};

syncHeaderState();
window.addEventListener('scroll', syncHeaderState, { passive: true });

const initHeroNetwork = () => {
  const canvas = document.querySelector('.hero-network-canvas');
  if (!(canvas instanceof HTMLCanvasElement)) {
    return;
  }

  const context = canvas.getContext('2d');

  if (!context) {
    return;
  }

  let width = 0;
  let height = 0;
  let animationFrame = 0;
  const isGpuVisual = canvas.classList.contains('hero-network-canvas--gpu');
  const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  const points = Array.from({ length: 34 }, (_, index) => ({
    angle: (index / 34) * Math.PI * 2,
    radius: 0.18 + (index % 7) * 0.055,
    speed: 0.0022 + (index % 5) * 0.00055,
    phase: index * 0.47
  }));

  const resize = () => {
    const rect = canvas.getBoundingClientRect();
    const pixelRatio = Math.min(window.devicePixelRatio || 1, 2);
    width = Math.max(1, rect.width);
    height = Math.max(1, rect.height);
    canvas.width = Math.round(width * pixelRatio);
    canvas.height = Math.round(height * pixelRatio);
    context.setTransform(pixelRatio, 0, 0, pixelRatio, 0, 0);
  };

  const drawGpuVisual = (time) => {
    context.clearRect(0, 0, width, height);

    const center = { x: width * 0.5, y: height * 0.5 };
    const systems = [
      { x: width * 0.19, y: height * 0.31, bend: -42, color: 'rgba(125, 231, 242, 0.82)' },
      { x: width * 0.50, y: height * 0.14, bend: 34, color: 'rgba(43, 167, 226, 0.82)' },
      { x: width * 0.82, y: height * 0.29, bend: 44, color: 'rgba(125, 231, 242, 0.78)' },
      { x: width * 0.21, y: height * 0.72, bend: 38, color: 'rgba(28, 191, 166, 0.76)' },
      { x: width * 0.78, y: height * 0.73, bend: -36, color: 'rgba(246, 162, 58, 0.78)' }
    ];
    const pulse = reduceMotion ? 0.62 : (time * 0.00028) % 1;
    const corePulse = reduceMotion ? 0.5 : Math.sin(time * 0.0014) * 0.5 + 0.5;

    const getQuadraticPoint = (from, control, to, progress) => {
      const inverse = 1 - progress;

      return {
        x: inverse * inverse * from.x + 2 * inverse * progress * control.x + progress * progress * to.x,
        y: inverse * inverse * from.y + 2 * inverse * progress * control.y + progress * progress * to.y
      };
    };

    context.lineWidth = 1;
    context.shadowBlur = 0;

    systems.forEach((system, index) => {
      const control = {
        x: (system.x + center.x) / 2 + system.bend,
        y: (system.y + center.y) / 2 + (index % 2 === 0 ? -24 : 22)
      };
      const gradient = context.createLinearGradient(system.x, system.y, center.x, center.y);
      gradient.addColorStop(0, system.color);
      gradient.addColorStop(1, 'rgba(8, 191, 211, 0.12)');
      context.strokeStyle = gradient;
      context.beginPath();
      context.moveTo(system.x, system.y);
      context.quadraticCurveTo(control.x, control.y, center.x, center.y);
      context.stroke();

      for (let packet = 0; packet < 2; packet++) {
        const localPulse = (pulse + index * 0.15 + packet * 0.48) % 1;
        const point = getQuadraticPoint(system, control, center, localPulse);
        const size = 2.4 + Math.sin(localPulse * Math.PI) * 1.8;
        context.fillStyle = system.color;
        context.shadowColor = system.color;
        context.shadowBlur = 18;
        context.beginPath();
        context.arc(point.x, point.y, size, 0, Math.PI * 2);
        context.fill();
      }
    });

    context.shadowBlur = 0;

    for (let i = 0; i < 4; i++) {
      const radius = 42 + i * 30 + Math.sin(time * 0.001 + i) * 5;
      context.strokeStyle = `rgba(8, 191, 211, ${0.21 - i * 0.036})`;
      context.lineWidth = 1;
      context.beginPath();
      context.ellipse(center.x, center.y, radius * 1.45, radius * 0.58, -0.22 + i * 0.18, 0, Math.PI * 2);
      context.stroke();
    }

    const halo = context.createRadialGradient(center.x, center.y, 8, center.x, center.y, 88 + corePulse * 18);
    halo.addColorStop(0, 'rgba(125, 231, 242, 0.28)');
    halo.addColorStop(0.44, 'rgba(8, 191, 211, 0.12)');
    halo.addColorStop(1, 'rgba(8, 191, 211, 0)');
    context.fillStyle = halo;
    context.beginPath();
    context.arc(center.x, center.y, 88 + corePulse * 18, 0, Math.PI * 2);
    context.fill();

    context.shadowColor = 'rgba(8, 191, 211, 0.56)';
    context.shadowBlur = 22;
    context.fillStyle = 'rgba(125, 231, 242, 0.92)';
    context.beginPath();
    context.arc(center.x, center.y, 4.8 + corePulse * 1.4, 0, Math.PI * 2);
    context.fill();
    context.shadowBlur = 0;

    if (!reduceMotion) {
      animationFrame = window.requestAnimationFrame(drawGpuVisual);
    }
  };

  const draw = (time) => {
    if (isGpuVisual) {
      drawGpuVisual(time);
      return;
    }

    context.clearRect(0, 0, width, height);
    const centerX = width * 0.5;
    const centerY = height * 0.46;
    const scale = Math.min(width, height);
    const coords = points.map((point) => {
      const angle = point.angle + time * point.speed;
      const wave = Math.sin(time * 0.0014 + point.phase) * scale * 0.035;

      return {
        x: centerX + Math.cos(angle) * scale * point.radius + wave,
        y: centerY + Math.sin(angle * 1.22) * scale * point.radius * 0.78 + Math.cos(time * 0.001 + point.phase) * 12
      };
    });

    coords.forEach((from, i) => {
      for (let j = i + 1; j < coords.length; j++) {
        const to = coords[j];
        const distance = Math.hypot(from.x - to.x, from.y - to.y);

        if (distance < scale * 0.26) {
          const opacity = 1 - distance / (scale * 0.26);
          context.strokeStyle = `rgba(0, 184, 169, ${opacity * 0.34})`;
          context.lineWidth = 1;
          context.beginPath();
          context.moveTo(from.x, from.y);
          context.lineTo(to.x, to.y);
          context.stroke();
        }
      }
    });

    coords.forEach((point, index) => {
      const glow = index % 6 === 0;
      context.fillStyle = glow ? 'rgba(242, 138, 0, 0.95)' : 'rgba(115, 232, 224, 0.92)';
      context.shadowColor = glow ? 'rgba(242, 138, 0, 0.55)' : 'rgba(0, 184, 169, 0.55)';
      context.shadowBlur = glow ? 18 : 12;
      context.beginPath();
      context.arc(point.x, point.y, glow ? 3.6 : 2.4, 0, Math.PI * 2);
      context.fill();
    });

    context.shadowBlur = 0;
    if (!reduceMotion) {
      animationFrame = window.requestAnimationFrame(draw);
    }
  };

  resize();
  window.addEventListener('resize', resize, { passive: true });
  animationFrame = window.requestAnimationFrame(draw);

  canvas.addEventListener('DOMNodeRemovedFromDocument', () => {
    window.cancelAnimationFrame(animationFrame);
  }, { once: true });
};

initHeroNetwork();

const revealTargets = document.querySelectorAll(
  '.page-hero__content, .page-hero__visual, .page-hero__dashboard, .home-signal, .section-title, .home-product-spotlight__copy, .home-product-spotlight__media, .feature-item, .product-card, .article-card, .cta-section, .employee-process__step, .employee-architecture__figure, .employee-indicators-card, .employee-privacy-card'
);

revealTargets.forEach((target) => target.classList.add('reveal-ready'));

if ('IntersectionObserver' in window) {
  const revealObserver = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        entry.target.classList.add('is-visible');
        revealObserver.unobserve(entry.target);
      }
    });
  }, { threshold: 0.16, rootMargin: '0px 0px -8% 0px' });

  revealTargets.forEach((target) => revealObserver.observe(target));
} else {
  revealTargets.forEach((target) => target.classList.add('is-visible'));
}

if ('serviceWorker' in navigator) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/service-worker.js').catch(() => undefined);
  });
}
