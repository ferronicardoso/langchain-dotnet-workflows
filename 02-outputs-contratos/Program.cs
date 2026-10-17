using Google.GenAI;
using Microsoft.Extensions.AI;
using OutputsContratos;

var builder = WebApplication.CreateBuilder(args);

var apiKey = builder.Configuration["GOOGLE_API_KEY"]
    ?? throw new InvalidOperationException("Configure GOOGLE_API_KEY no ambiente antes de executar.");

IChatClient modelo = new Client(apiKey: apiKey)
    .AsIChatClient(builder.Configuration["MODELO"] ?? "gemini-3.8-flash");

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonOptions.Padrao.PropertyNamingPolicy;
    foreach (var c in JsonOptions.Padrao.Converters) o.SerializerOptions.Converters.Add(c);
});
builder.Services.AddValidation(); // valida o contrato de entrada na fronteira, antes de chamar o modelo
builder.Services.AddSingleton(new AnaliseWorkflow(modelo));

var app = builder.Build();

app.MapPost("/analises", (AlertaEntrada entrada, AnaliseWorkflow w, CancellationToken ct) =>
    w.ExecutarAsync(entrada, ct));

app.Run();
