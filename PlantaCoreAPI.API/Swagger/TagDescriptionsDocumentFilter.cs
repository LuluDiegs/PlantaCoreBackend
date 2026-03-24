using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace PlantaCoreAPI.API.Swagger;

public class TagDescriptionsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags = new List<OpenApiTag>
        {
            new OpenApiTag { Name = "Autenticacao", Description = "Autenticação e registro de usuários" },
            new OpenApiTag { Name = "Comunidade", Description = "Comunidades e membros" },
            new OpenApiTag { Name = "Evento", Description = "Eventos e participação" },
            new OpenApiTag { Name = "LembreteCuidado", Description = "Lembretes de cuidado de plantas" },
            new OpenApiTag { Name = "Notificacao", Description = "Notificações e configurações" },
            new OpenApiTag { Name = "Planta", Description = "Plantas, identificação e social" },
            new OpenApiTag { Name = "Post", Description = "Feed, posts, engajamento" },
            new OpenApiTag { Name = "Usuario", Description = "Operações de usuário e social" },

        };
    }
}
