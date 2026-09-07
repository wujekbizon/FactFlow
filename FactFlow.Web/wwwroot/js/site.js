document.querySelectorAll("[data-fetch-form]").forEach((form) => {
  form.addEventListener("submit", () => {
    const button = form.querySelector("[data-fetch-button]");
    if (!button) return;

    button.disabled = true;
    button.querySelector("span:first-child").textContent = "Downloading...";
  });
});

document.querySelectorAll("[data-fact-input]").forEach((input) => {
  const counter = document.querySelector("[data-character-count]");
  if (!counter) return;

  const form = input.closest("[data-workspace-id]");
  const workspaceId = form?.dataset.workspaceId;
  const draftKey = workspaceId ? `FactFlow.Draft.${workspaceId}` : null;

  if (draftKey && form?.dataset.clearDraft === "true") {
    sessionStorage.removeItem(draftKey);
  } else if (draftKey) {
    input.value = sessionStorage.getItem(draftKey) ?? input.value;
  }

  const updateCounter = () => {
    counter.textContent = input.value.length.toString();
    if (draftKey) sessionStorage.setItem(draftKey, input.value);
  };

  input.addEventListener("input", updateCounter);
  updateCounter();
});

document.querySelectorAll("[data-close-workspace]").forEach((form) => {
  form.addEventListener("submit", () => {
    sessionStorage.removeItem(`FactFlow.Draft.${form.dataset.closeWorkspace}`);
  });
});
