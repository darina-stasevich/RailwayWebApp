using System;
using System.Collections.Generic;
using System.Globalization; // Для CultureInfo при конвертации чисел
using System.Reflection; // Для GetProperties
using System.Text;       // Для StringBuilder
using System.Collections;  // Для IDictionary, IEnumerable

namespace railway_service;
    
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } // "INFO", "WARN", "ERROR"
    public string Message { get; set; }
    public string LoggerName { get; set; } // Имя модуля/класса
    public string ThreadId { get; set; }
    public ExceptionDetails ExceptionInfo { get; set; }
    public Dictionary<string, object> ContextData { get; set; }

    public LogEntry()
    {
        Timestamp = DateTime.UtcNow;
        ThreadId = Environment.CurrentManagedThreadId.ToString();
        ContextData = new Dictionary<string, object>();
    }
}

public class ExceptionDetails
{
    public string Type { get; set; }
    public string Message { get; set; }
    public string StackTrace { get; set; }
    public ExceptionDetails InnerException { get; set; }

    public static ExceptionDetails FromException(Exception ex)
    {
        if (ex == null) return null;
        return new ExceptionDetails
        {
            Type = ex.GetType().FullName,
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            InnerException = FromException(ex.InnerException)
        };
    }
}