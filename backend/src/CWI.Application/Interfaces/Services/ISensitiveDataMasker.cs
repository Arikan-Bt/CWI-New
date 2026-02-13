namespace CWI.Application.Interfaces.Services;

public interface ISensitiveDataMasker
{
    string? MaskJson(string? rawJson);
    string? MaskText(string? key, string? value);
    IReadOnlyDictionary<string, string?> MaskKeyValuePairs(IReadOnlyDictionary<string, string?> values);
}
