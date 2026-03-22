using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.API.Extensions;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.Services;
using PlantaCoreAPI.Infrastructure.Repositorios;
using PlantaCoreAPI.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddLoggingConfigurado();
builder.Services.AddBancoDeDados(builder.Configuration);
builder.Services.AddAutenticacaoJwt(builder.Configuration);

var frontendUrl = builder.Configuration["Frontend:Url"]?.TrimEnd('/') ?? "http://localhost:5173";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:5174",
                "https://localhost:7123",
                frontendUrl
            );
        }
        else
        {
            policy.WithOrigins(frontendUrl);
        }

        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddRepositorios();
builder.Services.AddServicosAplicacao(builder.Configuration);
builder.Services.AddServicosExternos(builder.Configuration);

builder.Services.AddHostedService<PlantCareReminderBackgroundService>();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            return new BadRequestObjectResult(new
            {
                sucesso = false,
                mensagem = "Dados de entrada inválidos",
                erros = errors
            });
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PlantaCoreAPI",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScoped<EventoService>();
builder.Services.AddScoped<IRepositorioEvento, RepositorioEvento>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PlantaCoreAPI v1");
    c.RoutePrefix = string.Empty;
});


app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();