using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Blog.BLL.DTOs
{
    // DTO создания новой метки
    public class CreateTagDto
    {
        [Required(ErrorMessage = "Имя тега обязательно")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Длина тега должна быть от 2 до 40 символов")]
        [RegularExpression(@"^[a-zA-Z0-9а-яА-Я_-]+$", ErrorMessage = "Тег не должен содержать пробелов и спецсимволов")]
        public string Name { get; set; } = string.Empty;
    }

    // DTO редактирования тега
    public class UpdateTagDto
    {
        [Required(ErrorMessage = "Имя тега обязательно")]
        [StringLength(40, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z0-9а-яА-Я_-]+$", ErrorMessage = "Тег не должен содержать пробелов и спецсимволов")]
        public string Name { get; set; } = string.Empty;
    }

    // DTO информации о теге
    public record TagDto(
        int Id,
        string Name,
        string? CreatorId,
        int ArticlesCount
    );


}
