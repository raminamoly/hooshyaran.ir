(() => {
  const navLinks = Array.from(document.querySelectorAll('.ai-integrator-page .module-strip a'));

  if (!navLinks.length || !('IntersectionObserver' in window)) {
    return;
  }

  const sectionMap = new Map();

  navLinks.forEach((link) => {
    const id = link.getAttribute('href')?.replace('#', '');
    const section = id ? document.getElementById(id) : null;

    if (section) {
      sectionMap.set(section, link);
    }
  });

  const activateLink = (activeLink) => {
    navLinks.forEach((link) => link.classList.toggle('is-active', link === activeLink));
  };

  const observer = new IntersectionObserver((entries) => {
    const visible = entries
      .filter((entry) => entry.isIntersecting)
      .sort((first, second) => second.intersectionRatio - first.intersectionRatio)[0];

    if (!visible) {
      return;
    }

    const activeLink = sectionMap.get(visible.target);

    if (activeLink) {
      activateLink(activeLink);
    }
  }, {
    threshold: [0.22, 0.44, 0.66],
    rootMargin: '-26% 0px -46% 0px'
  });

  sectionMap.forEach((_, section) => observer.observe(section));
})();
