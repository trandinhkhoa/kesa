import { t } from "./i18n.js";

function getById(id) {
  return document.getElementById(id);
}

function escapeHtml(value) {
  return String(value)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#039;");
}

export function setBackendUrl(url) {
  getById("backend-url").textContent = url;
}

export function switchTab(tab) {
  const isCandidates = tab === "candidates";

  getById("tab-candidates").classList.toggle("active", isCandidates);
  getById("tab-fields").classList.toggle("active", !isCandidates);
  getById("panel-candidates").classList.toggle("hidden", !isCandidates);
  getById("panel-fields").classList.toggle("hidden", isCandidates);
}

export function showNotification(message, kind = "success", retryCallback = null) {
  const banner = getById("notification");
  banner.classList.remove("hidden", "success", "error");
  banner.classList.add(kind === "error" ? "error" : "success");

  banner.innerHTML = "";

  const text = document.createElement("span");
  text.textContent = message;
  banner.appendChild(text);

  if (retryCallback) {
    const retryButton = document.createElement("button");
    retryButton.textContent = t("retry");
    retryButton.className = "secondary";
    retryButton.addEventListener("click", retryCallback);
    banner.appendChild(retryButton);
  }

  window.setTimeout(() => {
    banner.classList.add("hidden");
  }, 5000);
}

export function clearFormErrors(containerId) {
  const container = getById(containerId);
  container.innerHTML = "";
  container.classList.add("hidden");
}

export function showFormErrors(containerId, messages) {
  const container = getById(containerId);
  const items = Array.isArray(messages) ? messages : [messages];
  container.innerHTML = `<ul>${items.map((item) => `<li>${escapeHtml(item)}</li>`).join("")}</ul>`;
  container.classList.remove("hidden");
}

export function mapApiErrorToMessages(error) {
  const messages = [];

  if (error.status === 400 && error.validationErrors && Object.keys(error.validationErrors).length > 0) {
    Object.entries(error.validationErrors).forEach(([field, fieldErrors]) => {
      const normalized = Array.isArray(fieldErrors) ? fieldErrors : [String(fieldErrors)];
      normalized.forEach((entry) => messages.push(`${field}: ${entry}`));
    });
    return messages;
  }

  if (error.status === 404) {
    return [t("resourceNotFound")];
  }

  if (error.status === 409) {
    return [error.message || t("requestConflict")];
  }

  if (error.status === 0) {
    return [error.message || t("networkError")];
  }

  return [error.message || t("apiRequestFailed")];
}

export function setBusy(buttonId, isBusy, busyLabel) {
  const button = getById(buttonId);
  if (!button) {
    return;
  }

  if (isBusy) {
    button.dataset.defaultLabel = button.textContent;
    button.disabled = true;
    button.textContent = busyLabel;
    return;
  }

  button.disabled = false;
  button.textContent = button.dataset.defaultLabel || button.textContent;
}

export function confirmDelete(message) {
  return window.confirm(message);
}

export function toLocalDateTime(value) {
  if (!value) {
    return "-";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleString("vi-VN");
}
