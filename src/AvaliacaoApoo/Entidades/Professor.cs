namespace AvaliacaoApoo.Domain.Entidades;

/// <summary>
/// Professor orientador responsável por avaliar os relatórios (questão 2).
/// </summary>
public sealed class Professor : Pessoa
{
    public Professor(int id, string nome) : base(id, nome)
    {
    }

    public void AnalisarRelatorio(Relatorio relatorio)
    {
        ArgumentNullException.ThrowIfNull(relatorio);
        relatorio.MarcarEmAnalise();
    }

    public void ComentarRelatorio(Relatorio relatorio, string comentario)
    {
        ArgumentNullException.ThrowIfNull(relatorio);
        relatorio.AdicionarComentario(Comentario.Criar(Nome, comentario));
    }

    public void AprovarRelatorio(Relatorio relatorio)
    {
        ArgumentNullException.ThrowIfNull(relatorio);
        relatorio.AtualizarStatus(Enumeracoes.StatusRelatorio.Aprovado);
    }

    public void SolicitarCorrecoes(Relatorio relatorio, string motivo)
    {
        ArgumentNullException.ThrowIfNull(relatorio);
        relatorio.SolicitarCorrecoes(motivo);
    }

    public void ReprovarRelatorio(Relatorio relatorio)
    {
        ArgumentNullException.ThrowIfNull(relatorio);
        relatorio.AtualizarStatus(Enumeracoes.StatusRelatorio.Reprovado);
    }
}
