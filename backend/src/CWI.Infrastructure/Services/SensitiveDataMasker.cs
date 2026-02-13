using System.Text.Json;
using System.Text.Json.Nodes;
using CWI.Application.Interfaces.Services;

namespace CWI.Infrastructure.Services;

public class SensitiveDataMasker : ISensitiveDataMasker
{
    private const string MaskedValue = "***MASKED***";

    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "newPassword",
        "confirmPassword",
        "token",
        "refreshToken",
        "authorization",
        "secret",
        "apiKey"
    };

    public string? MaskJson(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return rawJson;
        }

        try
        {
            var root = JsonNode.Parse(rawJson);
            if (root == null)
            {
                return rawJson;
            }

            MaskNode(root);
            return root.ToJsonString();
        }
        catch
        {
            return rawJson;
        }
    }

    public string? MaskText(string? key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (!string.IsNullOrWhiteSpace(key) && IsSensitiveKey(key))
        {
            return MaskedValue;
        }

        return value;
    }

    public IReadOnlyDictionary<string, string?> MaskKeyValuePairs(IReadOnlyDictionary<string, string?> values)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in values)
        {
            result[pair.Key] = MaskText(pair.Key, pair.Value);
        }

        return result;
    }

    private static bool IsSensitiveKey(string key)
    {
        var normalized = key.Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);
        return SensitiveKeys.Contains(normalized) || SensitiveKeys.Any(k => normalized.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static void MaskNode(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            var properties = jsonObject.ToList();
            foreach (var property in properties)
            {
                if (property.Key != null && IsSensitiveKey(property.Key))
                {
                    jsonObject[property.Key] = MaskedValue;
                    continue;
                }

                if (property.Value != null)
                {
                    MaskNode(property.Value);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item != null)
                {
                    MaskNode(item);
                }
            }
        }
    }
}
