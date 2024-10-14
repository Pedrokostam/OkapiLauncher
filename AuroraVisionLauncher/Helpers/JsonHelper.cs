using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AuroraVisionLauncher.Helpers;
public static class JsonHelper
{
    public static T? ReadElement<T>(object? potentialJsonElementOrObject, Func<JsonElement, T?>? converter = null, T? defaultValue=default)
    {
        if (potentialJsonElementOrObject is T expected)
        {
            return expected;
        }
        if (potentialJsonElementOrObject is JsonElement jelem)
        {
            if (typeof(T).IsEnum && Enum.TryParse(typeof(T),jelem.GetString(),out var result ))
            {
                return (T)result!;
            }
            if (converter is null)
            {
                return JsonSerializer.Deserialize<T>(jelem);
            }
            return converter(jelem);
        }
        return defaultValue;
    }
    public static T? ReadElement<T>(this IDictionary dict, string key, Func<JsonElement, T?>? converter = null, T? defaultValue = default)
    {
        if (dict.Contains(key))
        {
            return ReadElement<T>(dict[key], converter,defaultValue);
        }
        return defaultValue;
    }
    public static void InitializeKey<T>(this IDictionary dict, string key, Func<JsonElement, T?>? converter = null, T? defaultValue = default)
    {
        var value = dict.ReadElement(key,converter,defaultValue);
        dict[key] = value;
    }
}
