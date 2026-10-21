namespace Blog.BLL.Security
{
    /// <summary>
    /// Содержит константы, используемые при настройке аутентификации, авторизации и JWT-токенов.
    /// </summary>
    public static class SecurityConstants
    {
        /// <summary>
        /// Названия ролей, применяемые в системе.
        /// </summary>
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Moderator = "Moderator";
            public const string User = "User";
        }

        /// <summary>
        /// Названия политик авторизации.
        /// </summary>
        public static class Policies
        {
            public const string RequireAdminRole = "RequireAdminRole";
            public const string RequireModeratorRole = "RequireModeratorRole";
            public const string RequireAuthenticatedUser = "RequireAuthenticatedUser";
        }

        /// <summary>
        /// Типы утверждений (claims), используемые в токенах.
        /// </summary>
        public static class ClaimTypes
        {
            public const string UserId = "userId";
            public const string UserName = "userName";
            public const string Email = "email";
            public const string Role = "role";
        }

        /// <summary>
        /// Параметры JWT-токена.
        /// </summary>
        public static class Jwt
        {
            public const string Issuer = "Blog.Api";
            public const string Audience = "Blog.Client";

            // Срок жизни access-токена намеренно короткий: клиент обновляет его через refresh-токен.
            public const int AccessTokenLifetimeMinutes = 15;
            public const int RefreshTokenLifetimeDays = 7;

            public const string SecretKeyConfigurationName = "Jwt:SecretKey";
        }

        /// <summary>
        /// Имена cookie, используемых для аутентификации.
        /// </summary>
        public static class Cookies
        {
            public const string AccessToken = "access_token";
            public const string RefreshToken = "refresh_token";
        }
    }
}
