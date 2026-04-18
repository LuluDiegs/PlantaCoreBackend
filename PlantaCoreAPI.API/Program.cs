using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.API.Extensions;
using PlantaCoreAPI.API.Filters;
using PlantaCoreAPI.API.Options;
using PlantaCoreAPI.Infrastructure.Services;
using PlantaCoreAPI.API.Swagger;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load(".env");

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

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

builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection("Admin"));

builder.Services.AddRepositorios();
builder.Services.AddServicosAplicacao(builder.Configuration);
builder.Services.AddServicosExternos(builder.Configuration);
builder.Services.RegistrarEventosHandlers();

builder.Services.AddHostedService<PlantCareReminderBackgroundService>();

builder.Services.AddControllers(options =>
    {
        options.Filters.Add<PaginacaoSanitizerFilter>();
    })
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

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Exceção não tratada na requisição {Method} {Path}", context.Request.Method, context.Request.Path);

        if (!context.Response.HasStarted)
        {
            var isTransient = PlantaCoreAPI.Application.Utils.ExcecaoTransienteHelper.EhTransiente(ex);
            context.Response.StatusCode = isTransient ? 503 : 500;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                sucesso = false,
                mensagem = isTransient
                    ? "Serviço temporariamente indisponível. Tente novamente em instantes."
                    : "Erro interno no servidor. Tente novamente mais tarde."
            });
            await context.Response.WriteAsync(result);
        }
    }
});

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
