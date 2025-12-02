using System.Text.Json;
using System.Text.RegularExpressions;

namespace AutoStack.Infrastructure.Utilities;

/// <summary>
/// GDPR: This class sanitizes request bodies and exceptions to remove sensitive data.
/// Sensitive fields: password, token, secret, etc.
/// </summary>
public static partial class SanitizationHelper
{
    // A list of current and potential future fields (just to be on the safe side in case I forget to update this)
    private static readonly HashSet<string> SensitiveFieldNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "passwordhash",
        "newpassword",
        "oldpassword",
        "currentpassword",
        "confirmpassword",
        "token",
        "refreshtoken",
        "accesstoken",
        "bearertoken",
        "authorization",
        "authtoken",
        "secret",
        "apikey",
        "privatekey",
        "secretkey",
        "clientsecret",
        "cookie",
        "session",
        "sessionid",
        "csrf",
        "xsrf"
    };

    /// <summary>
    /// Sanitizes a JSON string by replacing sensitive field values with "[REDACTED]"
    /// </summary>
    public static string SanitizeJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return string.Empty;

        try
        {
            using var jsonDoc = JsonDocument.Parse(json);
            var sanitized = SanitizeJsonElement(jsonDoc.RootElement);
            return JsonSerializer.Serialize(sanitized, new JsonSerializerOptions
            {
                WriteIndented = false
            });
        }
        catch
        {
            return "[Invalid JSON - could not parse]";
        }
    }

    private static object? SanitizeJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => SanitizeJsonObject(element),
            JsonValueKind.Array  => SanitizeJsonArray(element),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetDouble(),
            JsonValueKind.True   => true,
            JsonValueKind.False  => false,
            JsonValueKind.Null   => null,
            _ => element.ToString()
        };
    }

    private static Dictionary<string, object?> SanitizeJsonObject(JsonElement element)
    {
        var result = new Dictionary<string, object?>();

        foreach (var property in element.EnumerateObject())
        {
            if (SensitiveFieldNames.Contains(property.Name))
            {
                result[property.Name] = "[REDACTED]";
            }
            else
            {
                result[property.Name] = SanitizeJsonElement(property.Value);
            }
        }

        return result;
    }

    private static List<object?> SanitizeJsonArray(JsonElement element)
    {
        var result = new List<object?>();

        foreach (var item in element.EnumerateArray())
        {
            result.Add(SanitizeJsonElement(item));
        }

        return result;
    }

    /// <summary>
    /// Sanitizes an exception by removing sensitive information like connection strings and file paths
    /// </summary>
    public static string SanitizeException(Exception exception)
    {
        if (exception == null)
            return string.Empty;

        var exceptionString = exception.ToString();

        exceptionString = ConnectionStringPasswordRegex().Replace(exceptionString, "$1=***$2");
        exceptionString = WindowsFilePathRegex().Replace(exceptionString, "***\\");
        exceptionString = UnixFilePathRegex().Replace(exceptionString, "***/");
        exceptionString = ApiKeyInUrlRegex().Replace(exceptionString, "$1=***");

        return exceptionString;
    }

    [GeneratedRegex(@"(password|pwd)=.*?(;|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex ConnectionStringPasswordRegex();

    [GeneratedRegex(@"[A-Z]:\\[^:\s]*\\", RegexOptions.Compiled)]
    private static partial Regex WindowsFilePathRegex();

    [GeneratedRegex(@"/(?:home|usr|var|opt)/[^\s:]+/", RegexOptions.Compiled)]
    private static partial Regex UnixFilePathRegex();

    [GeneratedRegex(@"(apikey|token|key|secret)=[^&\s]+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex ApiKeyInUrlRegex();
}
