import { config } from "./config.js";
import { state } from "./state.js";
import {
  initializeCandidateModule,
  loadCandidates,
  rerenderCandidateCustomFieldsPreservingValues,
  showCandidateCreate,
  showCandidateDetail,
  showCandidateList
} from "./candidates.js";
import { initializeFieldModule, loadFieldDefinitions } from "./fields.js";
import { setBackendUrl, showNotification, switchTab } from "./ui.js";
import { i18nInit, t } from "./i18n.js";
import { initRouter, navigate, route } from "./router.js";

function registerRoutes() {
  route("/candidates", () => {
    switchTab("candidates");
    showCandidateList();
  });

  route("/candidates/new", () => {
    switchTab("candidates");
    showCandidateCreate();
  });

  route("/candidates/:id/edit", ({ id }) => {
    switchTab("candidates");
    showCandidateDetail(id, "edit");
  });

  route("/candidates/:id/view", ({ id }) => {
    switchTab("candidates");
    showCandidateDetail(id, "view");
  });

  route("/fields", () => {
    switchTab("fields");
  });
}

function initializeTabs() {
  document.getElementById("tab-candidates").addEventListener("click", () => navigate("/candidates"));
  document.getElementById("tab-fields").addEventListener("click", () => navigate("/fields"));
}

async function bootstrap() {
  i18nInit();
  setBackendUrl(config.apiBaseUrl);
  initializeTabs();
  registerRoutes();
  initializeFieldModule();
  initializeCandidateModule();

  try {
    await loadFieldDefinitions();
    rerenderCandidateCustomFieldsPreservingValues();
    await loadCandidates();
  } catch {
    showNotification(t("initApiErrors"), "error");
  }

  initRouter();
}

bootstrap();
