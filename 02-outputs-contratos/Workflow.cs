using System.Text.Json;
using Microsoft.Extensions.AI;

namespace OutputsContratos;

// Aulas 1.3 + 1.4: workflow sequencial (evidências -> hipótese -> plano) com saída estruturada.
// O fluxo é decidido pelo código, não pelo modelo: por isso é automação e não agente.
public class AnaliseWorkflow(IChatClient modelo)
{
    private const string SystemEvidencias =
        "Voce analisa alertas operacionais. Extraia somente fatos presentes " +
        "na entrada. Nao formule causa, hipotese ou acao.";

    private const string SystemHipotese =
        "Voce formula uma unica hipotese operacional verificavel. Use somente " +
        "as evidencias recebidas, diferencie fato de inferencia e registre " +
        "o que ainda precisa ser confirmado.";

    private const string SystemPlano =
        "Voce cria um plano inicial e curto para verificar uma hipotese " +
        "operacional. Inclua apenas acoes de leitura ou comparacao. " +
        "Nao proponha correcao antes da verificacao.";

    public async Task<AnaliseIncidente> ExecutarAsync(AlertaEntrada entrada, CancellationToken ct = default)
    {
        var evidencias = await Cadeia.ExecutarEstruturadoAsync<Evidencias>(
            modelo, SystemEvidencias,
            "Origem: {origem}\n\nAlerta:\n\n{alerta}",
            new Dictionary<string, string> { ["origem"] = entrada.Origem, ["alerta"] = entrada.Alerta }, ct);
        Console.WriteLine($"Evidencias: {nameof(Evidencias)}");

        // Cada objeto vira JSON antes de ser a entrada da etapa seguinte (model_dump_json no Python).
        var evidenciasJson = JsonSerializer.Serialize(evidencias, JsonOptions.Padrao);

        var hipotese = await Cadeia.ExecutarEstruturadoAsync<Hipotese>(
            modelo, SystemHipotese,
            "Evidencias validadas:\n\n{evidencias}",
            new Dictionary<string, string> { ["evidencias"] = evidenciasJson }, ct);
        Console.WriteLine($"Hipotese: {nameof(Hipotese)}");

        var hipoteseJson = JsonSerializer.Serialize(hipotese, JsonOptions.Padrao);

        var plano = await Cadeia.ExecutarEstruturadoAsync<PlanoVerificacao>(
            modelo, SystemPlano,
            "Evidencias validadas:\n\n{evidencias}\n\nHipotese validada:\n\n{hipotese}",
            new Dictionary<string, string> { ["evidencias"] = evidenciasJson, ["hipotese"] = hipoteseJson }, ct);
        Console.WriteLine($"Plano: {nameof(PlanoVerificacao)}");

        return new AnaliseIncidente(evidencias, hipotese, plano);
    }
}
