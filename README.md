# 🌐 Blog3Tier

Веб-приложение блога с трехуровневой архитектурой на платформе ASP.NET Core (.NET 8) с поддержкой веб-интерфейса (MVC) и RESTful API.

> [!NOTE]
> Проект является дипломной работой, завершающей обучение на курсе SkillFactory

---

## 📌 Возможности

* **Статьи**: полный CRUD, поиск по тексту, фильтрация по тегам, выборка статей по автору.
* **Теги**: создание с фиксацией автора, привязка к статьям (Many-to-Many), контроль уникальности.
* **Комментарии**: добавление, инлайн-редактирование и удаление прямо на странице статьи.
* **Безопасность**:
  * Аутентификация через ASP.NET Core Identity (хэширование паролей).
  * Автоматическое назначение роли `Пользователь` при регистрации.
  * Разграничение доступа по ролям (`Администратор`, `Модератор`, `Пользователь`) и правам (Claims).
  * Серверная проверка владения объектом при редактировании и удалении (защита от подмены `AuthorId`).
* **Интерфейс**: Razor Views (Bootstrap 5.3) + Swagger UI для REST API.

---

## 🛠 Стек технологий

* **Платформа**: .NET 8 / C#
* **Фреймворк**: ASP.NET Core MVC / Web API
* **ORM**: Entity Framework Core 8 (SQL Server)
* **Аутентификация**: ASP.NET Core Identity
* **Документация API**: Swagger / OpenAPI (Swashbuckle)

---

## 📋 Архитектура решения

Проект разделен на три независимых слоя:

```text
Blog.Web (Presentation Layer)   --> Контроллеры MVC и API, Razor Views, Middlewares
   │
Blog.BLL (Business Logic Layer) --> Сервисы (Article, Comment, Tag, User), DTO, права доступа
   │
Blog.DAL (Data Access Layer)    --> Сущности, BlogDbContext, Fluent API конфигурации, Репозитории
```

---

## 📂 Структура проекта

```text
Blog3Tier/
├── Blog.sln
├── Blog.DAL/
│   ├── Configurations/
│   │   ├── ArticleConfiguration.cs
│   │   ├── CommentConfiguration.cs
│   │   └── TagConfiguration.cs
│   ├── Entities/
│   │   ├── Article.cs
│   │   ├── Comment.cs
│   │   ├── Tag.cs
│   │   └── User.cs
│   ├── Migrations/
│   ├── Repositories/
│   │   ├── ArticleRepository.cs
│   │   ├── CommentRepository.cs
│   │   ├── IArticleRepository.cs
│   │   ├── ICommentRepository.cs
│   │   ├── IRepository.cs
│   │   ├── ITagRepository.cs
│   │   └── TagRepository.cs
│   ├── BlogDbContext.cs
│   └── Blog.DAL.csproj
├── Blog.BLL/
│   ├── DTOs/
│   │   ├── ArticleDtos.cs
│   │   ├── CommentDtos.cs
│   │   ├── TagDtos.cs
│   │   └── UserDtos.cs
│   ├── Security/
│   │   └── SecurityConstants.cs
│   ├── Services/
│   │   ├── ArticleService.cs
│   │   ├── CommentService.cs
│   │   ├── IArticleService.cs
│   │   ├── ICommentService.cs
│   │   ├── ITagService.cs
│   │   ├── IUserService.cs
│   │   ├── TagService.cs
│   │   └── UserService.cs
│   └── Blog.BLL.csproj
└── Blog.Web/
    ├── Controllers/
    │   ├── AccountController.cs
    │   ├── ArticleController.cs
    │   ├── BaseApiController.cs
    │   ├── CommentController.cs
    │   ├── HomeController.cs
    │   ├── TagController.cs
    │   └── UserController.cs
    ├── Data/
    │   └── DbInitializer.cs
    ├── Middlewares/
    │   └── GlobalExceptionMiddleware.cs
    ├── Properties/
    │   └── launchSettings.json
    ├── ViewModels/
    │   ├── AccountViewModels.cs
    │   ├── ArticleViewModels.cs
    │   └── ErrorViewModel.cs
    ├── Views/
    │   ├── Account/
    │   │   ├── Login.cshtml
    │   │   └── Register.cshtml
    │   ├── Home/
    │   │   ├── Create.cshtml
    │   │   ├── Details.cshtml
    │   │   ├── Edit.cshtml
    │   │   └── Index.cshtml
    │   ├── Shared/
    │   │   └── _Layout.cshtml
    │   ├── _ViewImports.cshtml
    │   └── _ViewStart.cshtml
    ├── appsettings.json
    ├── Program.cs
    └── Blog.Web.csproj
```

---

## 🚀 Запуск проекта

### 1. Клонировать репозиторий:
```bash
git clone https://github.com/maexdenton/CDEV-Homework-Final-Project.git
cd CDEV-Homework-Final-Project/Blog3Tier
```

### 2. Проверить строку подключения в `Blog.Web/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=Blog3TierDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### 3. Собрать и запустить проект:
```bash
dotnet build
dotnet run --project Blog.Web
```
При первом запуске база данных `Blog3TierDb` создается и мигрирует автоматически.

---

## Предустановленные учетные записи

- Администратор: admin@blog.local / Admin123!
- Модератор: moderator@blog.local / Moderator123!

## Адреса в браузере

- Веб-интерфейс: https://localhost:7133/
- Swagger UI: https://localhost:7133/swagger

---

## Матрица доступа

| Роль              | Статьи и комментарии                               | Теги                                     | Пользователи и роли                        |
| ----------------- | -------------------------------------------------- | ---------------------------------------- | ------------------------------------------ |
| **Администратор** | Полный доступ ко всем                              | Полный доступ ко всем                    | Полный доступ (`Permissions.Roles.Manage`) |
| **Модератор**     | Редактирование и удаление любых                    | Только просмотр и создание               | Нет доступа                                |
| **Пользователь**  | Создание, редактирование/удаление **только своих** | Редактирование/удаление **только своих** | Только свой профиль                        |
| **Гость**         | Только просмотр                                    | Только просмотр                          | Регистрация и вход                         |

## Основные API-маршруты

| Метод                                   | Описание                                       |
| --------------------------------------- | ---------------------------------------------- |
| **POST /api/user/register**             | Регистрация пользователя                       |
| **GET, PUT, DELETE /api/user/{id}**     | Управление пользователями (Админ)              |
| **GET, POST, PUT, DELETE /api/article** | CRUD статей (`/api/article/author/{authorId}`) |
| **GET, POST, PUT, DELETE /api/comment** | CRUD комментариев                              |
| **GET, POST, PUT, DELETE /api/tag**     | CRUD тегов                                     |

---

## Миграции EF Core
```bash
# Добавление миграции
dotnet ef migrations add <Name> --project Blog.DAL --startup-project Blog.Web

# Применение миграции вручную
dotnet ef database update --project Blog.DAL --startup-project Blog.Web
```
