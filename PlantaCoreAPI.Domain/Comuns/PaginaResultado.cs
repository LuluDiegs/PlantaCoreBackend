namespace PlantaCoreAPI.Domain.Comuns;

public class PaginaResultado<T>
{
    public IEnumerable<T> Itens { get; set; } = Enumerable.Empty<T>();
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int Total { get; set; }
    public int TotalPaginas => TamanhoPagina > 0 ? (int)Math.Ceiling((double)Total / TamanhoPagina) : 0;
    public bool TemProxima => Pagina < TotalPaginas;
    public bool TemAnterior => Pagina > 1;
}
