import { del, get, post, put } from "./api.js";
import { state } from "./state.js";
import {
  clearFormErrors,
  confirmDelete,
  mapApiErrorToMessages,
  setBusy,
  showFormErrors,
  showNotification
} from "./ui.js";

function getById(id) {
  return document.getElementById(id);
}

function parseCsvOptions(input) {
  return input
    .split(",")
    .map((item) => item.trim())
    .filter(Boolean);
}

function renderFieldRows() {
  const body = getById("field-table-body");
  const empty = getById("field-empty");

  if (!state.fieldDefinitions.length) {
    body.innerHTML = "";
    empty.classList.remove("hidden");
    return;
  }

  empty.classList.add("hidden");
  body.innerHTML = state.fieldDefinitions
    .map(
      (item) => `
        <tr>
          <td>${item.name}</td>
          <td><code>${item.key}</code></td>
          <td>${item.dataType}</td>
          <td>${item.isRequired ? "Yes" : "No"}</td>
          <td>${item.isActive ? "Yes" : "No"}</td>
          <td>
            <div class="row-actions">
              <button class="secondary" data-field-view="${item.id}">View</button>
              <button class="secondary" data-field-edit="${item.id}">Edit</button>
              <button class="danger" data-field-delete="${item.id}">Delete</button>
            </div>
          </td>
        </tr>
      `
    )
    .join("");
}

function resetFieldForm() {
  state.fieldMode = "create";
  state.selectedFieldId = null;

  getById("field-id").value = "";
  getById("field-name").value = "";
  getById("field-key").value = "";
  getById("field-data-type").value = "";
  getById("field-is-required").checked = false;
  getById("field-is-active").checked = true;
  getById("field-options").value = "";
  getById("field-options-wrapper").classList.add("hidden");

  getById("field-mode-badge").textContent = "Create";
  getById("field-submit-btn").textContent = "Create Field";
  getById("field-form").querySelectorAll("input,select,button").forEach((node) => {
    if (node.id !== "field-submit-btn" && node.id !== "field-cancel-btn") {
      node.disabled = false;
    }
  });

  clearFormErrors("field-form-errors");
}

function fillFieldForm(item, mode = "edit") {
  state.fieldMode = mode;
  state.selectedFieldId = item.id;

  getById("field-id").value = item.id;
  getById("field-name").value = item.name;
  getById("field-key").value = item.key;
  getById("field-data-type").value = item.dataType;
  getById("field-is-required").checked = Boolean(item.isRequired);
  getById("field-is-active").checked = Boolean(item.isActive);
  getById("field-options").value = Array.isArray(item.options) ? item.options.join(",") : "";

  const isEnum = item.dataType === "Enum";
  getById("field-options-wrapper").classList.toggle("hidden", !isEnum);

  const isView = mode === "view";
  getById("field-mode-badge").textContent = isView ? "Read" : "Edit";
  getById("field-submit-btn").textContent = isView ? "Create Field" : "Update Field";

  getById("field-form").querySelectorAll("input,select").forEach((node) => {
    node.disabled = isView;
  });
  getById("field-submit-btn").disabled = isView;

  clearFormErrors("field-form-errors");
}

function validateFieldPayload(payload) {
  const errors = [];

  if (!payload.name.trim()) {
    errors.push("name: Name is required.");
  }

  if (!payload.key.trim()) {
    errors.push("key: Key is required.");
  }

  if (!payload.dataType) {
    errors.push("dataType: Data type is required.");
  }

  if (payload.dataType === "Enum") {
    if (!Array.isArray(payload.options) || !payload.options.length) {
      errors.push("options: Enum fields require at least one option.");
    }
  }

  return errors;
}

function buildFieldPayload() {
  const dataType = getById("field-data-type").value;
  const isEnum = dataType === "Enum";

  return {
    name: getById("field-name").value.trim(),
    key: getById("field-key").value.trim(),
    dataType,
    isRequired: getById("field-is-required").checked,
    isActive: getById("field-is-active").checked,
    options: isEnum ? parseCsvOptions(getById("field-options").value) : null
  };
}

export async function loadFieldDefinitions() {
  try {
    const result = await get("/api/v1/admin/profile-fields");
    state.fieldDefinitions = Array.isArray(result) ? result : [];
    renderFieldRows();
  } catch (error) {
    showNotification("Failed to load field definitions.", "error", loadFieldDefinitions);
  }
}

async function handleFieldSubmit(event) {
  event.preventDefault();

  clearFormErrors("field-form-errors");
  const payload = buildFieldPayload();
  const clientErrors = validateFieldPayload(payload);
  if (clientErrors.length > 0) {
    showFormErrors("field-form-errors", clientErrors);
    return;
  }

  try {
    setBusy("field-submit-btn", true, "Saving...");

    if (state.fieldMode === "edit" && state.selectedFieldId) {
      await put(`/api/v1/admin/profile-fields/${state.selectedFieldId}`, payload);
      showNotification("Field definition updated.");
    } else {
      await post("/api/v1/admin/profile-fields", payload);
      showNotification("Field definition created.");
    }

    await loadFieldDefinitions();
    resetFieldForm();
  } catch (error) {
    const messages = mapApiErrorToMessages(error);
    showFormErrors("field-form-errors", messages);

    if (error.status === 409) {
      showNotification("Conflict while saving field definition.", "error");
    }
  } finally {
    setBusy("field-submit-btn", false, "Saving...");
  }
}

async function handleFieldTableClick(event) {
  const viewId = event.target.dataset.fieldView;
  const editId = event.target.dataset.fieldEdit;
  const deleteId = event.target.dataset.fieldDelete;

  if (viewId) {
    const record = state.fieldDefinitions.find((item) => item.id === viewId);
    if (record) {
      fillFieldForm(record, "view");
    }
    return;
  }

  if (editId) {
    const record = state.fieldDefinitions.find((item) => item.id === editId);
    if (record) {
      fillFieldForm(record, "edit");
    }
    return;
  }

  if (deleteId) {
    if (!confirmDelete("Delete this field definition?")) {
      return;
    }

    try {
      await del(`/api/v1/admin/profile-fields/${deleteId}`);
      showNotification("Field definition deleted.");
      await loadFieldDefinitions();

      if (state.selectedFieldId === deleteId) {
        resetFieldForm();
      }
    } catch (error) {
      const messages = mapApiErrorToMessages(error);
      showNotification(messages[0], "error", loadFieldDefinitions);
    }
  }
}

function handleDataTypeChange() {
  const dataType = getById("field-data-type").value;
  const isEnum = dataType === "Enum";
  getById("field-options-wrapper").classList.toggle("hidden", !isEnum);
  if (!isEnum) {
    getById("field-options").value = "";
  }
}

export function initializeFieldModule() {
  getById("field-form").addEventListener("submit", handleFieldSubmit);
  getById("field-table-body").addEventListener("click", handleFieldTableClick);
  getById("field-data-type").addEventListener("change", handleDataTypeChange);
  getById("field-refresh-btn").addEventListener("click", loadFieldDefinitions);
  getById("field-cancel-btn").addEventListener("click", resetFieldForm);

  resetFieldForm();
}
