using System.ComponentModel.DataAnnotations;

namespace Blog.Web.ViewModels
{
    public class RoleListViewModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    public class RoleCreateViewModel
    {
        [Required(ErrorMessage = "Название роли обязательно")]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = null!;
    }

    public class RoleEditViewModel
    {
        public string Id { get; set; } = null!;

        [Required(ErrorMessage = "Название роли обязательно")]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = null!;
    }

    public class RoleDeleteViewModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
