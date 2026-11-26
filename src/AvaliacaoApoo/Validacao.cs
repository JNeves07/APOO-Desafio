namespace AvaliacaoApoo.Domain;

/// <summary>
/// Validações comuns às entidades do modelo de domínio.
/// </summary>
public static class Validacao
{
    public static string TextoObrigatorio(string? valor, string campo, int minimo, int maximo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new DomainException($"{campo} é obrigatório.");
        }

        var texto = valor.Trim();

        if (texto.Length < minimo || texto.Length > maximo)
        {
            throw new DomainException($"{campo} deve ter entre {minimo} e {maximo} caracteres.");
        }

        return texto;
    }

    public static void Identificador(int id, string campo)
    {
        if (id <= 0)
        {
            throw new DomainException($"{campo} deve ser maior que zero.");
        }
    }
}
