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

const decisionModal = document.querySelector("[data-decision-modal]");
if (decisionModal) {
  const decisionForm = decisionModal.querySelector("[data-decision-form]");
  const requestIdInput = decisionModal.querySelector("[data-decision-request-id]");
  const requestLabel = decisionModal.querySelector("[data-decision-request-label]");
  const factPreview = decisionModal.querySelector("[data-decision-fact]");
  const title = decisionModal.querySelector("[data-decision-title]");
  const noteLabel = decisionModal.querySelector("[data-decision-note-label]");
  const note = decisionModal.querySelector("[data-decision-note]");
  const validation = decisionModal.querySelector("[data-decision-validation]");
  const characterCount = decisionModal.querySelector("[data-decision-character-count]");
  const confirmButton = decisionModal.querySelector("[data-decision-confirm]");
  let activeTrigger = null;
  let decision = null;

  const closeDecisionModal = () => {
    if (decisionModal.open) decisionModal.close();
  };

  const updateDecisionCount = () => {
    characterCount.textContent = note.value.length.toString();
    note.setCustomValidity("");
    validation.textContent = "";
  };

  document.querySelectorAll("[data-decision-trigger]").forEach((trigger) => {
    trigger.addEventListener("click", () => {
      activeTrigger = trigger;
      decision = trigger.dataset.decision;
      const isReject = decision === "reject";

      decisionForm.action = trigger.dataset.actionUrl;
      requestIdInput.value = trigger.dataset.requestId;
      requestLabel.textContent = `Request #${trigger.dataset.requestId}`;
      factPreview.textContent = trigger.dataset.fact;
      title.textContent = isReject ? "Reject deletion request" : "Approve deletion request";
      noteLabel.textContent = isReject ? "Rejection reason" : "Approval note (optional)";
      note.placeholder = isReject
        ? "Explain why this request is rejected."
        : "Add context for the audit trail if needed.";
      note.required = isReject;
      note.minLength = isReject ? 10 : 0;
      note.value = "";
      confirmButton.textContent = isReject ? "Reject request" : "Approve request";
      confirmButton.classList.toggle("reject", isReject);
      updateDecisionCount();
      decisionModal.showModal();
      note.focus();
    });
  });

  decisionModal.querySelectorAll("[data-decision-close]").forEach((button) => {
    button.addEventListener("click", closeDecisionModal);
  });

  decisionModal.addEventListener("click", (event) => {
    if (event.target === decisionModal) closeDecisionModal();
  });

  decisionModal.addEventListener("close", () => {
    activeTrigger?.focus();
    activeTrigger = null;
    decision = null;
  });

  note.addEventListener("input", updateDecisionCount);

  decisionForm.addEventListener("submit", (event) => {
    note.value = note.value.trim();
    if (decision === "reject" && note.value.length < 10) {
      event.preventDefault();
      const message = "Rejection reason must contain at least 10 characters.";
      validation.textContent = message;
      note.setCustomValidity(message);
      note.reportValidity();
      return;
    }

    confirmButton.disabled = true;
    confirmButton.textContent = decision === "reject" ? "Rejecting..." : "Approving...";
  });
}
