import { del, get, post, put } from "./api.js";
import { state } from "./state.js";
import { t } from "./i18n.js";
import { navigate } from "./router.js";
import {
  clearFormErrors,
  confirmDelete,
  mapApiErrorToMessages,
  setBusy,
  showFormErrors,
  showNotification,
  toLocalDateTime
} from "./ui.js";

function getById(id) {
  return document.getElementById(id);
}

function formatDateOnly(value) {
  if (!value) {
    return "-";
  }
  return value;
}

function buildCustomFieldControl(definition, value) {
  const row = document.createElement("div");
  row.className = "custom-field-row";

  const wrapper = document.createElement("label");
  wrapper.dataset.fieldKey = definition.key;
  wrapper.dataset.fieldType = definition.dataType;
  wrapper.dataset.fieldRequired = String(Boolean(definition.isRequired));

  const labelText = definition.isRequired ? `${definition.name} *` : definition.name;
  wrapper.append(labelText);

  let input;
  switch (definition.dataType) {
    case "Number":
      input = document.createElement("input");
      input.type = "number";
      input.step = "any";
      if (value !== undefined && value !== null && value !== "") {
        input.value = String(value);
      }
      break;
    case "Date":
      input = document.createElement("input");
      input.type = "date";
      if (value) {
        input.value = String(value);
      }
      break;
    case "Boolean":
      input = document.createElement("input");
      input.type = "checkbox";
      input.checked = Boolean(value);
      break;
    case "Enum": {
      input = document.createElement("select");
      const defaultOption = document.createElement("option");
      defaultOption.value = "";
      defaultOption.textContent = t("selectOption");
      input.appendChild(defaultOption);

      (definition.options || []).forEach((option) => {
        const node = document.createElement("option");
        node.value = option;
        node.textContent = option;
        input.appendChild(node);
      });

      if (value !== undefined && value !== null) {
        input.value = String(value);
      }
      break;
    }
    default:
      input = document.createElement("input");
      input.type = "text";
      if (value !== undefined && value !== null) {
        input.value = String(value);
      }
      break;
  }

  input.dataset.customInput = "true";
  wrapper.appendChild(input);
  row.appendChild(wrapper);

  const removeBtn = document.createElement("button");
  removeBtn.type = "button";
  removeBtn.className = "danger remove-field-btn";
  removeBtn.textContent = "−";
  removeBtn.title = "Xóa trường";
  removeBtn.addEventListener("click", () => {
    row.remove();
  });
  row.appendChild(removeBtn);

  return row;
}

function renderCandidateRows() {
  const body = getById("candidate-table-body");
  const empty = getById("candidate-empty");

  if (!state.candidates.length) {
    body.innerHTML = "";
    empty.classList.remove("hidden");
    return;
  }

  empty.classList.add("hidden");
  body.innerHTML = state.candidates
    .map(
      (item) => `
      <tr>
        <td>${item.name}</td>
        <td>${formatDateOnly(item.birthDate)}</td>
        <td>${item.sex}</td>
        <td>${toLocalDateTime(item.updatedAt)}</td>
        <td>
          <div class="row-actions">
            <button class="secondary" data-candidate-view="${item.id}">${t("view")}</button>
            <button data-candidate-edit="${item.id}">${t("edit")}</button>
            <button class="danger" data-candidate-delete="${item.id}">${t("deleteAction")}</button>
          </div>
        </td>
      </tr>
    `
    )
    .join("");

  const currentPage = state.candidatePaging.pageNumber;
  const totalCount = state.candidatePaging.totalCount;
  const pageSize = state.candidatePaging.pageSize;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  getById("candidate-page-label").textContent = t("pageLabel", currentPage, totalPages, totalCount);
  getById("candidate-prev-page").disabled = currentPage <= 1;
  getById("candidate-next-page").disabled = currentPage >= totalPages;
}

function setCandidateMode(mode) {
  state.candidateMode = mode;

  const modeBadge = getById("candidate-mode-badge");
  const submitButton = getById("candidate-submit-btn");

  if (mode === "edit") {
    modeBadge.textContent = t("modeEdit");
    submitButton.textContent = t("updateCandidate");
  } else if (mode === "view") {
    modeBadge.textContent = t("modeRead");
    submitButton.textContent = t("createCandidate");
  } else {
    modeBadge.textContent = t("modeCreate");
    submitButton.textContent = t("createCandidate");
  }

  getById("candidate-edit-btn").classList.toggle("hidden", mode !== "view");

  const isReadonly = mode === "view";
  getById("candidate-form").querySelectorAll("input,select").forEach((node) => {
    if (node.id !== "candidate-id") {
      node.disabled = isReadonly;
    }
  });
  submitButton.disabled = isReadonly;
  submitButton.classList.toggle("hidden", isReadonly);

  const addRow = getById("candidate-add-custom-field");
  if (addRow) {
    addRow.querySelectorAll("input,button").forEach((node) => {
      node.disabled = isReadonly;
    });
    addRow.classList.toggle("hidden", isReadonly);
  }

  const showRemoveButtons = mode === "edit";
  getById("candidate-custom-fields").querySelectorAll(".remove-field-btn").forEach((btn) => {
    btn.disabled = !showRemoveButtons;
    btn.classList.toggle("hidden", !showRemoveButtons);
  });
}

function buildGenericFieldControl(key, value) {
  const row = document.createElement("div");
  row.className = "custom-field-row";

  const wrapper = document.createElement("label");
  wrapper.dataset.fieldKey = key;
  wrapper.dataset.fieldType = "String";
  wrapper.dataset.fieldRequired = "false";
  wrapper.append(key);

  const input = document.createElement("input");
  input.type = "text";
  input.dataset.customInput = "true";
  if (value !== undefined && value !== null) {
    input.value = String(value);
  }
  wrapper.appendChild(input);
  row.appendChild(wrapper);

  const removeBtn = document.createElement("button");
  removeBtn.type = "button";
  removeBtn.className = "danger remove-field-btn";
  removeBtn.textContent = "−";
  removeBtn.title = "Xóa trường";
  removeBtn.addEventListener("click", () => {
    row.remove();
  });
  row.appendChild(removeBtn);

  return row;
}

function renderDynamicCustomFields(customValues = {}) {
  const container = getById("candidate-custom-fields");
  container.innerHTML = "";

  const activeDefinitions = state.fieldDefinitions.filter((d) => d.isActive);
  activeDefinitions.forEach((definition) => {
    const value = customValues[definition.key];
    container.appendChild(buildCustomFieldControl(definition, value));
  });

  const definitionKeys = new Set(activeDefinitions.map((d) => d.key));
  Object.keys(customValues).forEach((key) => {
    if (!definitionKeys.has(key)) {
      container.appendChild(buildGenericFieldControl(key, customValues[key]));
    }
  });
}

function resetCandidateForm() {
  state.selectedCandidateId = null;
  getById("candidate-id").value = "";
  getById("candidate-name").value = "";
  getById("candidate-birth-date").value = "";
  getById("candidate-sex").value = "";
  clearFormErrors("candidate-form-errors");
  renderDynamicCustomFields();
  setCandidateMode("create");
}

function fillCandidateForm(item, mode) {
  state.selectedCandidateId = item.id;
  getById("candidate-id").value = item.id;
  getById("candidate-name").value = item.name || "";
  getById("candidate-birth-date").value = item.birthDate || "";
  getById("candidate-sex").value = item.sex || "";
  renderDynamicCustomFields(item.customFields || {});
  clearFormErrors("candidate-form-errors");
  setCandidateMode(mode);
}

function serializeDynamicFields() {
  const payload = {};
  const errors = [];

  const controls = getById("candidate-custom-fields").querySelectorAll("label[data-field-key]");
  controls.forEach((wrapper) => {
    const key = wrapper.dataset.fieldKey;
    const type = wrapper.dataset.fieldType;
    const required = wrapper.dataset.fieldRequired === "true";
    const input = wrapper.querySelector("[data-custom-input]");

    if (!input) {
      return;
    }

    let value;
    if (type === "Boolean") {
      value = Boolean(input.checked);
    } else {
      value = input.value;
    }

    if (required) {
      if (type === "Boolean") {
        if (!value) {
          errors.push(`${key}: ${t("fieldRequired")}`);
          return;
        }
      } else if (value === "" || value === null || value === undefined) {
        errors.push(`${key}: ${t("fieldRequired")}`);
        return;
      }
    }

    if (type !== "Boolean" && (value === "" || value === null || value === undefined)) {
      return;
    }

    if (type === "Number") {
      const numeric = Number(value);
      if (!Number.isFinite(numeric)) {
        errors.push(`${key}: ${t("mustBeValidNumber")}`);
        return;
      }
      payload[key] = numeric;
      return;
    }

    if (type === "Date") {
      payload[key] = String(value);
      return;
    }

    payload[key] = value;
  });

  return { payload, errors };
}

function validateCoreCandidatePayload(payload) {
  const errors = [];

  if (!payload.name.trim()) {
    errors.push(`name: ${t("nameRequired")}`);
  }

  if (!payload.birthDate) {
    errors.push(`birthDate: ${t("birthDateRequired")}`);
  }

  if (!payload.sex) {
    errors.push(`sex: ${t("sexRequired")}`);
  }

  return errors;
}

function buildCandidatePayload() {
  const dynamic = serializeDynamicFields();

  return {
    payload: {
      name: getById("candidate-name").value.trim(),
      birthDate: getById("candidate-birth-date").value,
      sex: getById("candidate-sex").value,
      customFields: dynamic.payload
    },
    errors: dynamic.errors
  };
}

export async function loadCandidates() {
  const pageNumber = state.candidatePaging.pageNumber;
  const pageSize = state.candidatePaging.pageSize;

  try {
    const result = await get(`/api/v1/candidates?pageNumber=${pageNumber}&pageSize=${pageSize}`);
    state.candidates = result.items || [];
    state.candidatePaging.totalCount = result.totalCount || 0;
    state.candidatePaging.pageNumber = result.pageNumber || pageNumber;
    state.candidatePaging.pageSize = result.pageSize || pageSize;
    renderCandidateRows();
  } catch (error) {
    showNotification(t("failedLoadCandidates"), "error", loadCandidates);
  }
}

async function openCandidate(id, mode) {
  try {
    const item = await get(`/api/v1/candidates/${id}`);
    fillCandidateForm(item, mode);
    showNotification(mode === "view" ? t("candidateLoadedRead") : t("candidateLoadedEdit"));
  } catch (error) {
    const messages = mapApiErrorToMessages(error);
    showNotification(messages[0], "error");

    if (error.status === 404) {
      await loadCandidates();
      navigate("/candidates");
    }
  }
}

async function handleCandidateTableClick(event) {
  const viewId = event.target.dataset.candidateView;
  const editId = event.target.dataset.candidateEdit;
  const deleteId = event.target.dataset.candidateDelete;

  if (viewId) {
    navigate(`/candidates/${viewId}/view`);
    return;
  }

  if (editId) {
    navigate(`/candidates/${editId}/edit`);
    return;
  }

  if (deleteId) {
    if (!confirmDelete(t("deleteCandidateConfirm"))) {
      return;
    }

    try {
      await del(`/api/v1/candidates/${deleteId}`);
      showNotification(t("candidateDeleted"));
      await loadCandidates();
    } catch (error) {
      const messages = mapApiErrorToMessages(error);
      showNotification(messages[0], "error", loadCandidates);
    }
  }
}

async function handleCandidateSubmit(event) {
  event.preventDefault();
  clearFormErrors("candidate-form-errors");

  const { payload, errors: dynamicErrors } = buildCandidatePayload();
  const coreErrors = validateCoreCandidatePayload(payload);
  const errors = [...coreErrors, ...dynamicErrors];
  if (errors.length > 0) {
    showFormErrors("candidate-form-errors", errors);
    return;
  }

  try {
    setBusy("candidate-submit-btn", true, t("saving"));

    if (state.candidateMode === "edit" && state.selectedCandidateId) {
      await put(`/api/v1/candidates/${state.selectedCandidateId}`, payload);
      showNotification(t("candidateUpdated"));
    } else {
      await post("/api/v1/candidates", payload);
      showNotification(t("candidateCreated"));
    }

    await loadCandidates();
    navigate("/candidates");
  } catch (error) {
    const messages = mapApiErrorToMessages(error);
    showFormErrors("candidate-form-errors", messages);

    if (error.status === 404) {
      showNotification(t("candidateNotFound"), "error", loadCandidates);
      await loadCandidates();
      navigate("/candidates");
      return;
    }

    if (error.status === 409) {
      showNotification(t("conflictSavingCandidate"), "error");
      return;
    }

    if (error.status >= 500 || error.status === 0) {
      showNotification(t("serverErrorSavingCandidate"), "error", () => handleCandidateSubmit(event));
    }
  } finally {
    setBusy("candidate-submit-btn", false, t("saving"));
  }
}

function handleAddCustomField() {
  const nameInput = getById("new-custom-field-name");
  const valueInput = getById("new-custom-field-value");
  const key = nameInput.value.trim();
  const value = valueInput.value;

  if (!key) {
    showNotification("Tên trường không được để trống.", "error");
    return;
  }

  const container = getById("candidate-custom-fields");
  const existing = container.querySelector(`label[data-field-key="${CSS.escape(key)}"]`);
  if (existing) {
    showNotification("Trường này đã tồn tại.", "error");
    return;
  }

  const definition = state.fieldDefinitions.find((d) => d.key === key && d.isActive);
  const row = definition
    ? buildCustomFieldControl(definition, value)
    : buildGenericFieldControl(key, value);

  container.appendChild(row);
  nameInput.value = "";
  valueInput.value = "";
  nameInput.focus();
}

function goToPreviousPage() {
  if (state.candidatePaging.pageNumber <= 1) {
    return;
  }

  state.candidatePaging.pageNumber -= 1;
  loadCandidates();
}

function goToNextPage() {
  const totalPages = Math.max(1, Math.ceil(state.candidatePaging.totalCount / state.candidatePaging.pageSize));
  if (state.candidatePaging.pageNumber >= totalPages) {
    return;
  }

  state.candidatePaging.pageNumber += 1;
  loadCandidates();
}

export function rerenderCandidateCustomFieldsPreservingValues() {
  const currentValues = {};
  const controls = getById("candidate-custom-fields").querySelectorAll("label[data-field-key]");
  controls.forEach((wrapper) => {
    const key = wrapper.dataset.fieldKey;
    const type = wrapper.dataset.fieldType;
    const input = wrapper.querySelector("[data-custom-input]");

    if (!input) {
      return;
    }

    currentValues[key] = type === "Boolean" ? Boolean(input.checked) : input.value;
  });

  renderDynamicCustomFields(currentValues);
  setCandidateMode(state.candidateMode);
}

// ── Route handlers (called by router) ──

export function showCandidateList() {
  getById("candidate-list-view").classList.remove("hidden");
  getById("candidate-detail-view").classList.add("hidden");
}

export function showCandidateCreate() {
  resetCandidateForm();
  getById("candidate-list-view").classList.add("hidden");
  getById("candidate-detail-view").classList.remove("hidden");
}

export async function showCandidateDetail(id, mode) {
  getById("candidate-list-view").classList.add("hidden");
  getById("candidate-detail-view").classList.remove("hidden");
  await openCandidate(id, mode);
}

export function initializeCandidateModule() {
  getById("candidate-form").addEventListener("submit", handleCandidateSubmit);
  getById("candidate-table-body").addEventListener("click", handleCandidateTableClick);
  getById("candidate-refresh-btn").addEventListener("click", loadCandidates);
getById("candidate-prev-page").addEventListener("click", goToPreviousPage);
  getById("candidate-next-page").addEventListener("click", goToNextPage);
  getById("add-custom-field-btn").addEventListener("click", handleAddCustomField);
  getById("candidate-new-btn").addEventListener("click", () => navigate("/candidates/new"));
  getById("candidate-back-btn").addEventListener("click", () => navigate("/candidates"));
  getById("candidate-edit-btn").addEventListener("click", () => navigate(`/candidates/${state.selectedCandidateId}/edit`));
}
