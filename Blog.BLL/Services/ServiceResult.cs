using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.BLL.Services
{
    /// <summary>
    /// Статус завершения операции бизнес-логики.
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
    /// Результат операции бизнес-логики без данных.
    /// </summary>
    public class ServiceResult
    {
        /// <summary>
        /// Статус операции.
        /// </summary>
        public ResultStatus Status { get; init; }

        /// <summary>
        /// Поясняющее сообщение (заполняется для ошибок).
        /// </summary>
        public string? Message { get; init; }

        /// <summary>
        /// Признак успешного завершения операции.
        /// </summary>
        public bool Succeeded => Status == ResultStatus.Success;

        public static ServiceResult Ok() =>
            new() { Status = ResultStatus.Success };

        public static ServiceResult NotFound(string message) =>
            new() { Status = ResultStatus.NotFound, Message = message };

        public static ServiceResult Forbidden(string message) =>
            new() { Status = ResultStatus.Forbidden, Message = message };

        public static ServiceResult Conflict(string message) =>
            new() { Status = ResultStatus.Conflict, Message = message };

        public static ServiceResult BadRequest(string message) =>
            new() { Status = ResultStatus.InvalidData, Message = message };
    }

    /// <summary>
    /// Результат операции бизнес-логики с полезной нагрузкой.
    /// </summary>
    /// <typeparam name="T">Тип возвращаемых данных.</typeparam>
    public class ServiceResult<T> : ServiceResult
    {
        /// <summary>
        /// Полезная нагрузка (заполняется при успехе).
        /// </summary>
        public T? Data { get; init; }

        public static ServiceResult<T> Ok(T data) =>
            new() { Status = ResultStatus.Success, Data = data };

        public new static ServiceResult<T> NotFound(string message) =>
            new() { Status = ResultStatus.NotFound, Message = message };

        public new static ServiceResult<T> Forbidden(string message) =>
            new() { Status = ResultStatus.Forbidden, Message = message };

        public new static ServiceResult<T> Conflict(string message) =>
            new() { Status = ResultStatus.Conflict, Message = message };

        public new static ServiceResult<T> BadRequest(string message) =>
            new() { Status = ResultStatus.InvalidData, Message = message };
    }
}
