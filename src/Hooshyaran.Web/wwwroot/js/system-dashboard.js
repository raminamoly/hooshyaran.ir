(function () {
  const analytics = window.hooshyaranAnalytics || {};
  const svgNamespace = 'http://www.w3.org/2000/svg';

  const createSvgElement = (name, attributes = {}) => {
    const element = document.createElementNS(svgNamespace, name);

    Object.entries(attributes).forEach(([key, value]) => {
      element.setAttribute(key, String(value));
    });

    return element;
  };

  const getSeries = (data) => ({
    labels: Array.isArray(data.labels) ? data.labels : [],
    values: Array.isArray(data.values) ? data.values.map(Number) : []
  });

  const clearChart = (containerId) => {
    const container = document.getElementById(containerId);

    if (!container) {
      return null;
    }

    container.textContent = '';
    return container;
  };

  const drawEmptyState = (container, text) => {
    const empty = document.createElement('span');
    empty.className = 'analytics-chart__empty';
    empty.textContent = text;
    container.appendChild(empty);
  };

  const drawAxisLabels = (svg, labels, width, height, padding) => {
    const visibleLabels = labels.length > 5
      ? labels.filter((_, index) => index === 0 || index === labels.length - 1 || index % Math.ceil(labels.length / 4) === 0)
      : labels;

    visibleLabels.forEach((label) => {
      const originalIndex = labels.indexOf(label);
      const x = padding + (labels.length <= 1 ? 0 : (originalIndex / (labels.length - 1)) * (width - padding * 2));
      const text = createSvgElement('text', {
        x,
        y: height - 10,
        'text-anchor': 'middle',
        class: 'analytics-chart__label'
      });
      text.textContent = label;
      svg.appendChild(text);
    });
  };

  const drawLineChart = (containerId, data, color) => {
    const container = clearChart(containerId);

    if (!container) {
      return;
    }

    const { labels, values } = getSeries(data || {});

    if (!values.length) {
      drawEmptyState(container, 'داده‌ای برای نمایش نیست');
      return;
    }

    const width = 520;
    const height = 220;
    const padding = 34;
    const maxValue = Math.max(...values, 1);
    const points = values.map((value, index) => {
      const x = padding + (values.length <= 1 ? 0 : (index / (values.length - 1)) * (width - padding * 2));
      const y = height - padding - (value / maxValue) * (height - padding * 2);
      return [x, y];
    });
    const pointList = points.map(([x, y]) => `${x.toFixed(1)},${y.toFixed(1)}`).join(' ');
    const areaPoints = `${padding},${height - padding} ${pointList} ${width - padding},${height - padding}`;
    const svg = createSvgElement('svg', {
      viewBox: `0 0 ${width} ${height}`,
      role: 'img',
      class: 'analytics-chart__svg'
    });

    svg.appendChild(createSvgElement('polygon', {
      points: areaPoints,
      fill: color,
      opacity: 0.14
    }));
    svg.appendChild(createSvgElement('polyline', {
      points: pointList,
      fill: 'none',
      stroke: color,
      'stroke-width': 4,
      'stroke-linecap': 'round',
      'stroke-linejoin': 'round'
    }));
    points.forEach(([x, y]) => {
      svg.appendChild(createSvgElement('circle', {
        cx: x,
        cy: y,
        r: 4,
        fill: '#ffffff',
        stroke: color,
        'stroke-width': 3
      }));
    });
    drawAxisLabels(svg, labels, width, height, padding);
    container.appendChild(svg);
  };

  const drawColumnChart = (containerId, data, color) => {
    const container = clearChart(containerId);

    if (!container) {
      return;
    }

    const { labels, values } = getSeries(data || {});

    if (!values.length) {
      drawEmptyState(container, 'داده‌ای برای نمایش نیست');
      return;
    }

    const width = 520;
    const height = 220;
    const padding = 34;
    const maxValue = Math.max(...values, 1);
    const innerWidth = width - padding * 2;
    const barSlot = innerWidth / values.length;
    const barWidth = Math.max(14, barSlot * 0.58);
    const svg = createSvgElement('svg', {
      viewBox: `0 0 ${width} ${height}`,
      role: 'img',
      class: 'analytics-chart__svg'
    });

    values.forEach((value, index) => {
      const barHeight = (value / maxValue) * (height - padding * 2);
      const x = padding + index * barSlot + (barSlot - barWidth) / 2;
      const y = height - padding - barHeight;
      svg.appendChild(createSvgElement('rect', {
        x,
        y,
        width: barWidth,
        height: Math.max(2, barHeight),
        rx: 5,
        fill: color,
        opacity: 0.86
      }));
    });
    drawAxisLabels(svg, labels, width, height, padding);
    container.appendChild(svg);
  };

  const drawDonutChart = (containerId, data) => {
    const container = clearChart(containerId);

    if (!container) {
      return;
    }

    const { labels, values } = getSeries(data || {});
    const total = values.reduce((sum, value) => sum + value, 0);

    if (!total) {
      drawEmptyState(container, 'داده‌ای برای نمایش نیست');
      return;
    }

    const width = 260;
    const height = 220;
    const radius = 64;
    const strokeWidth = 18;
    const circumference = 2 * Math.PI * radius;
    let offset = 0;
    const colors = ['#00b8a9', '#f28a00', '#2b6e9f'];
    const svg = createSvgElement('svg', {
      viewBox: `0 0 ${width} ${height}`,
      role: 'img',
      class: 'analytics-chart__svg analytics-chart__svg--donut'
    });
    const group = createSvgElement('g', {
      transform: `translate(${width / 2} 94) rotate(-90)`
    });

    values.forEach((value, index) => {
      const share = value / total;
      const circle = createSvgElement('circle', {
        r: radius,
        cx: 0,
        cy: 0,
        fill: 'none',
        stroke: colors[index % colors.length],
        'stroke-width': strokeWidth,
        'stroke-dasharray': `${circumference * share} ${circumference}`,
        'stroke-dashoffset': -offset,
        'stroke-linecap': 'round'
      });
      offset += circumference * share;
      group.appendChild(circle);
    });

    const totalLabel = createSvgElement('text', {
      x: width / 2,
      y: 100,
      'text-anchor': 'middle',
      class: 'analytics-chart__total'
    });
    totalLabel.textContent = total.toLocaleString('fa-IR');
    svg.appendChild(group);
    svg.appendChild(totalLabel);

    labels.forEach((label, index) => {
      const legend = createSvgElement('text', {
        x: width / 2,
        y: 178 + index * 22,
        'text-anchor': 'middle',
        class: 'analytics-chart__label'
      });
      legend.textContent = `${label}: ${(values[index] || 0).toLocaleString('fa-IR')}`;
      svg.appendChild(legend);
    });
    container.appendChild(svg);
  };

  drawLineChart('visitsChart', analytics.visits, '#2b6e9f');
  drawDonutChart('onlineChart', analytics.online);
  drawColumnChart('articleClicksChart', analytics.articleClicks, '#00b8a9');
  drawLineChart('demoSubmitsChart', analytics.demoSubmits, '#f28a00');
})();
