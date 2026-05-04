const vi = {
  appTitle: "Quản Lý Nhân Sự Kesa",
  appSubtitle: "Quản Lý Ứng Viên + Trường Thông Tin Động",
  backendLabel: "Backend:",

  tabCandidates: "Ứng Viên",
  tabFields: "Các trường thông tin mặc định",

  candidateListTitle: "Danh Sách Ứng Viên",
  fieldListTitle: "Danh Sách Định Nghĩa Trường",
  refresh: "Làm Mới",
  name: "Họ Tên",
  birthDate: "Ngày Sinh",
  sex: "Giới Tính",
  updated: "Cập Nhật",
  actions: "Thao Tác",
  noCandidates: "Không tìm thấy ứng viên nào.",
  noFields: "Không tìm thấy định nghĩa trường nào.",
  pageLabel: "Trang {0} / {1} (Tổng cộng {2})",
  prev: "Trước",
  next: "Sau",

  candidateFormTitle: "Biểu Mẫu Ứng Viên",
  fieldFormTitle: "Biểu Mẫu Định Nghĩa Trường",
  modeCreate: "Tạo Mới",
  modeEdit: "Chỉnh Sửa",
  modeRead: "Xem",
  selectSex: "Chọn giới tính",
  male: "Nam",
  female: "Nữ",
  other: "Khác",
  customFields: "Trường Thông Tin Tùy Chỉnh",
  createCandidate: "Tạo Ứng Viên",
  updateCandidate: "Cập Nhật Ứng Viên",
  candidateCreated: "Đã tạo ứng viên.",
  candidateUpdated: "Đã cập nhật ứng viên.",
  createField: "Tạo Trường",
  updateField: "Cập Nhật Trường",
  reset: "Đặt Lại",

  fieldName: "Tên trường",
  key: "Khóa",
  type: "Loại",
  required: "Bắt Buộc",
  active: "Kích Hoạt",
  selectType: "Chọn loại",
  string: "Chuỗi",
  number: "Số",
  date: "Ngày",
  boolean: "Đúng/Sai",
  enum: "Liệt Kê",
  enumOptionsLabel: "Tùy Chọn Liệt Kê (phân cách bằng dấu phẩy)",

  yes: "Có",
  no: "Không",
  view: "Xem",
  edit: "Sửa",
  deleteAction: "Xóa",
  selectOption: "Chọn tùy chọn",
  saving: "Đang lưu...",
  retry: "Thử Lại",

  failedLoadCandidates: "Không thể tải danh sách ứng viên.",
  failedLoadFields: "Không thể tải danh sách định nghĩa trường.",
  candidateLoadedRead: "Đã tải ứng viên ở chế độ xem.",
  candidateLoadedEdit: "Đã tải ứng viên để chỉnh sửa.",
  deleteCandidateConfirm: "Xóa hồ sơ ứng viên này?",
  candidateDeleted: "Đã xóa ứng viên.",
  deleteFieldConfirm: "Xóa định nghĩa trường này?",
  fieldDeleted: "Đã xóa định nghĩa trường.",
  candidateNotFound: "Ứng viên không còn tồn tại.",
  conflictSavingCandidate: "Xung đột khi lưu ứng viên.",
  conflictSavingField: "Xung đột khi lưu định nghĩa trường.",
  serverErrorSavingCandidate: "Lỗi máy chủ hoặc mạng khi lưu ứng viên.",
  initApiErrors: "Khởi tạo hoàn tất với lỗi API có thể khôi phục.",

  resourceNotFound: "Không tìm thấy tài nguyên. Có thể đã bị xóa.",
  requestConflict: "Xung đột yêu cầu. Vui lòng kiểm tra khóa trùng lặp hoặc dữ liệu cũ.",
  networkTimeout: "Yêu cầu hết thời gian. Vui lòng thử lại.",
  networkError: "Lỗi mạng. Vui lòng kiểm tra backend đang chạy.",
  apiRequestFailed: "Yêu cầu API thất bại.",

  nameRequired: "Họ tên là bắt buộc.",
  birthDateRequired: "Ngày sinh là bắt buộc.",
  sexRequired: "Giới tính là bắt buộc.",
  fieldNameRequired: "Tên trường là bắt buộc.",
  fieldKeyRequired: "Khóa trường là bắt buộc.",
  dataTypeRequired: "Loại dữ liệu là bắt buộc.",
  enumOptionsRequired: "Trường liệt kê phải có ít nhất một tùy chọn.",
  fieldRequired: "Trường này là bắt buộc.",
  mustBeValidNumber: "Phải là số hợp lệ.",
};

export function t(key, ...args) {
  let text = vi[key] || key;
  if (args.length > 0) {
    args.forEach((arg, i) => {
      text = text.replace(`{${i}}`, String(arg));
    });
  }
  return text;
}

export function i18nInit() {
  document.querySelectorAll("[data-i18n]").forEach((el) => {
    const key = el.dataset.i18n;
    if (key && vi[key]) {
      el.textContent = vi[key];
    }
  });
}
