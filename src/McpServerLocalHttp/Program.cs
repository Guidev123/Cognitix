using McpServerLocalHttp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<StorageClient>();
builder.Services.AddSingleton<TodoListTools>();
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<TodoListTools>();

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://+:{port}");

app.MapMcp("/mcp");

app.Run();