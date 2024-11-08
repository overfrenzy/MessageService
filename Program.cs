using MessageService.DataAccess;
using MessageService.GraphQL;
using MessageService.Hubs;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Очистить встроенные логгеры
builder.Logging.ClearProviders();

// Настройка Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();

// Репозиторий сообщений
builder.Services.AddSingleton<MessageRepository>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не найдена.");
    var logger = provider.GetRequiredService<ILogger<MessageRepository>>();
    return new MessageRepository(connectionString, logger);
});

// Настройка GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .ModifyRequestOptions(options => options.IncludeExceptionDetails = true);

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

app.UseStaticFiles();

// Логирование запуска приложения
app.Lifetime.ApplicationStarted.Register(() => Log.Information("Приложение успешно запущено."));
app.Lifetime.ApplicationStopped.Register(() => Log.Information("Приложение остановлено."));

// Настройка конвейера HTTP-запросов
app.MapHub<MessageHub>("/messageHub");

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapGraphQL("/graphql");

app.MapGet("/client1", async context =>
{
    await context.Response.SendFileAsync("wwwroot/client1.html");
});

app.MapGet("/client2", async context =>
{
    await context.Response.SendFileAsync("wwwroot/client2.html");
});

app.MapGet("/client3", async context =>
{
    await context.Response.SendFileAsync("wwwroot/client3.html");
});

app.Run("http://0.0.0.0:80");

Log.CloseAndFlush();
