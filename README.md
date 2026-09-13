# TikTok Feed

Учебный бэкенд в духе TikTok на .NET 8: три независимых микросервиса с общением по gRPC, каждый — с классическим слоистым разделением `Api → Application → Domain → Infrastructure`.

## Сервисы

| Сервис | Назначение | REST | gRPC | Swagger |
|---|---|---|---|---|
| **Identity** | Пользователи, аутентификация (JWT), подписки (`Follow`) | `:7080` | `:7081` | http://localhost:7080/swagger |
| **Content** | Видео, звуки, избранное, модерация | `:9080` | `:9081` | http://localhost:9080/swagger |
| **Engagement** | Лента, поиск, лайки/комментарии/репосты/просмотры | `:6080` | `:6081` | http://localhost:6080/swagger |

Сервисы обращаются друг к другу по gRPC (например, Engagement дергает Identity и Content, чтобы собрать ленту), а внешние зависимости (детекция контента, пуш-уведомления) в докере подменены моками `vision-mock` и `push-mock`.

Каждый сервис — отдельная БД PostgreSQL (`db-identity`, `db-content`, `db-engagement`), без общих таблиц между сервисами.

## Стек

ASP.NET Core 8, EF Core + Npgsql, gRPC, JWT-аутентификация, Polly (ретраи/резилиентность к внешним HTTP-вызовам), xUnit для тестов, StyleCop + аналайзеры SourceKit для code style, Docker Compose для локального запуска.

## Запуск

```bash
docker compose up --build
```

Поднимутся все 3 сервиса, 3 базы и 2 мока. Swagger каждого сервиса доступен по адресам из таблицы выше.

## Тесты

```bash
dotnet test tests/Engagement.IntegrationTests/Engagement.IntegrationTests.csproj
```

Интеграционные тесты поднимают Engagement и Identity через `WebApplicationFactory` с фейками вместо внешних сервисов (см. `tests/Engagement.IntegrationTests/Fakes`).

> Требуется установленный .NET 8 runtime — если `dotnet --list-runtimes` его не показывает, довести до нужного набора можно через [`dotnet-install.sh`](https://dotnet.microsoft.com/download/dotnet/8.0) (канал `8.0`, рантаймы `dotnet` и `aspnetcore`).

## Структура решения

```
src/
  Shared/                    — общие контракты: ошибки, JWT, gRPC-протофайлы, Polly-политики
  {Identity,Content,Engagement}.Domain/          — сущности, доменные исключения, без внешних зависимостей
  {Identity,Content,Engagement}.Application/     — use case'ы, DTO, маппинги, абстракции репозиториев/сервисов
  {Identity,Content,Engagement}.Infrastructure/  — EF Core (конфигурации/репозитории), gRPC-клиенты, внешние сервисы
  {Identity,Content,Engagement}.Api/             — контроллеры, gRPC-эндпоинты, middleware, Program.cs
tests/
  Engagement.IntegrationTests/   — интеграционные тесты Engagement + Identity (фейки внешних зависимостей)
mocks/
  vision-mock/, push-mock/       — HTTP-заглушки внешних сервисов для докер-окружения
```
