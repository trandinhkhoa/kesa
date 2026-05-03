import { config } from "./config.js";
import { state } from "./state.js";
import { initializeCandidateModule, loadCandidates, rerenderCandidateCustomFieldsPreservingValues } from "./candidates.js";
import { initializeFieldModule, loadFieldDefinitions } from "./fields.js";
import { setBackendUrl, showNotification, switchTab } from "./ui.js";
import { i18nInit, t } from "./i18n.js";

function getById(id) {
  return document.getElementById(id);
}

function setTab(tab) {
  state.activeTab = tab;
  switchTab(tab);
}

function initializeTabs() {
  getById("tab-candidates").addEventListener("click", () => setTab("candidates"));
  getById("tab-fields").addEventListener("click", () => setTab("fields"));
  setTab("candidates");
}

async function bootstrap() {
  i18nInit();
  setBackendUrl(config.apiBaseUrl);
  initializeTabs();
  initializeFieldModule();
  initializeCandidateModule();

  try {
    await loadFieldDefinitions();
    rerenderCandidateCustomFieldsPreservingValues();
    await loadCandidates();
  } catch {
    showNotification(t("initApiErrors"), "error");
  }
}

bootstrap();
