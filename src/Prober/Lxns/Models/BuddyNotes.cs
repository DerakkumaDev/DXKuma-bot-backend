using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns.Models;

public record BuddyNotes
{
    [JsonPropertyName("left")]
    public required Notes Player1 { get; init; }

    [JsonPropertyName("right")]
    public required Notes Player2 { get; init; }
}