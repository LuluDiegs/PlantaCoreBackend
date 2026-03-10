using PlantaCoreAPI.API.Extensions;
using PlantaCoreAPI.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddLoggingConfigurado();
builder.Services.AddBancoDeDados(builder.Configuration);
builder.Services.AddAutenticacaoJwt(builder.Configuration);

// Obter URL do frontend da configuração
// Em Azure: ler de Application Settings
// Localmente: ler de user-secrets
var frontendUrl = builder.Configuration["Frontend:Url"] ?? "http://localhost:5173";

// Log para debug
Console.WriteLine($"[CORS] Frontend URL: {frontendUrl}");

builder.Services.AddCors(opcoes =>
    opcoes.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(frontendUrl)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromHours(1))));

builder.Services.AddRepositorios();
builder.Services.AddServicosAplicacao(builder.Configuration);
builder.Services.AddServicosExternos(builder.Configuration);

builder.Services.AddHostedService<PlantCareReminderBackgroundService>();

builder.Services.AddControllers();
builder.Services.AddSwaggerConfigurado();

var app = builder.Build();

// Ativar Swagger em Development E Production
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ?? CORS DEVE VIR ANTES DE Authentication
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
