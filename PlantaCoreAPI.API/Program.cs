using PlantaCoreAPI.API.Extensions;
using PlantaCoreAPI.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddLoggingConfigurado();
builder.Services.AddBancoDeDados(builder.Configuration);
builder.Services.AddAutenticacaoJwt(builder.Configuration);

var frontendUrl = builder.Configuration["Frontend:Url"] ?? "http://localhost:5173";

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
