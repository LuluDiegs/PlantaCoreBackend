namespace PlantaCoreAPI.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string mensagem) : base(mensagem) { }
    public DomainException(string mensagem, Exception excecaoInterna)
        : base(mensagem, excecaoInterna) { }
}
