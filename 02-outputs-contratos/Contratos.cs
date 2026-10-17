using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OutputsContratos;

// Contratos de entrada e saída (equivalente aos modelos Pydantic de contratos.py)

public record AlertaEntrada(
    [property: Required, MinLength(1), Description("Conteudo bruto do alerta operacional")]
    string Alerta,
    [property: Description("Sistema que enviou o alerta")]
    string Origem = "monitoramento");

public enum CategoriaEvidencia { Sintoma, Mudanca, Contexto }

public record Evidencia(
    [property: Description("Papel da evidencia na analise do incidente")]
    CategoriaEvidencia Categoria,
    [property: Description("Fato observado diretamente no alerta")]
    string Descricao);

public record Evidencias(
    [property: Description("Servico afetado pelo alerta")]
    string Servico,
    [property: MinLength(1), Description("Evidencias extraidas sem formular causa ou acao")]
    List<Evidencia> Itens,
    [property: Description("Mudanca recente relacionada no tempo, quando informada")]
    string? MudancaRecente = null);

public record Hipotese(
    [property: Description("Uma unica hipotese operacional a ser verificada")]
    string Descricao,
    [property: MinLength(1), Description("Evidencias que sustentam a hipotese")]
    List<string> EvidenciasRelacionadas,
    [property: Description("Informacoes que ainda faltam para confirmar a hipotese")]
    List<string>? Incertezas = null);

public record PassoVerificacao(
    [property: Range(1, int.MaxValue), Description("Posicao do passo no plano de verificacao")]
    int Ordem,
    [property: Description("Acao de leitura ou comparacao a executar")]
    string Acao,
    [property: Description("Resultado observavel usado para avaliar a hipotese")]
    string EvidenciaEsperada);

public record PlanoVerificacao(
    [property: Description("O que o plano pretende confirmar ou descartar")]
    string Objetivo,
    [property: MinLength(1), Description("Sequencia curta de verificacoes sem acao corretiva")]
    List<PassoVerificacao> Passos,
    [property: Description("Condicao que encerra a verificacao da hipotese")]
    string CriterioEncerramento);

public record AnaliseIncidente(Evidencias Evidencias, Hipotese Hipotese, PlanoVerificacao Plano);

// JSON em snake_case e enums em minúsculas, como nos contratos Pydantic do curso
// (mudanca_recente, evidencias_relacionadas, "sintoma", "pipeline", ...).
public static class JsonOptions
{
    public static readonly JsonSerializerOptions Padrao = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };
}
