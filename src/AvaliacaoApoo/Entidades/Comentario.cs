namespace AvaliacaoApoo.Domain.Entidades;

/// <summary>
/// Comentário registrado pelo professor orientador (composição).
/// </summary>
public sealed record Comentario(DateTimeOffset CriadoEm, string Autor, string Texto)
{
    public static Comentario Criar(string autor, string texto)
        => new(
            DateTimeOffset.UtcNow,
            Validacao.TextoObrigatorio(autor, "Autor", 3, 120),
            Validacao.TextoObrigatorio(texto, "Comentário", 1, 1000));
}
