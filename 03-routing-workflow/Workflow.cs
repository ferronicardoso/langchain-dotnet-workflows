using Microsoft.Extensions.AI;

namespace RoutingApp;

// Aula 1.5: routing workflow. Um roteador classifica a solicitação (saída estruturada)
// e um Branch despacha para a tratativa de cada rota, cada uma com seu provider e modelo.

public record ModeloConfig(string Provider, string Nome, IChatClient Client);

// O LangChain .NET não tem RunnableBranch; este é o equivalente: avalia as condições em
// ordem, executa só a primeira verdadeira e cai no padrão quando nenhuma é satisfeita.
public class Branch<TIn, TOut>(
    IReadOnlyList<(Func<TIn, bool> Condicao, Func<TIn, CancellationToken, Task<TOut>> Ramo)> ramos,
    Func<TIn, CancellationToken, Task<TOut>> padrao)
{
    public Task<TOut> InvokeAsync(TIn entrada, CancellationToken ct = default)
    {
        foreach (var (condicao, ramo) in ramos)
            if (condicao(entrada)) return ramo(entrada, ct);
        return padrao(entrada, ct);
    }
}

public class RoutingWorkflow
{
    private const string TemplateHumano = "Titulo: {titulo}\n\nDescricao:\n{descricao}";

    private readonly ModeloConfig _router, _pipeline, _infraestrutura, _release;
    private readonly Branch<EntradaRoteada, Resultado> _branches;

    public RoutingWorkflow(ModeloConfig router, ModeloConfig pipeline, ModeloConfig infraestrutura, ModeloConfig release)
    {
        (_router, _pipeline, _infraestrutura, _release) = (router, pipeline, infraestrutura, release);

        // 6. Branch que escolhe somente uma tratativa
        _branches = new Branch<EntradaRoteada, Resultado>(
            [
                (e => e.Rota == Destino.Pipeline, TratarPipeline),
                (e => e.Rota == Destino.Infraestrutura, TratarInfraestrutura),
                (e => e.Rota == Destino.Release, TratarRelease),
            ],
            TratarFallback);
    }

    // 7. Workflow executado passo a passo: classifica, acrescenta a rota à entrada, invoca o branch.
    public async Task<Resultado> ExecutarAsync(Solicitacao solicitacao, CancellationToken ct = default)
    {
        var decisao = await ClassificarAsync(solicitacao, ct);
        var entradaComRota = new EntradaRoteada(solicitacao.Titulo, solicitacao.Descricao, decisao.Destino);
        return await _branches.InvokeAsync(entradaComRota, ct);
    }

    // 1. router_chain = router_prompt | router_model.with_structured_output(Roteamento)
    private Task<Roteamento> ClassificarAsync(Solicitacao s, CancellationToken ct) =>
        Cadeia.ExecutarEstruturadoAsync<Roteamento>(
            _router.Client,
            "Classifique a solicitacao em exatamente uma categoria: " +
            "pipeline para falha em pipeline ou deploy; infraestrutura para revisao " +
            "de Terraform, Kubernetes ou outra infraestrutura como codigo; release " +
            "para comunicado ou release notes; fallback quando houver mais de uma " +
            "intencao principal ou quando nenhuma categoria servir.",
            TemplateHumano, Valores(s.Titulo, s.Descricao), ct);

    // 2. Chain para falhas de pipeline
    private async Task<Resultado> TratarPipeline(EntradaRoteada e, CancellationToken ct) =>
        await Tratar("pipeline", _pipeline,
            "Voce analisa falhas em pipelines de entrega. Resuma o estagio que " +
            "falhou, separe evidencias de hipoteses e proponha verificacoes de " +
            "leitura antes de qualquer correcao.", e, ct);

    // 3. Chain para revisão de infraestrutura
    private async Task<Resultado> TratarInfraestrutura(EntradaRoteada e, CancellationToken ct) =>
        await Tratar("infraestrutura", _infraestrutura,
            "Voce revisa infraestrutura como codigo. Identifique o recurso " +
            "alterado, aponte riscos verificaveis e produza observacoes objetivas " +
            "para a revisao do pull request. Nao aplique a mudanca.", e, ct);

    // 4. Chain para comunicação de release
    private async Task<Resultado> TratarRelease(EntradaRoteada e, CancellationToken ct) =>
        await Tratar("release", _release,
            "Voce escreve comunicacoes curtas de release. Separe mudancas e " +
            "impacto operacional sem inventar funcionalidades que nao aparecem " +
            "na solicitacao.", e, ct);

    // Fallback não chama nenhum provider.
    private static Task<Resultado> TratarFallback(EntradaRoteada e, CancellationToken ct) =>
        Task.FromResult(new Resultado(
            Rota: "fallback",
            Provider: "aplicacao",
            Modelo: "nenhum",
            Resposta: "A solicitacao precisa ser esclarecida ou dividida antes de seguir " +
                      "para uma tratativa especializada."));

    private async Task<Resultado> Tratar(string rota, ModeloConfig modelo, string system, EntradaRoteada e, CancellationToken ct)
    {
        // chain = prompt | model | StrOutputParser()
        var resposta = await Cadeia.ExecutarAsync(
            modelo.Client, system, TemplateHumano, Valores(e.Titulo, e.Descricao), formato: null, ct);
        return new Resultado(rota, modelo.Provider, modelo.Nome, resposta);
    }

    private static Dictionary<string, string> Valores(string titulo, string descricao) =>
        new() { ["titulo"] = titulo, ["descricao"] = descricao };
}
