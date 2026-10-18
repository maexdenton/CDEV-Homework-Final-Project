using Blog.BLL.Security;
using Blog.BLL.Services;
using Blog.DAL;
using Blog.DAL.Entities;
using Blog.DAL.Repositories;
using Blog.Web.Data;
using Blog.Web.Middlewares;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Строка 'DefaultConnection' не найдена.");

            // Data Access Layer
            builder.Services.AddDbContext<BlogDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddScoped<ITagRepository, TagRepository>();

            // Business Logic Layer
            builder.Services.AddScoped<IArticleService, ArticleService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<ITagService, TagService>();
            builder.Services.AddScoped<IUserService, UserService>();

            // ASP.NET Core Identity
            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<BlogDbContext>()
            .AddDefaultTokenProviders();

            // Регистрация динамических политик авторизации на базе Claims
            builder.Services.AddAuthorization(options =>
            {
                foreach (var permission in AppPermissions.All)
                {
                    options.AddPolicy(permission, policy =>
                        policy.RequireClaim("permission", permission));
                }
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Error/403"; // страница «Доступ запрещен»
            });

            builder.Services.AddControllersWithViews();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Для API: middleware обрабатывает исключения и возвращает JSON.
            // Для MVC: middleware пробрасывает исключение дальше,
            // и его перехватывает UseExceptionHandler.
            app.UseMiddleware<GlobalExceptionMiddleware>();

            // Глобальный обработчик исключений для MVC (500)
            app.UseExceptionHandler("/Error");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Обязательный порядок Middlewares безопасности
            app.UseAuthentication();
            app.UseAuthorization();

            // Перенаправление на пользовательские страницы для 403 и 404
            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapControllers();

            // Инициализация схемы данных, ролей, claims и пользователей
            using (var scope = app.Services.CreateScope())
            {
                await DbInitializer.InitializeAsync(scope.ServiceProvider);
            }

            await app.RunAsync();
        }
    }
}