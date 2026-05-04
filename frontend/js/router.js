const routes = [];

export function route(pattern, handler) {
  routes.push({ pattern, handler });
}

export function navigate(path) {
  window.location.hash = path;
}

function dispatch() {
  const path = window.location.hash.slice(1) || '/candidates';
  for (const { pattern, handler } of routes) {
    const names = [];
    const regexStr =
      '^' +
      pattern.replace(/:([^/]+)/g, (_, n) => {
        names.push(n);
        return '([^/]+)';
      }) +
      '$';
    const m = path.match(new RegExp(regexStr));
    if (m) {
      const params = Object.fromEntries(names.map((n, i) => [n, m[i + 1]]));
      handler(params);
      return;
    }
  }
}

export function initRouter() {
  window.addEventListener('hashchange', dispatch);
  dispatch();
}
