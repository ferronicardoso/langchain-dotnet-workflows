using Anthropic;
using Microsoft.Extensions.AI;
using OpenAI;
using RoutingApp;

var builder = WebApplication.CreateBuilder(args);

string Config(string chave) => builder.Configuration[chave]
    ?? throw new InvalidOperationException($"{chave} is not set");

var openAi = new OpenAIClient(Config("OPENAI_API_KEY"));
var anthropic = new AnthropicClient { ApiKey = Config("ANTHROPIC_API_KEY") };

ModeloConfig OpenAi(string nome) => new("openai", nome, openAi.GetChatClient(nome).AsIChatClient());
ModeloConfig Anthropic(string nome) => new("anthropic", nome, anthropic.AsIChatClient(nome));

// Um provider e modelo por rota (os mesmos da aula). Cada nome pode ser trocado por configuração.
var router = OpenAi(builder.Configuration["MODELO_ROUTER"] ?? "gpt-5.6-luna");
var pipeline = Anthropic(builder.Configuration["MODELO_PIPELINE"] ?? "claude-sonnet-5");
var infraestrutura = OpenAi(builder.Configuration["MODELO_INFRAESTRUTURA"] ?? "gpt-5.6-sol");
var release = Anthropic(builder.Configuration["MODELO_RELEASE"] ?? "claude-haiku-4-5-20251001");

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonOptions.Padrao.PropertyNamingPolicy;
    foreach (var c in JsonOptions.Padrao.Converters) o.SerializerOptions.Converters.Add(c);
});
builder.Services.AddValidation(); // valida o contrato de entrada na fronteira, antes de chamar o modelo
builder.Services.AddSingleton(new RoutingWorkflow(router, pipeline, infraestrutura, release));

var app = builder.Build();

app.MapPost("/triagens", (Solicitacao solicitacao, RoutingWorkflow w, CancellationToken ct) =>
    w.ExecutarAsync(solicitacao, ct));

app.Run();
