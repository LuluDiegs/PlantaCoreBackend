using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.API.Extensions;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.Services;
using PlantaCoreAPI.Infrastructure.Repositorios;
using PlantaCoreAPI.Infrastructure.Services;
using PlantaCoreAPI.API.Swagger;

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

var allowedOrigins = new List<string>
{
    builder.Configuration["Frontend:Url"]?.TrimEnd('/') ?? "http://localhost:5173",
    "http://localhost:5173",
    "http://localhost:5174",
    "http://localhost:3000",
    "https://localhost:7123"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins.Distinct().ToArray())
              .SetIsOriginAllowed(origin => allowedOrigins.Any(o => string.Equals(o, origin, StringComparison.OrdinalIgnoreCase)))
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddRepositorios();
builder.Services.AddServicosAplicacao(builder.Configuration);
builder.Services.AddServicosExternos(builder.Configuration);
builder.Services.RegistrarEventosHandlers();

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

    // Agrupamento por tags
    options.DocumentFilter<TagDescriptionsDocumentFilter>();

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

// CORS deve vir antes de qualquer autenticação/authorization
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Middleware global de tratamento de erros
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new {
                sucesso = false,
                mensagem = "Erro interno no servidor.",
                detalhes = ex.Message
            });
            await context.Response.WriteAsync(result);
        }
        // Se a resposta já começou, apenas não faz nada para evitar o erro de status code
    }
});

app.Run();