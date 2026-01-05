namespace LampStoreProjects.Models
{
    /// <summary>
    /// Model trả về khi login hoặc refresh token
    /// </summary>
    public class TokenResponseModel
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; } // Thời gian hết hạn của Access Token (seconds)
        public string TokenType { get; set; } = "Bearer";
    }

    /// <summary>
    /// Model request để refresh token
    /// </summary>
    public class RefreshTokenRequestModel
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}

