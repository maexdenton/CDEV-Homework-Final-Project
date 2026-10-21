namespace Blog.BLL.Security
{
    /// <summary>
    /// Названия ролей, используемых в системе.
    /// Строковые значения совпадают с именами ролей Identity.
    /// </summary>
    public static class AppRoles
    {
        public const string Administrator = "Administrator";
        public const string Moderator = "Moderator";
        public const string User = "User";
    }
}
