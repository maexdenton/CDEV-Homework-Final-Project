using Blog.BLL.Services;
using Blog.DAL;
using Blog.DAL.Entities;
using Blog.DAL.Repositories;
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
                ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не найдена.");

            // Регистрация DAL: Контекст БД и Репозитории
            builder.Services.AddDbContext<BlogDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
            builder.Services.AddScoped<ITagRepository, TagRepository>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();

            // Регистрация BLL: Бизнес-сервисы
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IArticleService, ArticleService>();
            builder.Services.AddScoped<ITagService, TagService>();
            builder.Services.AddScoped<ICommentService, CommentService>();

            // Настройка Identity
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

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // Поддержка MVC-представлений (Razor) и API-контроллеров
            builder.Services.AddControllersWithViews();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Глобальная обработка ошибок
            app.UseMiddleware<GlobalExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(); // Разрешаем раздачу css/js/стилей

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Маршрут по умолчанию - открывает главную страницу блога при старте
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Подключение маршрутов REST API ([Route("api/[controller]")])
            app.MapControllers();

            // Автоматическое применение миграций и создание роли "Пользователь"
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
                await context.Database.MigrateAsync();

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                if (!await roleManager.RoleExistsAsync("Пользователь"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Пользователь"));
                }
            }

            await app.RunAsync();
        }
    }
}