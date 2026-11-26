namespace AvaliacaoApoo.Domain;

/// <summary>
/// Exceção base para violações das regras de negócio da avaliação.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
