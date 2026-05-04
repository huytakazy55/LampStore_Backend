namespace LampStoreProjects.Helpers
{
    /// <summary>
    /// Centralized error codes and Vietnamese messages for all API responses.
    /// Mã lỗi tập trung và thông báo tiếng Việt cho tất cả API.
    /// </summary>
    public static class ErrorCodes
    {
        // ══════════════════════════════════════════════════════════
        // CHUNG (COMMON)
        // ══════════════════════════════════════════════════════════

        public const string INTERNAL_ERROR = "COMMON_001";
        public const string NOT_FOUND = "COMMON_002";
        public const string UNAUTHORIZED = "COMMON_003";
        public const string FORBIDDEN = "COMMON_004";
        public const string BAD_REQUEST = "COMMON_005";
        public const string VALIDATION_FAILED = "COMMON_006";

        // ══════════════════════════════════════════════════════════
        // TÀI KHOẢN / XÁC THỰC (AUTH)
        // ══════════════════════════════════════════════════════════

        public const string AUTH_EMPTY_CREDENTIALS = "AUTH_001";
        public const string AUTH_INVALID_CREDENTIALS = "AUTH_002";
        public const string AUTH_ACCOUNT_LOCKED = "AUTH_003";
        public const string AUTH_GOOGLE_LOGIN_FAILED = "AUTH_004";
        public const string AUTH_REGISTER_FAILED = "AUTH_005";
        public const string AUTH_REFRESH_TOKEN_REQUIRED = "AUTH_006";
        public const string AUTH_REFRESH_TOKEN_INVALID = "AUTH_007";
        public const string AUTH_REFRESH_TOKEN_NOT_FOUND = "AUTH_008";
        public const string AUTH_PROFILE_NOT_FOUND = "AUTH_009";
        public const string AUTH_ROLE_NOT_FOUND = "AUTH_010";
        public const string AUTH_NO_USERS_FOUND = "AUTH_011";
        public const string AUTH_CANNOT_CREATE_SUPERADMIN = "AUTH_012";
        public const string AUTH_CHANGE_PW_REQUIRED = "AUTH_013";
        public const string AUTH_CHANGE_PW_MIN_LENGTH = "AUTH_014";
        public const string AUTH_CHANGE_PW_WRONG = "AUTH_015";
        public const string AUTH_GUEST_TOKEN_REQUIRED = "AUTH_016";
        public const string AUTH_USER_NOT_FOUND = "AUTH_017";

        // ══════════════════════════════════════════════════════════
        // SẢN PHẨM (PRODUCT)
        // ══════════════════════════════════════════════════════════

        public const string PRODUCT_NOT_FOUND = "PRODUCT_001";
        public const string PRODUCT_CREATE_FAILED = "PRODUCT_002";
        public const string PRODUCT_IMAGE_NOT_FOUND = "PRODUCT_003";
        public const string PRODUCT_NO_FILE = "PRODUCT_004";
        public const string PRODUCT_INVALID_FILE_TYPE = "PRODUCT_005";
        public const string PRODUCT_FILE_TOO_LARGE = "PRODUCT_006";
        public const string PRODUCT_MAX_IMAGES = "PRODUCT_007";
        public const string PRODUCT_SEARCH_FAILED = "PRODUCT_008";
        public const string PRODUCT_IMPORT_EMPTY = "PRODUCT_009";
        public const string PRODUCT_IMPORT_FAILED = "PRODUCT_010";
        public const string PRODUCT_ID_MISMATCH = "PRODUCT_011";
        public const string PRODUCT_VARIANT_NOT_FOUND = "PRODUCT_012";
        public const string PRODUCT_COMPRESS_FAILED = "PRODUCT_013";
        public const string PRODUCT_IMAGE_DIR_NOT_FOUND = "PRODUCT_014";

        // ══════════════════════════════════════════════════════════
        // ĐƠN HÀNG (ORDER)
        // ══════════════════════════════════════════════════════════

        public const string ORDER_NOT_FOUND = "ORDER_001";
        public const string ORDER_GUEST_TOKEN_REQUIRED = "ORDER_002";
        public const string ORDER_INVALID_STATUS = "ORDER_003";
        public const string ORDER_STATUS_TRANSITION_INVALID = "ORDER_004";

        // ══════════════════════════════════════════════════════════
        // GIỎ HÀNG (CART)
        // ══════════════════════════════════════════════════════════

        public const string CART_NOT_FOUND = "CART_001";
        public const string CART_ITEM_NOT_FOUND = "CART_002";
        public const string CART_ID_MISMATCH = "CART_003";

        // ══════════════════════════════════════════════════════════
        // DANH MỤC (CATEGORY)
        // ══════════════════════════════════════════════════════════

        public const string CATEGORY_NOT_FOUND = "CATEGORY_001";
        public const string CATEGORY_ID_MISMATCH = "CATEGORY_002";

        // ══════════════════════════════════════════════════════════
        // FLASH SALE
        // ══════════════════════════════════════════════════════════

        public const string FLASHSALE_NOT_FOUND = "FLASHSALE_001";
        public const string FLASHSALE_ID_MISMATCH = "FLASHSALE_002";

        // ══════════════════════════════════════════════════════════
        // CHAT
        // ══════════════════════════════════════════════════════════

        public const string CHAT_SUBJECT_REQUIRED = "CHAT_001";
        public const string CHAT_MESSAGE_REQUIRED = "CHAT_002";
        public const string CHAT_NOT_FOUND = "CHAT_003";
        public const string CHAT_GUEST_TOKEN_REQUIRED = "CHAT_004";
        public const string CHAT_CREATE_ERROR = "CHAT_005";
        public const string CHAT_SEND_ERROR = "CHAT_006";
        public const string CHAT_GENERAL_ERROR = "CHAT_007";

        // ══════════════════════════════════════════════════════════
        // ĐÁNH GIÁ (REVIEW)
        // ══════════════════════════════════════════════════════════

        public const string REVIEW_PRODUCT_ID_REQUIRED = "REVIEW_001";
        public const string REVIEW_INVALID_RATING = "REVIEW_002";
        public const string REVIEW_PURCHASE_REQUIRED = "REVIEW_003";
        public const string REVIEW_ALREADY_REVIEWED = "REVIEW_004";
        public const string REVIEW_PRODUCT_NOT_FOUND = "REVIEW_005";

        // ══════════════════════════════════════════════════════════
        // YÊU THÍCH (WISHLIST)
        // ══════════════════════════════════════════════════════════

        public const string WISHLIST_ALREADY_EXISTS = "WISHLIST_001";
        public const string WISHLIST_NOT_FOUND = "WISHLIST_002";

        // ══════════════════════════════════════════════════════════
        // TIN TỨC (NEWS)
        // ══════════════════════════════════════════════════════════

        public const string NEWS_NOT_FOUND = "NEWS_001";
        public const string NEWS_NO_FILE = "NEWS_002";

        // ══════════════════════════════════════════════════════════
        // BANNER
        // ══════════════════════════════════════════════════════════

        public const string BANNER_NOT_FOUND = "BANNER_001";
        public const string BANNER_ID_MISMATCH = "BANNER_002";
        public const string BANNER_NO_FILE = "BANNER_003";
        public const string BANNER_INVALID_FILE_TYPE = "BANNER_004";
        public const string BANNER_FILE_TOO_LARGE = "BANNER_005";

        // ══════════════════════════════════════════════════════════
        // HỒ SƠ NGƯỜI DÙNG (USER PROFILE)
        // ══════════════════════════════════════════════════════════

        public const string PROFILE_NO_FILE = "PROFILE_001";
        public const string PROFILE_USER_NOT_FOUND = "PROFILE_002";
        public const string PROFILE_ID_MISMATCH = "PROFILE_003";

        // ══════════════════════════════════════════════════════════
        // TAG
        // ══════════════════════════════════════════════════════════

        public const string TAG_NOT_FOUND = "TAG_001";
        public const string TAG_ID_MISMATCH = "TAG_002";

        // ══════════════════════════════════════════════════════════
        // CHECK-IN
        // ══════════════════════════════════════════════════════════

        public const string CHECKIN_NOT_FOUND = "CHECKIN_001";
        public const string CHECKIN_ID_MISMATCH = "CHECKIN_002";

        // ══════════════════════════════════════════════════════════
        // VẬN CHUYỂN (DELIVERY)
        // ══════════════════════════════════════════════════════════

        public const string DELIVERY_NOT_FOUND = "DELIVERY_001";
        public const string DELIVERY_ID_MISMATCH = "DELIVERY_002";

        // ══════════════════════════════════════════════════════════
        // BẢN ĐỒ THÔNG BÁO TIẾNG VIỆT
        // ══════════════════════════════════════════════════════════

        private static readonly Dictionary<string, string> _messages = new()
        {
            // ── Chung ──
            [INTERNAL_ERROR] = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
            [NOT_FOUND] = "Không tìm thấy dữ liệu yêu cầu.",
            [UNAUTHORIZED] = "Bạn chưa đăng nhập hoặc phiên đăng nhập đã hết hạn.",
            [FORBIDDEN] = "Bạn không có quyền truy cập tài nguyên này.",
            [BAD_REQUEST] = "Yêu cầu không hợp lệ.",
            [VALIDATION_FAILED] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.",

            // ── Tài khoản ──
            [AUTH_EMPTY_CREDENTIALS] = "Tên đăng nhập hoặc mật khẩu không được để trống.",
            [AUTH_INVALID_CREDENTIALS] = "Sai tên đăng nhập hoặc mật khẩu.",
            [AUTH_ACCOUNT_LOCKED] = "Tài khoản của bạn đã bị khóa! Liên hệ Admin để biết thêm thông tin.",
            [AUTH_GOOGLE_LOGIN_FAILED] = "Đăng nhập Google thất bại.",
            [AUTH_REGISTER_FAILED] = "Đăng ký tài khoản thất bại.",
            [AUTH_REFRESH_TOKEN_REQUIRED] = "Token làm mới là bắt buộc.",
            [AUTH_REFRESH_TOKEN_INVALID] = "Token làm mới không hợp lệ hoặc đã hết hạn.",
            [AUTH_REFRESH_TOKEN_NOT_FOUND] = "Không tìm thấy token làm mới.",
            [AUTH_PROFILE_NOT_FOUND] = "Không tìm thấy hồ sơ người dùng.",
            [AUTH_ROLE_NOT_FOUND] = "Không tìm thấy vai trò.",
            [AUTH_NO_USERS_FOUND] = "Không tìm thấy người dùng nào.",
            [AUTH_CANNOT_CREATE_SUPERADMIN] = "Không thể tạo mới quyền SuperAdmin.",
            [AUTH_CHANGE_PW_REQUIRED] = "Vui lòng nhập đầy đủ mật khẩu hiện tại và mật khẩu mới.",
            [AUTH_CHANGE_PW_MIN_LENGTH] = "Mật khẩu mới phải có ít nhất 6 ký tự.",
            [AUTH_CHANGE_PW_WRONG] = "Mật khẩu hiện tại không đúng.",
            [AUTH_GUEST_TOKEN_REQUIRED] = "GuestToken là bắt buộc.",
            [AUTH_USER_NOT_FOUND] = "Người dùng không tồn tại.",

            // ── Sản phẩm ──
            [PRODUCT_NOT_FOUND] = "Không tìm thấy sản phẩm.",
            [PRODUCT_CREATE_FAILED] = "Lỗi khi tạo sản phẩm.",
            [PRODUCT_IMAGE_NOT_FOUND] = "Hình ảnh không tồn tại.",
            [PRODUCT_NO_FILE] = "Vui lòng chọn file để tải lên.",
            [PRODUCT_INVALID_FILE_TYPE] = "File không hợp lệ. Chỉ chấp nhận JPEG, PNG, GIF, WebP.",
            [PRODUCT_FILE_TOO_LARGE] = "File vượt quá 5MB.",
            [PRODUCT_MAX_IMAGES] = "Đã đạt giới hạn số lượng ảnh tối đa.",
            [PRODUCT_SEARCH_FAILED] = "Lỗi khi tìm kiếm sản phẩm.",
            [PRODUCT_IMPORT_EMPTY] = "Không có dữ liệu sản phẩm để import.",
            [PRODUCT_IMPORT_FAILED] = "Có lỗi xảy ra khi import sản phẩm.",
            [PRODUCT_ID_MISMATCH] = "ID sản phẩm không khớp.",
            [PRODUCT_VARIANT_NOT_FOUND] = "Không tìm thấy biến thể sản phẩm.",
            [PRODUCT_COMPRESS_FAILED] = "Lỗi khi nén ảnh.",
            [PRODUCT_IMAGE_DIR_NOT_FOUND] = "Thư mục ảnh không tồn tại.",

            // ── Đơn hàng ──
            [ORDER_NOT_FOUND] = "Không tìm thấy đơn hàng.",
            [ORDER_GUEST_TOKEN_REQUIRED] = "GuestToken là bắt buộc cho đơn hàng khách vãng lai.",
            [ORDER_INVALID_STATUS] = "Trạng thái đơn hàng không hợp lệ.",
            [ORDER_STATUS_TRANSITION_INVALID] = "Không thể chuyển trạng thái đơn hàng. Trạng thái phải được cập nhật theo thứ tự.",

            // ── Giỏ hàng ──
            [CART_NOT_FOUND] = "Không tìm thấy giỏ hàng.",
            [CART_ITEM_NOT_FOUND] = "Không tìm thấy sản phẩm trong giỏ hàng.",
            [CART_ID_MISMATCH] = "ID giỏ hàng không khớp.",

            // ── Danh mục ──
            [CATEGORY_NOT_FOUND] = "Không tìm thấy danh mục.",
            [CATEGORY_ID_MISMATCH] = "ID danh mục không khớp.",

            // ── Flash Sale ──
            [FLASHSALE_NOT_FOUND] = "Không tìm thấy chương trình Flash Sale.",
            [FLASHSALE_ID_MISMATCH] = "ID Flash Sale không khớp.",

            // ── Chat ──
            [CHAT_SUBJECT_REQUIRED] = "Tiêu đề cuộc hội thoại là bắt buộc.",
            [CHAT_MESSAGE_REQUIRED] = "Nội dung tin nhắn không được để trống.",
            [CHAT_NOT_FOUND] = "Không tìm thấy cuộc hội thoại.",
            [CHAT_GUEST_TOKEN_REQUIRED] = "Header X-Guest-Token là bắt buộc.",
            [CHAT_CREATE_ERROR] = "Có lỗi xảy ra khi tạo cuộc hội thoại.",
            [CHAT_SEND_ERROR] = "Có lỗi xảy ra khi gửi tin nhắn.",
            [CHAT_GENERAL_ERROR] = "Có lỗi xảy ra.",

            // ── Đánh giá ──
            [REVIEW_PRODUCT_ID_REQUIRED] = "ProductId là bắt buộc.",
            [REVIEW_INVALID_RATING] = "Đánh giá phải từ 1 đến 5 sao.",
            [REVIEW_PURCHASE_REQUIRED] = "Bạn cần mua sản phẩm này trước khi đánh giá.",
            [REVIEW_ALREADY_REVIEWED] = "Bạn đã đánh giá sản phẩm này rồi.",
            [REVIEW_PRODUCT_NOT_FOUND] = "Sản phẩm không tồn tại.",

            // ── Yêu thích ──
            [WISHLIST_ALREADY_EXISTS] = "Sản phẩm đã có trong danh sách yêu thích hoặc không tồn tại.",
            [WISHLIST_NOT_FOUND] = "Sản phẩm không có trong danh sách yêu thích.",

            // ── Tin tức ──
            [NEWS_NOT_FOUND] = "Không tìm thấy bài viết.",
            [NEWS_NO_FILE] = "Vui lòng chọn file để tải lên.",

            // ── Banner ──
            [BANNER_NOT_FOUND] = "Không tìm thấy banner.",
            [BANNER_ID_MISMATCH] = "ID banner không khớp.",
            [BANNER_NO_FILE] = "Vui lòng chọn file để tải lên.",
            [BANNER_INVALID_FILE_TYPE] = "File không hợp lệ. Chỉ chấp nhận JPG, JPEG, PNG, GIF.",
            [BANNER_FILE_TOO_LARGE] = "Kích thước file quá lớn. Tối đa 5MB.",

            // ── Hồ sơ người dùng ──
            [PROFILE_NO_FILE] = "Vui lòng chọn ảnh đại diện để tải lên.",
            [PROFILE_USER_NOT_FOUND] = "Không tìm thấy người dùng.",
            [PROFILE_ID_MISMATCH] = "ID hồ sơ không khớp.",

            // ── Tag ──
            [TAG_NOT_FOUND] = "Không tìm thấy thẻ.",
            [TAG_ID_MISMATCH] = "ID thẻ không khớp.",

            // ── Check-in ──
            [CHECKIN_NOT_FOUND] = "Không tìm thấy dữ liệu check-in.",
            [CHECKIN_ID_MISMATCH] = "ID check-in không khớp.",

            // ── Vận chuyển ──
            [DELIVERY_NOT_FOUND] = "Không tìm thấy phương thức vận chuyển.",
            [DELIVERY_ID_MISMATCH] = "ID vận chuyển không khớp.",
        };

        /// <summary>
        /// Lấy thông báo tiếng Việt theo mã lỗi.
        /// </summary>
        public static string GetMessage(string errorCode)
        {
            return _messages.TryGetValue(errorCode, out var message) ? message : "Đã xảy ra lỗi không xác định.";
        }
    }
}
