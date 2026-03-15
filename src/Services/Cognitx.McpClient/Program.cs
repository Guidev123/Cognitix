using Cognitx.McpClient.Chat;
using Cognitx.McpClient.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.AddSecurity();
builder.AddMcpClientInfrastructure();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration["WebAppEndpoints"]!)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();

app.MapOpenApi();

app.UseAuthentication();
app.UseAuthorization();

app.MapChatEndpoints();

app.Run();