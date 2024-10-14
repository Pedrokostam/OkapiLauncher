using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AuroraVisionLauncher.Helpers;
public static class JsonHelper
{
    public class ColorConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                return (Color)System.Windows.Media.ColorConverter.ConvertFromString(reader.GetString());

            }
            catch (FormatException)
            {
                return default;
            }
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            var str = value.ToString(provider: null);
            writer.WriteStringValue(str);
        }
    }
    public static readonly JsonSerializerOptions Options = new JsonSerializerOptions()
    {
        WriteIndented = true,
        AllowTrailingCommas = true,
        IgnoreReadOnlyProperties = true,
        IgnoreReadOnlyFields = true,
        Converters = {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
           new ColorConverter(),
        },
    };
    public static T? ReadElement<T>(object? potentialJsonElementOrObject, Func<JsonElement, T?>? converter = null, T? defaultValue = default)
    {
        if (potentialJsonElementOrObject is T expected)
        {
            return expected;
        }
        if (potentialJsonElementOrObject is JsonElement jelem)
        {
            if (typeof(T).IsEnum && Enum.TryParse(typeof(T), jelem.GetString(), true, out var result))
            {
                return (T)result!;
            }
            if (converter is null)
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(jelem, Options);
                }
                catch (JsonException)
                {
                    return default;
                }
            }
            return converter(jelem);
        }
        return defaultValue;
    }
    public static IEnumerable<T?> ReadElementList<T>(object? potentialJsonElementOrObject, Func<JsonElement, T?>? converter = null, T? defaultValue = default)
    {
        if (potentialJsonElementOrObject is IEnumerable<T?> enumerable)
        {
            return enumerable;
        }
        if (potentialJsonElementOrObject is JsonElement jelem)
        {
            List<T?> result = [];
            foreach (var item in jelem.EnumerateArray())
            {
                result.Add(ReadElement(item, converter, defaultValue));
            }
            return result;
        }
        return [];
    }
    public static T? ReadElement<T>(this IDictionary dict, string key, Func<JsonElement, T?>? converter = null, T? defaultValue = default)
    {
        if (dict.Contains(key))
        {
            return ReadElement<T>(dict[key], converter, defaultValue);
        }
        return defaultValue;
    }
    public static IEnumerable<T?> ReadElementList<T>(this IDictionary dict, string key, Func<JsonElement, T?>? converter = null, T? defaultValue = default)
    {
        if (dict.Contains(key))
        {
            return ReadElementList<T>(dict[key], converter, defaultValue);
        }
        return [];
    }
    public static void InitializeDictKey<T>(this IDictionary dict, string key, Func<JsonElement, T?>? converter = null, T? defaultValue = default)
    {
        var value = dict.ReadElement(key, converter, defaultValue);
        dict[key] = value;
    }
}
