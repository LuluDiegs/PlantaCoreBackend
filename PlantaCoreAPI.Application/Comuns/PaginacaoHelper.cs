namespace PlantaCoreAPI.Application.Comuns;

public static class PaginacaoHelper
{
    public const int TamanhoMaximo = 100;
    public const int TamanhoPadrao = 10;

    public static (int Pagina, int Tamanho) Sanitizar(int pagina, int tamanho)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanho = tamanho < 1 ? TamanhoPadrao : tamanho > TamanhoMaximo ? TamanhoMaximo : tamanho;
        return (pagina, tamanho);
    }
}
