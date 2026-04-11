using Microsoft.AspNetCore.Mvc.Filters;
using PlantaCoreAPI.Application.Comuns;

namespace PlantaCoreAPI.API.Filters;

public sealed class PaginacaoSanitizerFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionArguments.TryGetValue("pagina", out var paginaObj) && paginaObj is int pagina)
        {
            if (pagina < 1)
                context.ActionArguments["pagina"] = 1;
        }

        if (context.ActionArguments.TryGetValue("tamanho", out var tamanhoObj) && tamanhoObj is int tamanho)
        {
            if (tamanho < 1)
                context.ActionArguments["tamanho"] = PaginacaoHelper.TamanhoPadrao;
            else if (tamanho > PaginacaoHelper.TamanhoMaximo)
                context.ActionArguments["tamanho"] = PaginacaoHelper.TamanhoMaximo;
        }
    }
    public void OnActionExecuted(ActionExecutedContext context) { }
}
