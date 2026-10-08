using System.Security.Claims;
using Blog.BLL.Security;
using Blog.DAL;
using Blog.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog.Web.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<BlogDbContext>();
            await context.Database.MigrateAsync();

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            // Создание ролей
            var roles = new[] { AppRoles.Administrator, AppRoles.Moderator, AppRoles.User };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Назначение Claims (Permissions) для роли "Администратор" (Все права)
            var adminRole = await roleManager.FindByNameAsync(AppRoles.Administrator);
            var existingAdminClaims = await roleManager.GetClaimsAsync(adminRole!);
            foreach (var permission in AppPermissions.All)
            {
                if (!existingAdminClaims.Any(c => c.Type == "permission" && c.Value == permission))
                {
                    await roleManager.AddClaimAsync(adminRole!, new Claim("permission", permission));
                }
            }

            // Назначение Claims для роли "Модератор"
            var moderatorRole = await roleManager.FindByNameAsync(AppRoles.Moderator);
            var existingModClaims = await roleManager.GetClaimsAsync(moderatorRole!);
            var moderatorPermissions = new[]
            {
                AppPermissions.ArticlesView, AppPermissions.ArticlesCreate, AppPermissions.ArticlesUpdate, AppPermissions.ArticlesDelete,
                AppPermissions.CommentsView, AppPermissions.CommentsCreate, AppPermissions.CommentsUpdate, AppPermissions.CommentsDelete,
                AppPermissions.TagsView, AppPermissions.TagsCreate
            };
            foreach (var permission in moderatorPermissions)
            {
                if (!existingModClaims.Any(c => c.Type == "permission" && c.Value == permission))
                {
                    await roleManager.AddClaimAsync(moderatorRole!, new Claim("permission", permission));
                }
            }

            // Назначение Claims для роли "Пользователь"
            var userRole = await roleManager.FindByNameAsync(AppRoles.User);
            var existingUserClaims = await roleManager.GetClaimsAsync(userRole!);
            var userPermissions = new[]
            {
                AppPermissions.ArticlesView, AppPermissions.ArticlesCreate, AppPermissions.ArticlesUpdate, AppPermissions.ArticlesDelete,
                AppPermissions.CommentsView, AppPermissions.CommentsCreate, AppPermissions.CommentsUpdate, AppPermissions.CommentsDelete,
                AppPermissions.TagsView, AppPermissions.TagsCreate, AppPermissions.TagsUpdate, AppPermissions.TagsDelete
            };
            foreach (var permission in userPermissions)
            {
                if (!existingUserClaims.Any(c => c.Type == "permission" && c.Value == permission))
                {
                    await roleManager.AddClaimAsync(userRole!, new Claim("permission", permission));
                }
            }

            // Создание тестового Администратора
            const string adminEmail = "admin@blog.local";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Главный",
                    LastName = "Администратор",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, AppRoles.Administrator);
            }

            // Создание тестового Модератора
            const string modEmail = "moderator@blog.local";
            if (await userManager.FindByEmailAsync(modEmail) == null)
            {
                var mod = new User
                {
                    UserName = modEmail,
                    Email = modEmail,
                    FirstName = "Дежурный",
                    LastName = "Модератор",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(mod, "Moderator123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(mod, AppRoles.Moderator);
            }
        }
    }
}
