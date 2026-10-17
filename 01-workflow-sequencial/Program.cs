using Google.GenAI;
using LangChain.Chains.HelperChains;
using Microsoft.Extensions.AI;
using static LangChain.Chains.Chain;

var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
if (string.IsNullOrEmpty(apiKey))
{
    Console.Error.WriteLine("Configure GOOGLE_API_KEY no ambiente antes de executar.");
    return 1;
}

IChatClient model = new Client(apiKey: apiKey).AsIChatClient("gemini-3.8-flash");

// Cada etapa e uma chain: Set (valor) | Template (mensagem humana) | LLM (modelo).
// O system prompt vai nas opcoes do modelo; o retorno e uma string para a proxima etapa.

async Task<string> Etapa(string system, string template, string variavel, string valor)
{
    BaseStackableChain chain =
        Set(valor, variavel)
        | Template(template)
        | LLM(model, "text", "text", new ChatOptions { Instructions = system });

    return await chain.RunAsync("text") ?? string.Empty;
}

const string systemEvidencias =
    "Voce analisa alertas operacionais. Extraia somente evidencias presentes " +
    "na entrada, sem formular causa ou acao.";

const string systemHipotese =
    "Voce formula uma unica hipotese operacional a partir das evidencias " +
    "recebidas. Diferencie fato de inferencia.";

const string systemPlano =
    "Voce cria um plano inicial e curto para verificar uma hipotese " +
    "operacional. Nao proponha correcao antes da verificacao.";

var alerta = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "alerta.txt"));

var evidencias = await Etapa(systemEvidencias, "Alerta:\n\n{alerta}", "alerta", alerta);
Console.WriteLine($"EVIDENCIAS: \n{evidencias}");
Console.WriteLine("\n\n------------------------------------------------------------\n\n");

var hipotese = await Etapa(systemHipotese, "Evidencias:\n\n{evidencias}", "evidencias", evidencias);
Console.WriteLine($"HIPOTESE: \n{hipotese}");
Console.WriteLine("\n\n------------------------------------------------------------\n\n");

var plano = await Etapa(systemPlano, "Hipotese:\n\n{hipotese}", "hipotese", hipotese);
Console.WriteLine($"PLANO: \n{plano}");

return 0;
