using System.Text.Json;
using LangChain.Chains.HelperChains;
using Microsoft.Extensions.AI;
using static LangChain.Chains.Chain;

namespace OutputsContratos;

// Monta e executa `ChatPromptTemplate | model` com o LangChain .NET:
// Set(valores) | Template(mensagem humana) | LLM(modelo, system prompt nas opções).
public static class Cadeia
{
    public static async Task<string> ExecutarAsync(
        IChatClient modelo, string system, string templateHumano,
        IReadOnlyDictionary<string, string> valores, ChatResponseFormat? formato, CancellationToken ct)
    {
        var opcoes = new ChatOptions { Instructions = system, ResponseFormat = formato };

        BaseStackableChain? chain = null;
        foreach (var (chave, valor) in valores)
            chain = chain is null ? Set(valor, chave) : chain | Set(valor, chave);

        chain = chain! | Template(templateHumano) | LLM(modelo, "text", "text", opcoes);
        return await chain.RunAsync("text", null, ct) ?? string.Empty;
    }

    // Equivalente a `prompt | model.with_structured_output(Contrato, method="json_schema")`.
    public static async Task<T> ExecutarEstruturadoAsync<T>(
        IChatClient modelo, string system, string templateHumano,
        IReadOnlyDictionary<string, string> valores, CancellationToken ct)
    {
        var json = await ExecutarAsync(modelo, system, templateHumano, valores,
            ChatResponseFormat.ForJsonSchema<T>(JsonOptions.Padrao), ct);

        return JsonSerializer.Deserialize<T>(json, JsonOptions.Padrao)
            ?? throw new InvalidOperationException($"O modelo não retornou um {typeof(T).Name}.");
    }
}
