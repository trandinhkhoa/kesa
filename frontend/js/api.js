import { config } from "./config.js";

const defaultHeaders = {
  "Content-Type": "application/json"
};

function buildUrl(path) {
  return `${config.apiBaseUrl}${path}`;
}

function parseValidationErrors(payload) {
  if (!payload || typeof payload !== "object") {
    return {};
  }

  if (payload.errors && typeof payload.errors === "object") {
    return payload.errors;
  }

  if (payload.extensions && typeof payload.extensions === "object" && payload.extensions.errors) {
    return payload.extensions.errors;
  }

  return {};
}

async function parseBody(response) {
  const raw = await response.text();
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw);
  } catch {
    return { detail: raw };
  }
}

async function request(method, path, body) {
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), 10000);

  try {
    const response = await fetch(buildUrl(path), {
      method,
      headers: defaultHeaders,
      body: body === undefined ? undefined : JSON.stringify(body),
      signal: controller.signal
    });

    const payload = await parseBody(response);

    if (!response.ok) {
      const error = new Error(payload?.detail || payload?.title || "API request failed");
      error.status = response.status;
      error.code = payload?.errorCode || payload?.extensions?.errorCode || null;
      error.payload = payload;
      error.validationErrors = parseValidationErrors(payload);
      throw error;
    }

    return payload;
  } catch (error) {
    if (error.name === "AbortError") {
      const timeoutError = new Error("Request timed out. Please try again.");
      timeoutError.status = 0;
      timeoutError.code = "NETWORK_TIMEOUT";
      timeoutError.validationErrors = {};
      throw timeoutError;
    }

    if (error.status === undefined) {
      const networkError = new Error("Network error. Please verify backend is running.");
      networkError.status = 0;
      networkError.code = "NETWORK_ERROR";
      networkError.validationErrors = {};
      throw networkError;
    }

    throw error;
  } finally {
    clearTimeout(timeoutId);
  }
}

export function get(path) {
  return request("GET", path);
}

export function post(path, body) {
  return request("POST", path, body);
}

export function put(path, body) {
  return request("PUT", path, body);
}

export function del(path) {
  return request("DELETE", path);
}
