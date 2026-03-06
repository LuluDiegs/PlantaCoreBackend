using Microsoft.OpenApi.Models;

namespace PlantaCoreAPI.API.Extensions;

internal static class SwaggerExtensions
{
    internal static IServiceCollection AddSwaggerConfigurado(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(opcoes =>
        {
            opcoes.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PlantaCoreAPI",
                Version = "v1",
                Description = "API de identificação e gerenciamento de plantas com IA"
            });

            opcoes.TagActionsBy(api =>
            {
                var tag = api.GroupName ?? api.ActionDescriptor.RouteValues["controller"];
                return new[] { tag! };
            });

            opcoes.OrderActionsBy(api =>
            {
                var controller = api.ActionDescriptor.RouteValues["controller"] ?? "";
                var rota = api.RelativePath ?? "";
                var metodo = api.HttpMethod ?? "";

                var ordemController = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Autenticacao", 1 }, { "Usuario", 2 }, { "Planta", 3 },
                    { "Post", 4 }, { "Notificacao", 5 }, { "Armazenamento", 6 }
                };

                var ordemAutenticacao = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    { "api/v1/Autenticacao/registrar", 1 },
                    { "api/v1/Autenticacao/confirmar-email", 2 },
                    { "api/v1/Autenticacao/reenviar-confirmacao", 3 },
                    { "api/v1/Autenticacao/login", 4 },
                    { "api/v1/Autenticacao/refresh-token", 5 },
                    { "api/v1/Autenticacao/logout", 6 },
                    { "api/v1/Autenticacao/resetar-senha", 7 },
                    { "api/v1/Autenticacao/nova-senha", 8 },
                    { "api/v1/Autenticacao/trocar-senha", 9 },
                };

                var ordemUsuario = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    { "api/v1/Usuario/perfil", 1 },
                    { "api/v1/Usuario/perfil-publico/{usuarioId}", 2 },
                    { "api/v1/Usuario/nome", 3 },
                    { "api/v1/Usuario/biografia", 4 },
                    { "api/v1/Usuario/foto-perfil", 5 },
                    { "api/v1/Usuario/seguir/{usuarioIdParaSeguir}", 6 },
                    { "api/v1/Usuario/seguir/{usuarioIdParaDeseguir}", 7 },
                    { "api/v1/Usuario/{usuarioId}/seguidores", 8 },
                    { "api/v1/Usuario/{usuarioId}/seguindo", 9 },
                    { "api/v1/Usuario/conta", 10 },
                };

                var ordemPost = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    { "api/v1/Post", 1 }, { "api/v1/Post/feed", 2 }, { "api/v1/Post/explorar", 3 },
                    { "api/v1/Post/{postId}", 4 }, { "api/v1/Post/usuario/{usuarioId}", 5 },
                    { "api/v1/Post/usuario/{usuarioId}/curtidos", 6 },
                    { "api/v1/Post/{postId}/curtir", 7 }, { "api/v1/Post/{postId}/curtida", 8 },
                    { "api/v1/Post/{postId}/comentarios", 9 }, { "api/v1/Post/comentario", 10 },
                    { "api/v1/Post/comentario/{comentarioId}", 11 },
                    { "api/v1/Post/comentario/{comentarioId}/curtir", 12 },
                    { "api/v1/Post/comentario/{comentarioId}/curtida", 13 },
                    { "api/v1/Post/{postId}/comentario/{comentarioId}", 14 },
                };

                var posicaoController = ordemController.TryGetValue(controller, out var ci) ? ci : 99;
                int posicaoRota = 99;

                if (controller.Equals("Autenticacao", StringComparison.OrdinalIgnoreCase))
                    ordemAutenticacao.TryGetValue(rota, out posicaoRota);
                else if (controller.Equals("Usuario", StringComparison.OrdinalIgnoreCase))
                    ordemUsuario.TryGetValue(rota, out posicaoRota);
                else if (controller.Equals("Post", StringComparison.OrdinalIgnoreCase))
                    ordemPost.TryGetValue(rota, out posicaoRota);

                var ordemMetodo = metodo switch
                {
                    "GET" => 1, "POST" => 2, "PUT" => 3, "DELETE" => 4, _ => 5
                };

                return $"{posicaoController:D2}_{posicaoRota:D2}_{rota}_{ordemMetodo}";
            });

            opcoes.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
            });

            opcoes.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
