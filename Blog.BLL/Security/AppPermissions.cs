using System.Collections.Generic;

namespace Blog.BLL.Security
{
    /// <summary>
    /// Разрешения (permissions), используемые в политиках авторизации.
    /// Каждое разрешение превращается в claim типа "permission" и проверяется
    /// через политику с тем же именем (см. Program.cs и DbInitializer).
    /// </summary>
    public static class AppPermissions
    {
        // Статьи
        public const string ArticlesView = "Articles.View";
        public const string ArticlesCreate = "Articles.Create";
        public const string ArticlesUpdate = "Articles.Update";
        public const string ArticlesDelete = "Articles.Delete";

        // Комментарии
        public const string CommentsView = "Comments.View";
        public const string CommentsCreate = "Comments.Create";
        public const string CommentsUpdate = "Comments.Update";
        public const string CommentsDelete = "Comments.Delete";

        // Теги
        public const string TagsView = "Tags.View";
        public const string TagsCreate = "Tags.Create";
        public const string TagsUpdate = "Tags.Update";
        public const string TagsDelete = "Tags.Delete";

        // Пользователи и роли
        public const string UsersView = "Users.View";
        public const string UsersDelete = "Users.Delete";
        public const string RolesManage = "Roles.Manage";

        /// <summary>
        /// Полный список разрешений — используется для назначения прав роли
        /// «Администратор» и для регистрации политик в DI.
        /// </summary>
        public static readonly IReadOnlyList<string> All = new[]
        {
            ArticlesView, ArticlesCreate, ArticlesUpdate, ArticlesDelete,
            CommentsView, CommentsCreate, CommentsUpdate, CommentsDelete,
            TagsView, TagsCreate, TagsUpdate, TagsDelete,
            UsersView, UsersDelete, RolesManage
        };
    }
}
