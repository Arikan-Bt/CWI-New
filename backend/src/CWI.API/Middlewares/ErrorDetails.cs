using System.Text.Json;

namespace CWI.API.Middlewares;

/// <summary>
/// Tüm API hataları için standart yanıt modeli
/// </summary>
public class ErrorDetails
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? DetailedMessage { get; set; }
    
    public override string ToString() => JsonSerializer.Serialize(this);
}
