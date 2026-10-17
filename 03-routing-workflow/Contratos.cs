using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoutingApp;

// Contratos do roteamento (equivalem a contratos.py da aula 1.5)

public record Solicitacao(
    [property: Required, MinLength(1)] string Titulo,
    [property: Required, MinLength(1)] string Descricao);

public enum Destino { Pipeline, Infraestrutura, Release, Fallback }

public record Roteamento(Destino Destino);

public record Resultado(string Rota, string Provider, string Modelo, string Resposta);

// Entrada do branch: a solicitação já com a rota decidida pelo roteador.
public record EntradaRoteada(string Titulo, string Descricao, Destino Rota);

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
