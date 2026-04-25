import { del, get, post, put } from "./api.js";
import { state } from "./state.js";
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

function activeFieldDefinitions() {
  return state.fieldDefinitions.filter((item) => item.isActive);
}

function formatDateOnly(value) {
  if (!value) {
    return "-";
  }
  return value;
}

function buildCustomFieldControl(definition, value) {
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
      defaultOption.textContent = "Select option";
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

  return wrapper;
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
            <button class="secondary" data-candidate-view="${item.id}">View</button>
            <button class="secondary" data-candidate-edit="${item.id}">Edit</button>
            <button class="danger" data-candidate-delete="${item.id}">Delete</button>
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

  getById("candidate-page-label").textContent = `Page ${currentPage} / ${totalPages} (Total ${totalCount})`;
  getById("candidate-prev-page").disabled = currentPage <= 1;
  getById("candidate-next-page").disabled = currentPage >= totalPages;
}

function setCandidateMode(mode) {
  state.candidateMode = mode;

  const modeBadge = getById("candidate-mode-badge");
  const submitButton = getById("candidate-submit-btn");

  if (mode === "edit") {
    modeBadge.textContent = "Edit";
    submitButton.textContent = "Update Candidate";
  } else if (mode === "view") {
    modeBadge.textContent = "Read";
    submitButton.textContent = "Create Candidate";
  } else {
    modeBadge.textContent = "Create";
    submitButton.textContent = "Create Candidate";
  }

  const isReadonly = mode === "view";
  getById("candidate-form").querySelectorAll("input,select").forEach((node) => {
    if (node.id !== "candidate-id") {
      node.disabled = isReadonly;
    }
  });
  submitButton.disabled = isReadonly;
}

function renderDynamicCustomFields(customValues = {}) {
  const container = getById("candidate-custom-fields");
  container.innerHTML = "";

  activeFieldDefinitions().forEach((definition) => {
    const value = customValues[definition.key];
    container.appendChild(buildCustomFieldControl(definition, value));
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
          errors.push(`${key}: This field is required.`);
          return;
        }
      } else if (value === "" || value === null || value === undefined) {
        errors.push(`${key}: This field is required.`);
        return;
      }
    }

    if (type !== "Boolean" && (value === "" || value === null || value === undefined)) {
      return;
    }

    if (type === "Number") {
      const numeric = Number(value);
      if (!Number.isFinite(numeric)) {
        errors.push(`${key}: Must be a valid number.`);
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
    errors.push("name: Name is required.");
  }

  if (!payload.birthDate) {
    errors.push("birthDate: Birth date is required.");
  }

  if (!payload.sex) {
    errors.push("sex: Sex is required.");
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
    showNotification("Failed to load candidates.", "error", loadCandidates);
  }
}

async function openCandidate(id, mode) {
  try {
    const item = await get(`/api/v1/candidates/${id}`);
    fillCandidateForm(item, mode);
    showNotification(mode === "view" ? "Candidate loaded in read mode." : "Candidate loaded for edit.");
  } catch (error) {
    const messages = mapApiErrorToMessages(error);
    showNotification(messages[0], "error", loadCandidates);

    if (error.status === 404) {
      await loadCandidates();
      resetCandidateForm();
    }
  }
}

async function handleCandidateTableClick(event) {
  const viewId = event.target.dataset.candidateView;
  const editId = event.target.dataset.candidateEdit;
  const deleteId = event.target.dataset.candidateDelete;

  if (viewId) {
    await openCandidate(viewId, "view");
    return;
  }

  if (editId) {
    await openCandidate(editId, "edit");
    return;
  }

  if (deleteId) {
    if (!confirmDelete("Delete this candidate profile?")) {
      return;
    }

    try {
      await del(`/api/v1/candidates/${deleteId}`);
      showNotification("Candidate deleted.");
      await loadCandidates();

      if (state.selectedCandidateId === deleteId) {
        resetCandidateForm();
      }
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
    setBusy("candidate-submit-btn", true, "Saving...");

    if (state.candidateMode === "edit" && state.selectedCandidateId) {
      await put(`/api/v1/candidates/${state.selectedCandidateId}`, payload);
      showNotification("Candidate updated.");
    } else {
      await post("/api/v1/candidates", payload);
      showNotification("Candidate created.");
    }

    await loadCandidates();
    resetCandidateForm();
  } catch (error) {
    const messages = mapApiErrorToMessages(error);
    showFormErrors("candidate-form-errors", messages);

    if (error.status === 404) {
      showNotification("Candidate no longer exists.", "error", loadCandidates);
      await loadCandidates();
      resetCandidateForm();
      return;
    }

    if (error.status === 409) {
      showNotification("Conflict while saving candidate.", "error");
      return;
    }

    if (error.status >= 500 || error.status === 0) {
      showNotification("Server or network error while saving candidate.", "error", () => handleCandidateSubmit(event));
    }
  } finally {
    setBusy("candidate-submit-btn", false, "Saving...");
  }
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
}

export function initializeCandidateModule() {
  getById("candidate-form").addEventListener("submit", handleCandidateSubmit);
  getById("candidate-table-body").addEventListener("click", handleCandidateTableClick);
  getById("candidate-refresh-btn").addEventListener("click", loadCandidates);
  getById("candidate-cancel-btn").addEventListener("click", resetCandidateForm);
  getById("candidate-prev-page").addEventListener("click", goToPreviousPage);
  getById("candidate-next-page").addEventListener("click", goToNextPage);

  resetCandidateForm();
}
