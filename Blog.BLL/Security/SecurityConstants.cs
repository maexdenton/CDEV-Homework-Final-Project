using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.BLL.Security
{
    /// <summary>
    /// Системные роли приложения
    /// </summary>
    public static class AppRoles
    {
        public const string Administrator = "Администратор";
        public const string Moderator = "Модератор";
        public const string User = "Пользователь";
    }

    /// <summary>
    /// Детализированные права (Permissions), связываемые через Claims с ролями пользователей
    /// </summary>
    public static class AppPermissions
    {
        public const string UsersView = "Permissions.Users.View";
        public const string UsersCreate = "Permissions.Users.Create";
        public const string UsersUpdate = "Permissions.Users.Update";
        public const string UsersDelete = "Permissions.Users.Delete";

        public const string ArticlesView = "Permissions.Articles.View";
        public const string ArticlesCreate = "Permissions.Articles.Create";
        public const string ArticlesUpdate = "Permissions.Articles.Update";
        public const string ArticlesDelete = "Permissions.Articles.Delete";

        public const string CommentsView = "Permissions.Comments.View";
        public const string CommentsCreate = "Permissions.Comments.Create";
        public const string CommentsUpdate = "Permissions.Comments.Update";
        public const string CommentsDelete = "Permissions.Comments.Delete";

        public const string TagsView = "Permissions.Tags.View";
        public const string TagsCreate = "Permissions.Tags.Create";
        public const string TagsUpdate = "Permissions.Tags.Update";
        public const string TagsDelete = "Permissions.Tags.Delete";

        public const string RolesManage = "Permissions.Roles.Manage";

        /// <summary>
        /// Полный перечень всех прав в системе для автоматического связывания с ролью Администратора
        /// </summary>
        public static readonly IReadOnlyList<string> All = new[]
        {
        UsersView, UsersCreate, UsersUpdate, UsersDelete,
        ArticlesView, ArticlesCreate, ArticlesUpdate, ArticlesDelete,
        CommentsView, CommentsCreate, CommentsUpdate, CommentsDelete,
        TagsView, TagsCreate, TagsUpdate, TagsDelete,
        RolesManage
    };
    }

    /// <summary>
    /// Статусы завершения бизнес-операции, исключающие смешивание кодов ошибок в HTTP
    /// </summary>
    public enum ResultStatus
    {
        Success,
        NotFound,
        Forbidden,
        Conflict,
        InvalidData
    }

    /// <summary>
    /// Универсальная обертка результата бизнес-операции со строгой семантикой
    /// </summary>
    public class ServiceResult
    {
        public bool Succeeded => Status == ResultStatus.Success;
        public ResultStatus Status { get; init; }
        public string? Message { get; init; }

        public static ServiceResult Ok() => new() { Status = ResultStatus.Success };
        public static ServiceResult NotFound(string message = "Запрошенный ресурс не найден.") => new() { Status = ResultStatus.NotFound, Message = message };
        public static ServiceResult Forbidden(string message = "У вас недостаточно прав для выполнения данной операции.") => new() { Status = ResultStatus.Forbidden, Message = message };
        public static ServiceResult Conflict(string message) => new() { Status = ResultStatus.Conflict, Message = message };
        public static ServiceResult BadRequest(string message) => new() { Status = ResultStatus.InvalidData, Message = message };
    }

    /// <summary>
    /// Обобщенный результат операции с полезной нагрузкой
    /// </summary>
    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; init; }

        public static ServiceResult<T> Ok(T data) => new() { Status = ResultStatus.Success, Data = data };
        public new static ServiceResult<T> NotFound(string message = "Запрошенный ресурс не найден.") => new() { Status = ResultStatus.NotFound, Message = message };
        public new static ServiceResult<T> Forbidden(string message = "У вас недостаточно прав для выполнения данной операции.") => new() { Status = ResultStatus.Forbidden, Message = message };
        public new static ServiceResult<T> Conflict(string message) => new() { Status = ResultStatus.Conflict, Message = message };
        public new static ServiceResult<T> BadRequest(string message) => new() { Status = ResultStatus.InvalidData, Message = message };
    }
}
