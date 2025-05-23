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
    private const string DateTimeFormat = "o"; // ISO 8601 round-trip
    private const string NullMarker = "[NULL]";
    private const string EmptyListMarker = "[EMPTY_LIST]";
    private const string EmptyObjectMarker = "[EMPTY_OBJECT]"; // Для объектов без свойств или со всеми null свойствами

    // --- СЕРИАЛИЗАЦИЯ ---
    public void Serialize(object obj, string filePath)
    {
        if (obj == null)
        {
            File.WriteAllText(filePath, $"{NullMarker}"); // Корневой объект null
            return;
        }

        var lines = new List<string>();
        SerializeRecursive(obj, string.Empty, lines, new HashSet<object>()); // Добавляем HashSet для отслеживания объектов (базовая защита от циклов)
        
        if (!lines.Any() && obj.GetType() != typeof(string) && !IsSimpleType(obj.GetType())) // Если корневой объект не простой и не дал строк
        {
            // Это может быть пустой объект без свойств или коллекция, которая дала только [EMPTY_LIST] без префикса
            // Если это объект без свойств, добавим маркер
            if (!typeof(IEnumerable).IsAssignableFrom(obj.GetType()) || obj.GetType() == typeof(string))
            {
                 lines.Add($":{EmptyObjectMarker}"); // Специальный ключ для корневого пустого объекта
            }
        }
        // Сортировка строк обеспечивает консистентный вывод, что полезно для тестирования
        File.WriteAllLines(filePath, lines.OrderBy(l => l, StringComparer.Ordinal));
    }

    private void SerializeRecursive(object currentObject, string currentKeyPrefix, List<string> lines, HashSet<object> visitedObjects)
    {
        if (currentObject == null)
        {
            if (!string.IsNullOrEmpty(currentKeyPrefix)) // Только для свойств, не для корневого null
            {
                lines.Add($"{currentKeyPrefix}:{NullMarker}");
            }
            return;
        }

        Type objType = currentObject.GetType();

        // Базовая защита от циклических ссылок для ссылочных типов (не для ValueType)
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
        else if (typeof(IEnumerable).IsAssignableFrom(objType) && objType != typeof(string)) // Коллекции (кроме строк)
        {
            var collection = (IEnumerable)currentObject;
            int index = 0;
            bool collectionHasItems = false;
            foreach (var item in collection)
            {
                collectionHasItems = true;
                // Для каждого элемента создаем новый HashSet посещенных объектов, чтобы избежать ложных срабатываний циклов между разными элементами списка
                SerializeRecursive(item, $"{currentKeyPrefix}[{index}]", lines, new HashSet<object>(visitedObjects));
                index++;
            }
            if (!collectionHasItems && !string.IsNullOrEmpty(currentKeyPrefix))
            {
                lines.Add($"{currentKeyPrefix}:{EmptyListMarker}");
            }
        }
        else // Сложный объект (класс или структура)
        {
            PropertyInfo[] properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                             .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                                             .OrderBy(p => p.Name) // Сортируем свойства для консистентности
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
                // Если у объекта есть свойства, но все они null и не были записаны (т.к. SerializeRecursive для null не добавляет строку без префикса)
                // или если все свойства - пустые объекты/коллекции, которые не дали строк
                // Это сложный случай. Текущая логика: если свойство null, оно запишется как "Path.Prop:[NULL]".
                // Если объект состоит только из null-свойств, то для каждого будет такая строка.
            }
        }
        
        if (!objType.IsValueType && objType != typeof(string)) // Удаляем из посещенных при выходе из рекурсии для данного объекта
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
        if (type == typeof(TimeSpan)) return ((TimeSpan)value).ToString(); // TimeSpan.ToString() дает стандартный формат
        if (type == typeof(Guid)) return value.ToString();
        if (type == typeof(bool)) return ((bool)value).ToString(CultureInfo.InvariantCulture); // True / False
        if (type == typeof(float)) return ((float)value).ToString("R", CultureInfo.InvariantCulture); // "R" для round-trip
        if (type == typeof(double)) return ((double)value).ToString("R", CultureInfo.InvariantCulture);
        if (type == typeof(decimal)) return ((decimal)value).ToString(CultureInfo.InvariantCulture);
        if (type == typeof(DateOnly)) return ((DateOnly)value).ToString("O", CultureInfo.InvariantCulture);
        // Для остальных примитивов (int, char, byte, etc.) и string
        return Convert.ToString(value, CultureInfo.InvariantCulture);
    }

    // --- ДЕСЕРИАЛИЗАЦИЯ ---
    public T Deserialize<T>(string filePath) where T : new() // : new() может быть не всегда идеально для коллекций-интерфейсов
{
    if (!File.Exists(filePath)) throw new FileNotFoundException("Serialized file not found.", filePath);

    var lines = File.ReadAllLines(filePath);

    if (lines.Length == 0) // Пустой файл
    {
        Type rootTypeForEmpty = typeof(T);
        if (IsCollectionType(rootTypeForEmpty))
            return (T)CreateEmptyCollection(rootTypeForEmpty); // Пустая коллекция
        return new T(); // Пустой объект
    }

    // Обработка корневого [NULL]
    if (lines.Length == 1 && lines[0].Trim() == NullMarker)
    {
        return default(T); // null для ссылочных типов, default() для значимых
    }

    var flatData = new Dictionary<string, string>(StringComparer.Ordinal);
    foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)))
    {
        var parts = line.Split(new[] { ':' }, 2);
        if (parts.Length == 2)
        {
            flatData[parts[0].Trim()] = parts[1].Trim();
        }
        // Обработка корневых маркеров типа ":[EMPTY_LIST]" или ":[EMPTY_OBJECT]"
        else if (parts.Length == 1 && string.IsNullOrEmpty(parts[0].Trim()) && line.Trim().StartsWith(":"))
        {
            // Ключ будет пустой строкой, значение - сам маркер
            flatData[string.Empty] = line.Trim().Substring(1);
        }
    }

    Type rootType = typeof(T);

    // Проверка на корневые маркеры для пустых коллекций/объектов
    if (flatData.TryGetValue(string.Empty, out string rootMarkerValue))
    {
        if (rootMarkerValue == EmptyListMarker && IsCollectionType(rootType))
        {
            return (T)CreateEmptyCollection(rootType);
        }
        if (rootMarkerValue == EmptyObjectMarker && !IsSimpleType(rootType) && !IsCollectionType(rootType))
        {
            // Для new T() требуется constraint : new()
            return (T)Activator.CreateInstance(rootType);
        }
        // Если маркер есть, но тип не соответствует, это может быть ошибка формата
        // или просто файл с одним свойством без имени (что наш формат не предполагает)
    }
    
    // Если flatData пуст после всех проверок (например, файл был, но не содержал валидных строк ключ:значение)
    // и не было корневых маркеров
    if (!flatData.Any())
    {
        if (IsCollectionType(rootType))
            return (T)CreateEmptyCollection(rootType);
        return new T(); // Или (T)Activator.CreateInstance(rootType);
    }


    // --- ОСНОВНОЕ ВЕТВЛЕНИЕ ---
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
        // 1. Проверка прямого значения для текущего пути (для простых типов или маркеров)
        if (flatData.TryGetValue(currentPathPrefix, out string directValueStr))
        {
            if (directValueStr == NullMarker) return null;
            if (directValueStr == EmptyObjectMarker && !IsSimpleType(targetType) && !IsCollectionType(targetType)) return Activator.CreateInstance(targetType);
            // EmptyListMarker обрабатывается в BuildListRecursive
            if (IsSimpleType(targetType))
            {
                return ParseValue(directValueStr, targetType);
            }
            // Если это не простой тип и не маркер, это может быть некорректный формат,
            // или значение для сложного типа, которое не должно быть здесь (ожидаются подсвойства).
        }

        // Если для currentPathPrefix нет прямого значения, это должен быть сложный объект или коллекция.
        // Проверяем, есть ли вообще ключи, начинающиеся с этого префикса.
        // Если нет, и это не корневой вызов, то свойство могло отсутствовать в файле или быть null без явного маркера
        // (хотя наша сериализация всегда пишет [NULL]).
        bool hasRelevantKeys = flatData.Keys.Any(k => k.StartsWith(currentPathPrefix, StringComparison.Ordinal) && k != currentPathPrefix);
        if (!hasRelevantKeys && !string.IsNullOrEmpty(currentPathPrefix)) {
             // Если для этого пути нет ни прямого значения, ни дочерних, значит свойство отсутствует или было полностью пустым.
             // Возвращаем null для ссылочных типов, default для значимых.
             return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }


        object instance = Activator.CreateInstance(targetType); // Требует public конструктор без параметров

        PropertyInfo[] properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                           .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0).ToArray();

        foreach (PropertyInfo property in properties)
        {
            string propertyPath = ApplyPrefix(currentPathPrefix, property.Name);

            if (flatData.TryGetValue(propertyPath, out string valueStr)) // Есть прямое значение для свойства
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
                // Иначе (не простой тип, не маркер) - это ошибка формата, т.к. сложные типы должны иметь подсвойства
            }
            // Если прямого значения нет, проверяем, является ли свойство коллекцией или сложным объектом
            else if (IsCollectionType(property.PropertyType))
            {
                // Проверяем наличие элементов (ключи вида "propertyPath[0]")
                if (flatData.Keys.Any(k => k.StartsWith($"{propertyPath}[", StringComparison.Ordinal)))
                {
                    object listValue = BuildListRecursive(property.PropertyType, propertyPath, flatData);
                    property.SetValue(instance, listValue);
                }
                // Если элементов нет, и не было маркера EmptyListMarker, свойство останется null/default
            }
            else if (!IsSimpleType(property.PropertyType)) // Вложенный сложный объект
            {
                // Проверяем наличие подсвойств (ключи вида "propertyPath.NestedProp")
                if (flatData.Keys.Any(k => k.StartsWith($"{propertyPath}.", StringComparison.Ordinal)))
                {
                    object nestedValue = BuildObjectRecursive(property.PropertyType, propertyPath, flatData);
                    property.SetValue(instance, nestedValue);
                }
                // Если подсвойств нет, свойство останется null/default
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
        // Для интерфейсов и конкретных реализаций List<T>
        if (collectionType.IsGenericType)
        {
            var genericArgs = collectionType.GetGenericArguments();
            if (genericArgs.Length == 1)
            {
                Type itemType = genericArgs[0];
                var listGenericType = typeof(List<>).MakeGenericType(itemType);
                if (collectionType.IsAssignableFrom(listGenericType)) // Проверяем, можно ли присвоить List<T> этому типу коллекции
                {
                     return Activator.CreateInstance(listGenericType);
                }
            }
        }
        // Для конкретных типов коллекций с конструктором по умолчанию
        if (!collectionType.IsAbstract && !collectionType.IsInterface && collectionType.GetConstructor(Type.EmptyTypes) != null)
        {
             return Activator.CreateInstance(collectionType);
        }
        // По умолчанию, если не смогли создать специфичный тип, пробуем List<object> если это IEnumerable
        // Но это рискованно и может не подойти. Лучше выбросить исключение.
        throw new NotSupportedException($"Cannot create an empty instance of collection type {collectionType.FullName}. Ensure it's a supported collection type (like List<T>, T[], or has a parameterless constructor).");
    }

    private object BuildListRecursive(Type listType, string listPathPrefix, IReadOnlyDictionary<string, string> flatData)
    {
        Type itemType;
        if (listType.IsArray) itemType = listType.GetElementType();
        else if (listType.IsGenericType && (typeof(IList<>).IsAssignableFrom(listType.GetGenericTypeDefinition()) || typeof(ICollection<>).IsAssignableFrom(listType.GetGenericTypeDefinition()) || typeof(IEnumerable<>).IsAssignableFrom(listType.GetGenericTypeDefinition()) || listType.GetGenericTypeDefinition() == typeof(List<>) )) itemType = listType.GetGenericArguments()[0];
        else throw new NotSupportedException($"List type {listType.FullName} not supported for deserialization. Only T[], List<T> and compatible interfaces are supported.");

        var listInstanceInternal = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType)); // Внутренне работаем с List<T>
        
        // Собираем все элементы, которые относятся к этому списку
        var listElementPaths = new Dictionary<int, string>(); // index -> path prefix for this item
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
        
        if (!listElementPaths.Any()) { // Если не нашли ни одного элемента по ключам
             // Если был маркер [EMPTY_LIST] для самого listPathPrefix, он уже обработан в BuildObjectRecursive
             // Здесь мы просто возвращаем пустой типизированный список/массив
             if (listType.IsArray) return Array.CreateInstance(itemType, 0);
             return listInstanceInternal; // Пустой List<T>
        }

        int maxIndex = listElementPaths.Keys.Max();
        for (int i = 0; i <= maxIndex; i++)
        {
            if (listElementPaths.TryGetValue(i, out string itemPathPrefix))
            {
                object listItem;
                // Проверяем, есть ли прямое значение для элемента (простой тип или [NULL])
                if (flatData.TryGetValue(itemPathPrefix, out string directItemValueStr))
                {
                    if (directItemValueStr == NullMarker) listItem = null;
                    else listItem = ParseValue(directItemValueStr, itemType);
                }
                else // Элемент списка - сложный объект, строим его рекурсивно
                {
                    listItem = BuildObjectRecursive(itemType, itemPathPrefix, flatData);
                }
                listInstanceInternal.Add(listItem);
            }
            else
            {
                // Если индекс пропущен, добавляем null для ссылочных или default для значимых
                listInstanceInternal.Add(itemType.IsValueType ? Activator.CreateInstance(itemType) : null);
            }
        }
        
        if (listType.IsArray)
        {
            Array arrayInstance = Array.CreateInstance(itemType, listInstanceInternal.Count);
            listInstanceInternal.CopyTo(arrayInstance, 0);
            return arrayInstance;
        }
        // Если listType это, например, IList<T>, то созданный List<T> ему присвоится.
        // Если listType - это конкретный тип, который не List<T>, но реализует IList, то могут быть проблемы,
        // если нет конструктора, принимающего IEnumerable<T>.
        // Для простоты, мы возвращаем List<T>, который должен быть присваиваемым к IList<T>, IEnumerable<T>, ICollection<T>.
        // Если нужен был конкретный тип коллекции, отличный от List<T>, то CreateEmptyCollection должен был бы это учесть,
        // и здесь нужно было бы заполнять его, а не List<T>.
        return listInstanceInternal;
    }

    private object ParseValue(string valueStr, Type targetType)
    {
        if (valueStr == NullMarker) // Хотя обычно ParseValue вызывается, когда значение не NullMarker
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
            // Для совместимости с некоторыми системами, можно проверять "1" / "0"
            if (valueStr == "1") return true;
            if (valueStr == "0") return false;
            throw new FormatException($"Cannot parse '{valueStr}' as boolean for type {targetType.Name}.");
        }
        // Для остальных примитивов (int, double, string, decimal и т.д.)
        // Используем Convert.ChangeType с CultureInfo.InvariantCulture для корректного парсинга чисел
        return Convert.ChangeType(valueStr, actualTargetType, CultureInfo.InvariantCulture);
    }
}