using AvaliacaoApoo.Domain.Enumeracoes;

namespace AvaliacaoApoo.Domain.Entidades;

/// <summary>
/// Relatório submetido pelo estudante e avaliado pelo professor orientador.
/// Controla o fluxo descrito na questão 3 por meio de transições de estado.
/// </summary>
public sealed class Relatorio
{
    private static readonly IReadOnlyDictionary<StatusRelatorio, IReadOnlySet<StatusRelatorio>> Transicoes =
        new Dictionary<StatusRelatorio, IReadOnlySet<StatusRelatorio>>
        {
            [StatusRelatorio.EmElaboracao] = new HashSet<StatusRelatorio>
            {
                StatusRelatorio.Enviado
            },
            [StatusRelatorio.Enviado] = new HashSet<StatusRelatorio>
            {
                StatusRelatorio.EmAnalise
            },
            [StatusRelatorio.EmAnalise] = new HashSet<StatusRelatorio>
            {
                StatusRelatorio.Aprovado,
                StatusRelatorio.AguardandoCorrecao,
                StatusRelatorio.Reprovado
            },
            [StatusRelatorio.AguardandoCorrecao] = new HashSet<StatusRelatorio>
            {
                StatusRelatorio.Enviado,
                StatusRelatorio.Reprovado
            },
            [StatusRelatorio.Aprovado] = new HashSet<StatusRelatorio>(),
            [StatusRelatorio.Reprovado] = new HashSet<StatusRelatorio>()
        };

    private readonly List<EventoRelatorio> _historico = new();
    private readonly List<Comentario> _comentarios = new();

    public Relatorio(int id, string descricao, Projeto projeto, DateTimeOffset? criadoEm = null)
    {
        Validacao.Identificador(id, "Id");
        Id = id;
        Descricao = Validacao.TextoObrigatorio(descricao, "Descrição", 10, 2000);
        Projeto = projeto ?? throw new ArgumentNullException(nameof(projeto));
        CriadoEm = criadoEm ?? DateTimeOffset.UtcNow;

        RegistrarEvento("Relatório elaborado pelo estudante.");
    }

    public int Id { get; }

    public string Descricao { get; private set; }

    public DateTimeOffset CriadoEm { get; }

    public DateTimeOffset? EnviadoEm { get; private set; }

    public StatusRelatorio Status { get; private set; } = StatusRelatorio.EmElaboracao;

    public int Tentativas { get; private set; }

    /// <summary>Composição: o relatório pertence a um projeto.</summary>
    public Projeto Projeto { get; }

    public IReadOnlyList<EventoRelatorio> Historico => _historico.AsReadOnly();

    public IReadOnlyList<Comentario> Comentarios => _comentarios.AsReadOnly();

    /// <summary>
    /// "Sistema registra e envia ao professor" / "Reenvia relatório".
    /// </summary>
    public void Enviar()
    {
        AtualizarStatus(StatusRelatorio.Enviado);
        EnviadoEm = DateTimeOffset.UtcNow;
        Tentativas++;
        RegistrarEvento($"Relatório enviado ao professor orientador (tentativa {Tentativas}).");
    }

    public void MarcarEmAnalise()
    {
        AtualizarStatus(StatusRelatorio.EmAnalise);
        RegistrarEvento("Professor orientador iniciou a análise do relatório.");
    }

    public void Aprovar()
        => AtualizarStatus(StatusRelatorio.Aprovado);

    public void SolicitarCorrecoes(string motivo)
    {
        var texto = Validacao.TextoObrigatorio(motivo, "Motivo", 3, 500);
        AtualizarStatus(StatusRelatorio.AguardandoCorrecao);
        RegistrarEvento($"Correções solicitadas: {texto}");
    }

    public void DevolverAoEstudante()
    {
        if (Status != StatusRelatorio.AguardandoCorrecao)
        {
            throw new DomainException(
                "Só é possível devolver um relatório que aguarda correções.");
        }

        RegistrarEvento("Sistema devolveu o relatório ao estudante.");
    }

    public void AplicarCorrecao(string novaDescricao)
    {
        if (Status != StatusRelatorio.AguardandoCorrecao)
        {
            throw new DomainException(
                "Só é possível aplicar correções em um relatório que as aguarda.");
        }

        Descricao = Validacao.TextoObrigatorio(novaDescricao, "Descrição", 10, 2000);
        RegistrarEvento("Estudante aplicou as correções solicitadas.");
    }

    public void Reprovar()
        => AtualizarStatus(StatusRelatorio.Reprovado);

    /// <summary>
    /// Aplica uma transição válida de status (método "atualizarStatus" do diagrama).
    /// </summary>
    public void AtualizarStatus(StatusRelatorio novoStatus)
    {
        if (!Transicoes[Status].Contains(novoStatus))
        {
            throw new DomainException(
                $"Transição de {Status} para {novoStatus} não é permitida.");
        }

        var anterior = Status;
        Status = novoStatus;
        RegistrarEvento($"Status alterado de {anterior} para {novoStatus}.");
    }

    public void AdicionarComentario(Comentario comentario)
    {
        ArgumentNullException.ThrowIfNull(comentario);
        _comentarios.Add(comentario);
        RegistrarEvento("Comentário registrado pelo professor orientador.");
    }

    private void RegistrarEvento(string descricao)
        => _historico.Add(new EventoRelatorio(DateTimeOffset.UtcNow, Status, descricao));

    public override string ToString()
        => $"Relatório #{Id} — {Status} — projeto: {Projeto.Titulo}";
}
