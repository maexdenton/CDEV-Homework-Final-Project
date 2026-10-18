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

            // Строка подключения читается из конфигурации (appsettings.json / User Secrets / переменные окружения).
            // Если ключ отсутствует, приложение падает сразу с понятным сообщением, а не с неинформативной ошибкой при первом обращении к БД.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Строка 'DefaultConnection' не найдена.");

            // Data Access Layer
            // 
            // DbContext регистрируется как Scoped: один экземпляр на HTTP-запрос.
            // Это стандартное время жизни для EF Core, т.к. DbContext не потокобезопасен.
            builder.Services.AddDbContext<BlogDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Репозитории также Scoped: они используют DbContext, который живет в рамках запроса.
            // Регистрация через интерфейсы позволяет подменять реализацию в тестах.
            builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddScoped<ITagRepository, TagRepository>();

            // Business Logic Layer
            // 
            // Сервисы Scoped, т.к. они зависят от репозиториев (а те от DbContext)
            builder.Services.AddScoped<IArticleService, ArticleService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<ITagService, TagService>();
            builder.Services.AddScoped<IUserService, UserService>();

            // ASP.NET Core Identity
            // 
            // Политика паролей намеренно ослаблена для учебного проекта:
            // минимум 6 символов, без требований к цифрам, регистру и спецсимволам.
            // В production эти настройки следует ужесточить.
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

            // Регистрация политик авторизации на основе claim'ов.
            // Для каждого разрешения из AppPermissions.All создается политика с тем же именем,
            // которая требует наличия claim'а "permission" с соответствующим значением.
            // Это позволяет использовать [Authorize(Policy = AppPermissions.UsersView)] и т.д.
            builder.Services.AddAuthorization(options =>
            {
                foreach (var permission in AppPermissions.All)
                {
                    options.AddPolicy(permission, policy =>
                        policy.RequireClaim("permission", permission));
                }
            });

            // Настройка Cookie-аутентификации:
            // - LoginPath: куда перенаправлять неавторизованных пользователей;
            // - AccessDeniedPath: куда перенаправлять авторизованных, но без нужных прав (403).
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Error/403"; // страница "Доступ запрещен"
            });

            builder.Services.AddControllersWithViews();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Конвейер middleware
            // 
            // Порядок middleware критичен:
            // 1. GlobalExceptionMiddleware стоит раньше всех, чтобы перехватить исключения из API-запросов и вернуть JSON,
            //  а для MVC — пробросить исключение дальше.
            // 2. UseExceptionHandler ловит проброшенное исключение и показывает страницу /Error (500).
            app.UseMiddleware<GlobalExceptionMiddleware>();

            // Глобальный обработчик исключений для MVC (500)
            app.UseExceptionHandler("/Error");

            // Swagger включается только в режиме разработки, чтобы не раскрывать API-документацию в production
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Middleware аутентификации и авторизации должны идти строго в этом порядке: 
            // сначала Authentication (кто пользователь), затем Authorization (что ему можно).
            app.UseAuthentication();
            app.UseAuthorization();

            // Обработка статус-кодов 403 и 404: перенаправляет на /Error/{code}, где ErrorController выбирает соответствующее представление.
            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            // Стандартный маршрут MVC: /{controller}/{action}/{id?}
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Маршрутизация для API-контроллеров (атрибутная маршрутизация)
            app.MapControllers();

            // Инициализация БД при старте: миграции, роли, Сlaims и тестовые пользователи. 
            // Создаётся отдельный scope, т.к. все используемые сервисы (DbContext, RoleManager, UserManager) — Scoped.
            using (var scope = app.Services.CreateScope())
            {
                await DbInitializer.InitializeAsync(scope.ServiceProvider);
            }

            await app.RunAsync();
        }
    }
}