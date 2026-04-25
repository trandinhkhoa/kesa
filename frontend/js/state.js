export const state = {
  activeTab: "candidates",
  candidateMode: "create",
  fieldMode: "create",
  candidatePaging: {
    pageNumber: 1,
    pageSize: 20,
    totalCount: 0
  },
  candidates: [],
  selectedCandidateId: null,
  fieldDefinitions: [],
  selectedFieldId: null
};
