using PlantaCoreAPI.API.Extensions;
using PlantaCoreAPI.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddLoggingConfigurado();
builder.Services.AddBancoDeDados(builder.Configuration);
builder.Services.AddAutenticacaoJwt(builder.Configuration);
builder.Services.AddCors(opcoes =>
    opcoes.AddPolicy("TodosOrigenes", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddRepositorios();
builder.Services.AddServicosAplicacao();
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
app.UseCors("TodosOrigenes");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
