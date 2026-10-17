using Microsoft.Extensions.AI;
using OpenAI;
using ParallelApp;

var builder = WebApplication.CreateBuilder(args);

var apiKey = builder.Configuration["OPENAI_API_KEY"]
    ?? throw new InvalidOperationException("Configure OPENAI_API_KEY no ambiente antes de executar.");

IChatClient modelo = new OpenAIClient(apiKey)
    .GetChatClient(builder.Configuration["MODELO"] ?? "gpt-5.6-luna")
    .AsIChatClient();

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonOptions.Padrao.PropertyNamingPolicy;
    foreach (var c in JsonOptions.Padrao.Converters) o.SerializerOptions.Converters.Add(c);
});
builder.Services.AddValidation(); // valida o contrato de entrada na fronteira, antes de chamar o modelo
builder.Services.AddSingleton(new ParaleloWorkflow(modelo));

var app = builder.Build();

app.MapPost("/analises", (Incidente incidente, ParaleloWorkflow w, CancellationToken ct) =>
    w.ExecutarAsync(incidente, ct));

app.Run();
