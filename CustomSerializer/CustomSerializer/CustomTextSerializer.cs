using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CustomSerializer;

public class CustomTextSerializer
{
    private const string DateTimeFormat = "o";
    private const string NullMarker = "[NULL]";
    private const string EmptyListMarker = "[EMPTY_LIST]";
    private const string EmptyObjectMarker = "[EMPTY_OBJECT]"; 
    public void Serialize(object obj, string filePath)
    {
        if (obj == null)
        {
            File.WriteAllText(filePath, $"{NullMarker}");
            return;
        }

        var lines = new List<string>();
        SerializeRecursive(obj, string.Empty, lines, new HashSet<object>());
        
        if (!lines.Any() && obj.GetType() != typeof(string) && !IsSimpleType(obj.GetType())) 
        {
            if (!typeof(IEnumerable).IsAssignableFrom(obj.GetType()) || obj.GetType() == typeof(string))
            {
                 lines.Add($":{EmptyObjectMarker}"); 
            }
        }
        File.WriteAllLines(filePath, lines.OrderBy(l => l, StringComparer.Ordinal));
    }

    private void SerializeRecursive(object currentObject, string currentKeyPrefix, List<string> lines, HashSet<object> visitedObjects)
    {
        if (currentObject == null)
        {
            if (!string.IsNullOrEmpty(currentKeyPrefix)) 
            {
                lines.Add($"{currentKeyPrefix}:{NullMarker}");
            }
            return;
        }

        Type objType = currentObject.GetType();

        if (!objType.IsValueType && objType != typeof(string))
        {
            if (visitedObjects.Contains(currentObject))
            {
                lines.Add($"{currentKeyPrefix}:[CIRCULAR_REFERENCE_DETECTED]");
                return;
            }
            visitedObjects.Add(currentObject);
        }

        if (IsSimpleType(objType))
        {
            lines.Add($"{currentKeyPrefix}:{FormatValue(currentObject)}");
        }
        else if (typeof(IEnumerable).IsAssignableFrom(objType) && objType != typeof(string)) 
        {
            var collection = (IEnumerable)currentObject;
            int index = 0;
            bool collectionHasItems = false;
            foreach (var item in collection)
            {
                collectionHasItems = true;
                SerializeRecursive(item, $"{currentKeyPrefix}[{index}]", lines, new HashSet<object>(visitedObjects));
                index++;
            }
            if (!collectionHasItems && !string.IsNullOrEmpty(currentKeyPrefix))
            {
                lines.Add($"{currentKeyPrefix}:{EmptyListMarker}");
            }
        }
        else 
        {
            PropertyInfo[] properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                             .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                                             .OrderBy(p => p.Name) 
                                             .ToArray();
            
            if (!properties.Any() && !string.IsNullOrEmpty(currentKeyPrefix))
            {
                 lines.Add($"{currentKeyPrefix}:{EmptyObjectMarker}");
            }
            else
            {
                bool propertyWritten = false;
                foreach (PropertyInfo property in properties)
                {
                    object propertyValue = property.GetValue(currentObject);
                    SerializeRecursive(propertyValue, ApplyPrefix(currentKeyPrefix, property.Name), lines, visitedObjects);
                    propertyWritten = true; 
                }
            }
        }
        
        if (!objType.IsValueType && objType != typeof(string)) 
        {
            visitedObjects.Remove(currentObject);
        }
    }

    private string ApplyPrefix(string prefix, string propertyName)
    {
        return string.IsNullOrEmpty(prefix) ? propertyName : $"{prefix}.{propertyName}";
    }

    private bool IsSimpleType(Type type)
    {
        Type actualType = Nullable.GetUnderlyingType(type) ?? type;
        return actualType.IsPrimitive ||
               actualType.IsEnum ||
               actualType == typeof(string) ||
               actualType == typeof(decimal) ||
               actualType == typeof(DateTime) ||
               actualType == typeof(TimeSpan) ||
               actualType == typeof(Guid) ||
               actualType == typeof(DateOnly);
    }

    private string FormatValue(object value)
    {
        Type type = value.GetType();

        if (type.IsEnum) return value.ToString();
        if (type == typeof(DateTime)) return ((DateTime)value).ToUniversalTime().ToString(DateTimeFormat);
        if (type == typeof(TimeSpan)) return ((TimeSpan)value).ToString(); 
        if (type == typeof(Guid)) return value.ToString();
        if (type == typeof(bool)) return ((bool)value).ToString(CultureInfo.InvariantCulture);
        if (type == typeof(float)) return ((float)value).ToString("R", CultureInfo.InvariantCulture);
        if (type == typeof(double)) return ((double)value).ToString("R", CultureInfo.InvariantCulture);
        if (type == typeof(decimal)) return ((decimal)value).ToString(CultureInfo.InvariantCulture);
        if (type == typeof(DateOnly)) return ((DateOnly)value).ToString("O", CultureInfo.InvariantCulture);
        
        return Convert.ToString(value, CultureInfo.InvariantCulture);
    }

    public T Deserialize<T>(string filePath) where T : new()
{
    if (!File.Exists(filePath)) throw new FileNotFoundException("Serialized file not found.", filePath);

    var lines = File.ReadAllLines(filePath);

    if (lines.Length == 0)
    {
        Type rootTypeForEmpty = typeof(T);
        if (IsCollectionType(rootTypeForEmpty))
            return (T)CreateEmptyCollection(rootTypeForEmpty);
        return new T(); 
    }

    if (lines.Length == 1 && lines[0].Trim() == NullMarker)
    {
        return default;
    }

    var flatData = new Dictionary<string, string>(StringComparer.Ordinal);
    foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)))
    {
        var parts = line.Split(new[] { ':' }, 2);
        if (parts.Length == 2)
        {
            flatData[parts[0].Trim()] = parts[1].Trim();
        }
        else if (parts.Length == 1 && string.IsNullOrEmpty(parts[0].Trim()) && line.Trim().StartsWith(":"))
        {
            flatData[string.Empty] = line.Trim().Substring(1);
        }
    }

    Type rootType = typeof(T);

    if (flatData.TryGetValue(string.Empty, out string rootMarkerValue))
    {
        if (rootMarkerValue == EmptyListMarker && IsCollectionType(rootType))
        {
            return (T)CreateEmptyCollection(rootType);
        }
        if (rootMarkerValue == EmptyObjectMarker && !IsSimpleType(rootType) && !IsCollectionType(rootType))
        {
            return (T)Activator.CreateInstance(rootType);
        }
    }
    
    if (!flatData.Any())
    {
        if (IsCollectionType(rootType))
            return (T)CreateEmptyCollection(rootType);
        return new T();
    }


    if (IsCollectionType(rootType))
    {
        return (T)BuildListRecursive(rootType, string.Empty, flatData);
    }
    else
    {
        return (T)BuildObjectRecursive(rootType, string.Empty, flatData);
    }
}

    private object BuildObjectRecursive(Type targetType, string currentPathPrefix, IReadOnlyDictionary<string, string> flatData)
    {
        if (flatData.TryGetValue(currentPathPrefix, out string directValueStr))
        {
            if (directValueStr == NullMarker) return null;
            if (directValueStr == EmptyObjectMarker && !IsSimpleType(targetType) && !IsCollectionType(targetType)) return Activator.CreateInstance(targetType);
            if (IsSimpleType(targetType))
            {
                return ParseValue(directValueStr, targetType);
            }
        }

        bool hasRelevantKeys = flatData.Keys.Any(k => k.StartsWith(currentPathPrefix, StringComparison.Ordinal) && k != currentPathPrefix);
        if (!hasRelevantKeys && !string.IsNullOrEmpty(currentPathPrefix)) {
             return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }


        object instance = Activator.CreateInstance(targetType); 

        PropertyInfo[] properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                           .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0).ToArray();

        foreach (PropertyInfo property in properties)
        {
            string propertyPath = ApplyPrefix(currentPathPrefix, property.Name);

            if (flatData.TryGetValue(propertyPath, out string valueStr))
            {
                if (valueStr == NullMarker)
                {
                    property.SetValue(instance, null);
                }
                else if (valueStr == EmptyListMarker && IsCollectionType(property.PropertyType))
                {
                    property.SetValue(instance, CreateEmptyCollection(property.PropertyType));
                }
                else if (valueStr == EmptyObjectMarker && !IsSimpleType(property.PropertyType) && !IsCollectionType(property.PropertyType))
                {
                    property.SetValue(instance, Activator.CreateInstance(property.PropertyType));
                }
                else if (IsSimpleType(property.PropertyType))
                {
                    property.SetValue(instance, ParseValue(valueStr, property.PropertyType));
                }
            }
            else if (IsCollectionType(property.PropertyType))
            {
                if (flatData.Keys.Any(k => k.StartsWith($"{propertyPath}[", StringComparison.Ordinal)))
                {
                    object listValue = BuildListRecursive(property.PropertyType, propertyPath, flatData);
                    property.SetValue(instance, listValue);
                }
            }
            else if (!IsSimpleType(property.PropertyType))
            {
                if (flatData.Keys.Any(k => k.StartsWith($"{propertyPath}.", StringComparison.Ordinal)))
                {
                    object nestedValue = BuildObjectRecursive(property.PropertyType, propertyPath, flatData);
                    property.SetValue(instance, nestedValue);
                }
            }
        }
        return instance;
    }
    
    private bool IsCollectionType(Type type)
    {
        return typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string);
    }

    private object CreateEmptyCollection(Type collectionType)
    {
        if (collectionType.IsArray)
        {
            return Array.CreateInstance(collectionType.GetElementType(), 0);
        }
        if (collectionType.IsGenericType)
        {
            var genericArgs = collectionType.GetGenericArguments();
            if (genericArgs.Length == 1)
            {
                Type itemType = genericArgs[0];
                var listGenericType = typeof(List<>).MakeGenericType(itemType);
                if (collectionType.IsAssignableFrom(listGenericType)){
                     return Activator.CreateInstance(listGenericType);
                }
            }
        }
        if (!collectionType.IsAbstract && !collectionType.IsInterface && collectionType.GetConstructor(Type.EmptyTypes) != null)
        {
             return Activator.CreateInstance(collectionType);
        }
        throw new NotSupportedException($"Cannot create an empty instance of collection type {collectionType.FullName}. Ensure it's a supported collection type (like List<T>, T[], or has a parameterless constructor).");
    }

    private object BuildListRecursive(Type listType, string listPathPrefix, IReadOnlyDictionary<string, string> flatData)
    {
        Type itemType;
        if (listType.IsArray) itemType = listType.GetElementType();
        else if (listType.IsGenericType && (typeof(IList<>).IsAssignableFrom(listType.GetGenericTypeDefinition()) || typeof(ICollection<>).IsAssignableFrom(listType.GetGenericTypeDefinition()) || typeof(IEnumerable<>).IsAssignableFrom(listType.GetGenericTypeDefinition()) || listType.GetGenericTypeDefinition() == typeof(List<>) )) itemType = listType.GetGenericArguments()[0];
        else throw new NotSupportedException($"List type {listType.FullName} not supported for deserialization. Only T[], List<T> and compatible interfaces are supported.");

        var listInstanceInternal = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
        
        var listElementPaths = new Dictionary<int, string>(); 
        string prefixWithBracket = listPathPrefix + "[";

        foreach (var key in flatData.Keys.Where(k => k.StartsWith(prefixWithBracket, StringComparison.Ordinal)))
        {
            int closingBracketIndex = key.IndexOf(']', prefixWithBracket.Length);
            if (closingBracketIndex > prefixWithBracket.Length)
            {
                if (int.TryParse(key.AsSpan(prefixWithBracket.Length, closingBracketIndex - prefixWithBracket.Length), out int index))
                {
                    if (!listElementPaths.ContainsKey(index))
                    {
                         listElementPaths[index] = $"{listPathPrefix}[{index}]";
                    }
                }
            }
        }
        
        if (!listElementPaths.Any()) {
             if (listType.IsArray) return Array.CreateInstance(itemType, 0);
             return listInstanceInternal; 
        }

        int maxIndex = listElementPaths.Keys.Max();
        for (int i = 0; i <= maxIndex; i++)
        {
            if (listElementPaths.TryGetValue(i, out string itemPathPrefix))
            {
                object listItem;
                if (flatData.TryGetValue(itemPathPrefix, out string directItemValueStr))
                {
                    if (directItemValueStr == NullMarker) listItem = null;
                    else listItem = ParseValue(directItemValueStr, itemType);
                }
                else 
                {
                    listItem = BuildObjectRecursive(itemType, itemPathPrefix, flatData);
                }
                listInstanceInternal.Add(listItem);
            }
            else
            {
                listInstanceInternal.Add(itemType.IsValueType ? Activator.CreateInstance(itemType) : null);
            }
        }
        
        if (listType.IsArray)
        {
            Array arrayInstance = Array.CreateInstance(itemType, listInstanceInternal.Count);
            listInstanceInternal.CopyTo(arrayInstance, 0);
            return arrayInstance;
        } 
        return listInstanceInternal;
    }

    private object ParseValue(string valueStr, Type targetType)
    {
        if (valueStr == NullMarker)
        {
            if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                throw new FormatException($"Cannot assign [NULL] to non-nullable value type {targetType.Name}"); 
            return null;
        }

        Type actualTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (actualTargetType.IsEnum) return Enum.Parse(actualTargetType, valueStr, true);
        if (actualTargetType == typeof(Guid)) return Guid.Parse(valueStr);
        if (actualTargetType == typeof(DateTime)) return DateTime.ParseExact(valueStr, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        if (actualTargetType == typeof(TimeSpan)) return TimeSpan.Parse(valueStr, CultureInfo.InvariantCulture);
        if (actualTargetType == typeof(DateOnly)) return DateOnly.ParseExact(valueStr, "O", CultureInfo.InvariantCulture);
        if (actualTargetType == typeof(bool))
        {
            if (bool.TryParse(valueStr, out bool boolResult)) return boolResult;
            if (valueStr == "1") return true;
            if (valueStr == "0") return false;
            throw new FormatException($"Cannot parse '{valueStr}' as boolean for type {targetType.Name}.");
        }
        return Convert.ChangeType(valueStr, actualTargetType, CultureInfo.InvariantCulture);
    }
}