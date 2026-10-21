using Blog.BLL.Security;
using Blog.BLL.Services;
using Blog.DAL;
using Blog.DAL.Entities;
using Blog.DAL.Repositories;
using Blog.Web.Data;
using Blog.Web.Filters;
using Blog.Web.Middlewares;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Blog.Web
{
    /// <summary>
    /// Точка входа приложения Blog3Tier.
    /// Настраивает Serilog, DI-контейнер (DAL, BLL, Identity, авторизация),
    /// конвейер middleware и инициализацию базы данных при старте.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Запускает веб-приложение: конфигурирует сервисы, middleware и выполняет
        /// инициализацию БД (миграции, роли, тестовые пользователи) перед стартом сервера.
        /// </summary>
        /// <param name="args">Аргументы командной строки.</param>
        public static async Task Main(string[] args)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==================================================");
            Console.WriteLine(" Инициализация запуска приложения Blog3Tier...");
            Console.WriteLine("==================================================");
            Console.WriteLine();
            Console.ResetColor();

            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Строка 'DefaultConnection' не найдена.");

            // Data Access Layer
            builder.Services.AddDbContext<BlogDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
            builder.Services.AddScoped<ITagRepository, TagRepository>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();

            // Business Logic Layer
            builder.Services.AddScoped<IArticleService, ArticleService>();
            builder.Services.AddScoped<ITagService, TagService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
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

            builder.Services.AddAuthorization(options =>
            {
                // Каждое разрешение из AppPermissions.All превращается в политику,
                // проверяющую claim "permission" — это позволяет гибко управлять доступом без изменения кода.
                foreach (var permission in AppPermissions.All)
                {
                    options.AddPolicy(permission, policy =>
                        policy.RequireClaim("permission", permission));
                }
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Error/403";
            });

            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add<AuditLogFilter>();
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseExceptionHandler("/Error");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Обогащает логи контекстом пользователя (UserName, IP, URL), чтобы записи
            // можно было фильтровать по конкретному пользователю или запросу.
            app.UseMiddleware<UserContextLoggingMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {FullUrl} responded {StatusCode} in {Elapsed:0.0000} ms";
            });

            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllers();

            // Инициализация идемпотентна: повторный запуск не создает дубликаты ролей и пользователей.
            using (var scope = app.Services.CreateScope())
            {
                await DbInitializer.InitializeAsync(scope.ServiceProvider);
            }

            try
            {
                Log.Information("Запуск веб-сервера...");
                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Приложение неожиданно завершилось.");
            }
            finally
            {
                // Сбрасывает буферизованные логи при выключении, чтобы последние записи не потерялись.
                Log.CloseAndFlush();
            }
        }
    }
}
