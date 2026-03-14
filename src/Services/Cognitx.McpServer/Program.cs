using Cognitx.McpServer.Storage;
using Cognitx.McpServer.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<StorageOptions>(
    builder.Configuration.GetSection(StorageOptions.SectionName));

builder.Services.AddSingleton<StorageClient>();
builder.Services.AddSingleton<TodoListTools>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<TodoListTools>();

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://+:{port}");

app.MapMcp("/mcp");

app.Run();