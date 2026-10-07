using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Blog.BLL.DTOs
{
    /// <summary>
    /// DTO создания новой метки
    /// </summary>
    public class CreateTagDto
    {
        [Required(ErrorMessage = "Имя тега обязательно")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Длина тега должна быть от 2 до 40 символов")]
        [RegularExpression(@"^[a-zA-Z0-9а-яА-Я_-]+$", ErrorMessage = "Тег не должен содержать пробелов и спецсимволов")]
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO редактирования тега
    /// </summary>
    public class UpdateTagDto
    {
        [Required(ErrorMessage = "Имя тега обязательно")]
        [StringLength(40, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z0-9а-яА-Я_-]+$", ErrorMessage = "Тег не должен содержать пробелов и спецсимволов")]
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO информации о теге
    /// </summary>
    public record TagDto(
        int Id,
        string Name,
        int ArticlesCount
    );
}
