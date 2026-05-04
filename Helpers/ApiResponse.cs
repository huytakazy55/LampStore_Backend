namespace LampStoreProjects.Helpers
{
    /// <summary>
    /// Standardized API error response format.
    /// Chuẩn hoá format response lỗi cho tất cả API.
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>Mã lỗi (VD: "AUTH_001")</summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>Thông báo lỗi tiếng Việt cho người dùng</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Chi tiết lỗi bổ sung (chỉ hiển thị trong Development)</summary>
        public string? Detail { get; set; }

        /// <summary>Danh sách lỗi validation chi tiết (nếu có)</summary>
        public object? Errors { get; set; }

        /// <summary>
        /// Kiểm tra có phải môi trường Development không.
        /// Nếu KHÔNG phải Development, Detail từ exception sẽ bị ẩn để tránh lộ thông tin nhạy cảm.
        /// </summary>
        private static bool IsDevelopment =>
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        public ApiErrorResponse() { }

        public ApiErrorResponse(string errorCode, string? detail = null, object? errors = null)
        {
            ErrorCode = errorCode;
            Message = ErrorCodes.GetMessage(errorCode);
            Detail = detail;
            Errors = errors;
        }

        /// <summary>
        /// Tạo response lỗi nhanh từ mã lỗi.
        /// Detail luôn hiển thị (dùng cho thông tin an toàn, VD: "Bạn chỉ có thể upload tối đa 5 ảnh").
        /// </summary>
        public static ApiErrorResponse FromCode(string errorCode, string? detail = null)
        {
            return new ApiErrorResponse(errorCode, detail);
        }

        /// <summary>
        /// Tạo response lỗi với exception detail.
        /// Detail chỉ hiển thị trong môi trường Development, production sẽ ẩn để bảo mật.
        /// Dùng khi detail chứa ex.Message hoặc thông tin nội bộ.
        /// </summary>
        public static ApiErrorResponse FromException(string errorCode, Exception ex)
        {
            return new ApiErrorResponse(errorCode, IsDevelopment ? ex.Message : null);
        }

        /// <summary>
        /// Tạo response lỗi với danh sách lỗi validation.
        /// </summary>
        public static ApiErrorResponse WithErrors(string errorCode, object errors, string? detail = null)
        {
            return new ApiErrorResponse(errorCode, detail, errors);
        }
    }

    /// <summary>
    /// Standardized API success response format.
    /// Chuẩn hoá format response thành công cho tất cả API.
    /// </summary>
    public class ApiSuccessResponse
    {
        /// <summary>Thông báo thành công tiếng Việt</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Dữ liệu trả về (nếu có)</summary>
        public object? Data { get; set; }

        public ApiSuccessResponse(string message, object? data = null)
        {
            Message = message;
            Data = data;
        }
    }
}
