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
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==================================================");
            Console.WriteLine("  Инициализация запуска приложения Blog3Tier...");
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
                options.AccessDeniedPath = "/Error/403";
            });

            // Регистрация глобального фильтра аудита действий пользователя
            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add<AuditLogFilter>();
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Конвейер middleware
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

            // Middleware для добавления UserName, IP и URL в контекст логов
            app.UseMiddleware<UserContextLoggingMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            // Логирование всех входящих HTTP-запросов
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {FullUrl} responded {StatusCode} in {Elapsed:0.0000} ms";
            });

            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapControllers();

            // Инициализация БД (миграции, роли, пользователи)
            using (var scope = app.Services.CreateScope())
            {
                await DbInitializer.InitializeAsync(scope.ServiceProvider);
            }

            // 3. Запуск приложения с перехватом фатальных ошибок
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
                // Обеспечиваем сброс оставшихся логов при выключении
                Log.CloseAndFlush();
            }
        }
    }
}
