# Avaliação Prática — Análise e Projeto Orientado a Objetos (3º período)

Solução em **C# / .NET 10** para a avaliação prática de APOO cujo material original é um ZIP com três imagens (`1questao.png`, `questão 2.png` e `questão 3.png`). As imagens foram extraídas e preservadas integralmente em [`docs/original/`](docs/original/); o ZIP original permanece na raiz do repositório.

O projeto implementa e **executa** os três enunciados com POO: casos de uso (questão 1), diagrama de classes (questão 2) e fluxo de aprovação de relatório (questão 3).

## Tecnologias

| Item | Uso |
| --- | --- |
| C# / .NET SDK | 10.0.402 — `net10.0` |
| xUnit | 33 testes de unidade |
| Microsoft.NET.Test.Sdk + xunit.runner.visualstudio | execução dos testes |
| coverlet.collector | coleta de cobertura |
| GitHub Actions | CI com `dotnet test` |
| OCR (Windows.Media.Ocr, pt-BR) | recuperação dos enunciados a partir das imagens |

## Leitura das imagens e limitações (importante)

O modelo de IA que preparou este repositório **não consegue receber imagens como entrada**, portanto a ferramenta de leitura de imagem nativa falhou. Para não perder informação, foi aplicado **OCR local do Windows (`Windows.Media.Ocr`, idioma pt-BR)** diretamente sobre os três PNG. O texto recuperado foi suficiente para identificar os rótulos, mas **posições, setas e multiplicidades exatas não são totalmente confiáveis**.

Por isso, algumas decisões abaixo são **suposições documentadas** e estão concentradas em pontos fáceis de alterar no código:

### Questão 1 — diagrama de casos de uso

Texto recuperado: `Sistema`, `Enviar Relatório`, `Consultar Status do Relatório`, `Estudante`, `Submeter Proposta de Projeto`, `Consultar Relatórios`, `Visualizar Estatísticas`, `Coordenação`, `Acompanhar Projetos`, `Avaliar Relatório`, `Professor Orientador`, `Aprovar Relatório`, `Solicitar Correções`, `Registrar Comentários`.

**Assunção:** `Sistema` é o limite do sistema (não um ator). Os 10 casos de uso foram distribuídos por afinidade semântica:

- **Estudante (3):** Submeter Proposta de Projeto, Enviar Relatório, Consultar Relatórios.
- **Professor Orientador (4):** Avaliar Relatório, Aprovar Relatório, Solicitar Correções, Registrar Comentários.
- **Coordenação (3):** Acompanhar Projetos, Consultar Status do Relatório, Visualizar Estatísticas.

Caso o desenho original associe `Avaliar Relatório` à Coordenação, basta mover o caso em `DiagramaCasosDeUso.CriarPadrao()`; os testes de distribuição devem ser ajustados junto.

### Questão 2 — diagrama de classes

Texto recuperado (classes, atributos e operações):

- `Professor`: `id: int`, `nome: String`; `analisarRelatorio()`, `comentarRelatorio()`, `aprovarRelatorio()`.
- `Estudante`: `id: int`, `nome: String`, `matricula: String`; `submeterProposta()`, `enviarRelatorio()`.
- `Projeto`: `id: int`, `titulo: String`, `descricao: String`.
- `Relatorio`: `id: int`, `descricao: String`, `dataEnvio: Date`, `status: String`; `enviar()`, `atualizarStatus()`.
- `Coordenacao`: `acompanharProjetos()`, `visualizarEstatisticas()`.

**Assunção:** a multiplicidade ilegível `0..` foi interpretada como **`0..*`**, ou seja, um projeto agrega zero ou mais relatórios. `String` → `string`, `Date` → `DateTimeOffset`. `Coordenacao` não possui atributos no desenho. Foram acrescentados `tentativas`, `Comentario`, `EventoRelatorio` e `orientador` como aprimoramentos para dar suporte à questão 3.

### Questão 3 — fluxo de envio e aprovação

Texto recuperado, na ordem: *Estudante elabora relatório → Submete relatório no sistema → Sistema registra e envia ao professor → Professor analisa o relatório → Relatório está adequado? → Professor aprova o relatório → Sistema atualiza status para "Aprovado" → Sistema atualiza histórico acadêmico → Professor solicita correções → Sistema devolve relatório ao estudante → Estudante realiza correções → Reenvia relatório → Professor analisa o relatório*.

**Assunção:** o enunciado não explicita o critério da decisão “Relatório está adequado?”. Adotou-se: descrição com **pelo menos 20 caracteres** e **sem a palavra “rascunho”** (`CriteriosAvaliacao`). Também foi adicionado um **limite de 3 tentativas** (`FluxoAprovacaoRelatorio.MaximoTentativas`), com estado terminal `Reprovado`, para evitar o loop infinito mostrado no diagrama. Ambos são aprimoramentos, não requisitos lidos da imagem.

## Modelo de domínio

```text
src/AvaliacaoApoo/
├── DomainException.cs
├── Validacao.cs
├── Enumeracoes/
│   ├── TipoAtor.cs            # Estudante | ProfessorOrientador | Coordenacao
│   └── StatusRelatorio.cs     # EmElaboracao → Enviado → EmAnalise → Aprovado
│                              #                         ↘ AguardandoCorrecao → Enviado
│                              #                         ↘ Reprovado
├── CasosDeUso/
│   ├── CasoDeUso.cs
│   └── DiagramaCasosDeUso.cs  # agregado com os 10 casos (questão 1)
├── Entidades/
│   ├── Pessoa.cs              # classe abstrata (herança)
│   ├── Estudante.cs           # herança: Pessoa
│   ├── Professor.cs           # herança: Pessoa
│   ├── Projeto.cs             # agregação de Estudante/Professor e Relatorios
│   ├── Relatorio.cs           # composição de histórico e comentários + máquina de estados
│   ├── EventoRelatorio.cs
│   ├── Comentario.cs
│   └── Coordenacao.cs         # implementa IMonitoramentoProjetos (interface)
└── Servicos/
    ├── CriteriosAvaliacao.cs
    ├── ResultadoAvaliacao.cs (no mesmo arquivo de CriteriosAvaliacao)
    ├── HistoricoAcademicoEmMemoria.cs  # IHistoricoAcademico
    └── FluxoAprovacaoRelatorio.cs      # orquestração da questão 3
```

### Padrões e conceitos de POO aplicados

| Conceito | Onde aparece |
| --- | --- |
| Classes e encapsulamento | `Usuario`→ `Relatorio`, `Projeto`, `Pessoa` com propriedades `private set` e operações explícitas |
| Herança | `Estudante : Pessoa`, `Professor : Pessoa` |
| Interfaces | `IMonitoramentoProjetos`, `IHistoricoAcademico` |
| Agregação | `Projeto` conhece `Responsavel` e `Orientador` sem possuí-los |
| Composição | `Projeto` possui `Relatorios`; `Relatorio` possui `Historico` e `Comentarios` |
| Polimorfismo | `Pessoa.ToString()` e uso das interfaces por `Coordenacao`/histórico |
| Value objects / records | `CasoDeUso`, `EventoRelatorio`, `Comentario`, `EstatisticasProjetos`, `ResultadoProcessamento` |
| Regras de negócio | validação de textos/ids, transições de status, critérios de avaliação, limite de tentativas |

### Máquina de estados do relatório

| De | Para |
| --- | --- |
| `EmElaboracao` | `Enviado` |
| `Enviado` | `EmAnalise` |
| `EmAnalise` | `Aprovado`, `AguardandoCorrecao`, `Reprovado` |
| `AguardandoCorrecao` | `Enviado` (reenvio), `Reprovado` |
| `Aprovado` / `Reprovado` | estado terminal |

Qualquer outra transição lança `DomainException`. Cada mudança gera um `EventoRelatorio` no histórico.

## Como executar

Pré-requisito: **.NET SDK 10.0.402** (ou qualquer 10.0.x).

```powershell
dotnet run --project src/AvaliacaoApoo.Cli --configuration Release
```

O console apresenta, em sequência:

1. os casos de uso agrupados por ator;
2. o resumo do diagrama de classes e instâncias criadas (agregação/composição);
3. três cenários do fluxo da questão 3 (aprovação direta, correção e reenvio, reprovação por limite de tentativas) e o histórico acadêmico.

## Testes

```powershell
dotnet test AvaliacaoApoo.sln --configuration Release
```

Cobertura dos testes (33 no total):

- **Questão 1:** os 10 casos, distribuição por ator, busca insensível a caixa, duplicidade e validação de nome.
- **Questão 2:** validação de `Estudante`/`Professor`/`Projeto`, agregação e composição, rejeição de relatório de outro projeto e de id repetido, transições de status, comentários, estatísticas da coordenação e ordem do histórico.
- **Questão 3:** critérios de adequação, aprovação na primeira tentativa com atualização do histórico, ciclo de correção/reenvio, reprovação após 3 tentativas, estratégia de correção injetada, idempotência de fluxo já aprovado e duplicidade no histórico.

Resultado esperado: **33 testes aprovados**.

## Integração contínua

[`.github/workflows/ci.yml`](.github/workflows/ci.yml) instala o .NET 10, restaura, compila em `Release` e executa `dotnet test` em `push`/`pull request` nas branches `main` e `master`.

## Estrutura geral

```text
.
├── Avaliação APOO JOAO MARCELO.zip   # material original
├── docs/original/                    # as 3 imagens extraídas
├── src/AvaliacaoApoo/                # biblioteca de domínio
├── src/AvaliacaoApoo.Cli/            # saída formatada no console
├── tests/AvaliacaoApoo.Tests/        # xUnit
├── .github/workflows/ci.yml
├── AvaliacaoApoo.sln
└── README.md
```
