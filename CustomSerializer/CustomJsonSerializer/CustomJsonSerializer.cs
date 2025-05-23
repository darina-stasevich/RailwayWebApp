using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace CustomJsonSerializer;

public class CustomJsonSerializer
{
    public string Serialize(object obj)
    {
        if (obj == null) return "null";
        var sb = new StringBuilder();
        WriteValue(obj, sb);
        return sb.ToString();
    }

    private void WriteValue(object value, StringBuilder sb)
    {
        if (value == null)
        {
            sb.Append("null");
            return;
        }

        Type type = value.GetType();
        Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;


        if (underlyingType == typeof(string) || underlyingType == typeof(Guid))
        {
            sb.Append("\"");
            sb.Append(EscapeJsonString(value.ToString()));
            sb.Append("\"");
        }
        else if (underlyingType == typeof(DateTime))
        {
            sb.Append("\"");
            sb.Append(((DateTime)value).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"));
            sb.Append("\"");
        }
        else if (underlyingType == typeof(DateOnly))
        {
            sb.Append("\"");
            sb.Append(((DateOnly)value).ToString("yyyy-MM-dd")); 
            sb.Append("\"");
        }
        else if (underlyingType == typeof(TimeSpan))
        {
            sb.Append("\"");
            sb.Append(((TimeSpan)value).ToString("c", CultureInfo.InvariantCulture));
            sb.Append("\"");
        }
        else if (underlyingType.IsPrimitive) 
        {
            if (value is bool b) sb.Append(b ? "true" : "false");
            else if (value is char c) { sb.Append("\""); sb.Append(EscapeJsonString(c.ToString())); sb.Append("\""); }
            else sb.Append(Convert.ToString(value, CultureInfo.InvariantCulture)); 
        }
        else if (underlyingType == typeof(decimal))
        {
             sb.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
        }
        else if (underlyingType.IsEnum)
        {
            sb.Append("\"");
            sb.Append(value.ToString()); 
            sb.Append("\"");
        }
        else if (value is IDictionary dict) 
        {
            WriteDictionary(dict, sb);
        }
        else if (value is IEnumerable enumerable && underlyingType != typeof(string)) 
        {
            WriteEnumerable(enumerable, sb);
        }
        else 
        {
            WriteObject(value, sb);
        }
    }

    private void WriteObject(object obj, StringBuilder sb)
    {
        sb.Append("{");
        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0).ToList();
        bool first = true;
        foreach (var prop in properties)
        {
            if (!first) sb.Append(",");
            first = false;

            sb.Append("\"");
            sb.Append(EscapeJsonString(prop.Name));
            sb.Append("\":");
            WriteValue(prop.GetValue(obj), sb);
        }
        sb.Append("}");
    }

    private void WriteDictionary(IDictionary dict, StringBuilder sb)
    {
        sb.Append("{");
        bool first = true;
        foreach (DictionaryEntry entry in dict)
        {
            if (!(entry.Key is string keyStr)) continue; 

            if (!first) sb.Append(",");
            first = false;

            sb.Append("\"");
            sb.Append(EscapeJsonString(keyStr));
            sb.Append("\":");
            WriteValue(entry.Value, sb);
        }
        sb.Append("}");
    }

    private void WriteEnumerable(IEnumerable enumerable, StringBuilder sb)
    {
        sb.Append("[");
        bool first = true;
        foreach (var item in enumerable)
        {
            if (!first) sb.Append(",");
            first = false;
            WriteValue(item, sb);
        }
        sb.Append("]");
    }

    private string EscapeJsonString(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;

        var sb = new StringBuilder(s.Length + 5);
        foreach (char c in s)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '"': sb.Append("\\\""); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < ' ')
                    {
                        sb.AppendFormat("\\u{0:x4}", (int)c);
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }
        return sb.ToString();
    }
    
    public T Deserialize<T>(string json) where T : new()
    {
        if (string.IsNullOrWhiteSpace(json))
            return default; 

        using (JsonDocument document = JsonDocument.Parse(json))
        {
            JsonElement root = document.RootElement;
            return (T)ParseElement(root, typeof(T));
        }
    }

    private object ParseElement(JsonElement element, Type targetType)
    {
        targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        switch (element.ValueKind)
        {
            case JsonValueKind.Null:
                return null;
            case JsonValueKind.String:
                string stringValue = element.GetString();
                if (targetType == typeof(string)) return stringValue;
                if (targetType == typeof(Guid)) return Guid.Parse(stringValue);
                if (targetType == typeof(DateTime)) return DateTime.Parse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                if (targetType == typeof(DateOnly)) return DateOnly.Parse(stringValue, CultureInfo.InvariantCulture);
                if (targetType == typeof(TimeSpan)) return TimeSpan.Parse(stringValue, CultureInfo.InvariantCulture);
                if (targetType.IsEnum) return Enum.Parse(targetType, stringValue);
                break;
            case JsonValueKind.Number:
                if (targetType == typeof(int)) return element.GetInt32();
                if (targetType == typeof(long)) return element.GetInt64();
                if (targetType == typeof(double)) return element.GetDouble();
                if (targetType == typeof(float)) return element.GetSingle();
                if (targetType == typeof(decimal)) return element.GetDecimal();
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                if (targetType == typeof(bool)) return element.GetBoolean();
                break;
            case JsonValueKind.Object:
                return ParseJsonObject(element, targetType);
            case JsonValueKind.Array:
                return ParseJsonArray(element, targetType);
        }
        throw new NotSupportedException($"Unsupported JSON value kind {element.ValueKind} or target type {targetType.Name}");
    }

    private object ParseJsonObject(JsonElement element, Type targetType)
    {
        if (typeof(IDictionary).IsAssignableFrom(targetType) &&
          targetType.IsGenericType &&
          targetType.GetGenericArguments()[0] == typeof(string))
        {
            var dict = (IDictionary)Activator.CreateInstance(targetType);
            Type valueType = targetType.GetGenericArguments()[1];
            foreach (JsonProperty property in element.EnumerateObject())
            {
                dict[property.Name] = ParseElement(property.Value, valueType);
            }
            return dict;
        }

        object instance = Activator.CreateInstance(targetType);
        foreach (JsonProperty property in element.EnumerateObject())
        {
            PropertyInfo propInfo = targetType.GetProperty(property.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propInfo != null && propInfo.CanWrite)
            {
                object propertyValue = ParseElement(property.Value, propInfo.PropertyType);
                propInfo.SetValue(instance, propertyValue);
            }
        }
        return instance;
    }

    private object ParseJsonArray(JsonElement element, Type targetType)
    {
        if (!targetType.IsArray && (!targetType.IsGenericType || targetType.GetGenericTypeDefinition() != typeof(List<>)))
        {
            throw new NotSupportedException($"Target type {targetType.Name} is not a supported array/list type.");
        }

        Type elementType;
        if (targetType.IsArray)
        {
            elementType = targetType.GetElementType();
        }
        else
        {
            elementType = targetType.GetGenericArguments()[0];
        }

        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
        foreach (JsonElement itemElement in element.EnumerateArray())
        {
            list.Add(ParseElement(itemElement, elementType));
        }

        if (targetType.IsArray)
        {
            Array arrayInstance = Array.CreateInstance(elementType, list.Count);
            list.CopyTo(arrayInstance, 0);
            return arrayInstance;
        }
        return list;
    }
}