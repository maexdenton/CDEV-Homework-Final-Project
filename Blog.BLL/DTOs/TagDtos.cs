using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Blog.BLL.DTOs
{
    /// <summary>
    /// DTO для создания тега.
    /// </summary>
    public class CreateTagDto
    {
        [Required(ErrorMessage = "Название тега обязательно")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Название тега должно быть от 2 до 50 символов")]
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO для редактирования тега.
    /// </summary>
    public class UpdateTagDto
    {
        [Required(ErrorMessage = "Название тега обязательно")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Название тега должно быть от 2 до 50 символов")]
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO для отображения тега.
    /// </summary>
    public record TagDto(
        int Id,
        string Name,
        int ArticlesCount
    );
}
