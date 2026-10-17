using System.Text.Json;
using Microsoft.Extensions.AI;

namespace ParallelApp;

// Aula 1.6: execução paralela. Três análises independentes sobre o mesmo incidente (fan-out) e uma
// consolidação (fan-in) que só roda depois que as três terminaram.
public class ParaleloWorkflow(IChatClient modelo)
{
    private const string TemplateHumano = "Titulo: {titulo}\n\nDescricao:\n{descricao}";

    private const string SystemPipeline =
        "Voce analisa incidentes pela perspectiva do pipeline de entrega. " +
        "Use somente os dados recebidos, separe evidencias de hipotese e " +
        "devolva perspectiva='pipeline'.";

    private const string SystemInfraestrutura =
        "Voce analisa incidentes pela perspectiva de infraestrutura. " +
        "Considere capacidade, rede, computacao e dependencias gerenciadas. " +
        "Use somente os dados recebidos, separe evidencias de hipotese e " +
        "devolva perspectiva='infraestrutura'.";

    private const string SystemAplicacao =
        "Voce analisa incidentes pela perspectiva da aplicacao. " +
        "Considere comportamento em runtime, dependencias, latencia e erros. " +
        "Use somente os dados recebidos, separe evidencias de hipotese e " +
        "devolva perspectiva='aplicacao'.";

    private const string SystemConsolidacao =
        "Voce consolida analises independentes de um incidente. Preserve " +
        "divergencias entre os especialistas, nao transforme hipoteses em " +
        "fatos e proponha apenas proximos passos de verificacao.";

    public async Task<Resultado> ExecutarAsync(Incidente incidente, CancellationToken ct = default)
    {
        var entrada = new Dictionary<string, string>
        {
            ["titulo"] = incidente.Titulo,
            ["descricao"] = incidente.Descricao,
        };

        // 4. Fan-out: as três chains rodam ao mesmo tempo sobre a mesma entrada
        //    (RunnableParallel(pipeline=..., infraestrutura=..., aplicacao=...) no LangChain Python).
        var pipeline = Analisar(SystemPipeline, entrada, ct);
        var infraestrutura = Analisar(SystemInfraestrutura, entrada, ct);
        var aplicacao = Analisar(SystemAplicacao, entrada, ct);
        await Task.WhenAll(pipeline, infraestrutura, aplicacao);

        var analises = new AnalisesParalelas(pipeline.Result, infraestrutura.Result, aplicacao.Result);

        // 5. Fan-in: a consolidação recebe o conjunto completo, serializado como JSON.
        var consolidacao = await Cadeia.ExecutarEstruturadoAsync<Consolidacao>(
            modelo, SystemConsolidacao, "Analises independentes:\n{analises}",
            new Dictionary<string, string> { ["analises"] = JsonSerializer.Serialize(analises, JsonOptions.Indentado) }, ct);

        return new Resultado(analises, consolidacao);
    }

    // chain = prompt | model.with_structured_output(Analise)
    private Task<Analise> Analisar(string system, Dictionary<string, string> entrada, CancellationToken ct) =>
        Cadeia.ExecutarEstruturadoAsync<Analise>(modelo, system, TemplateHumano, entrada, ct);
}
