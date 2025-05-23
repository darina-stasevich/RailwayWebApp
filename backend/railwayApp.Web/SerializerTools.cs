using System;
using System.Collections.Generic;
using System.Globalization; 
using System.Reflection; 
using System.Text;       
using System.Collections;

namespace railway_service;
    
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string Message { get; set; }
    public string LoggerName { get; set; }
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

public interface IMyCustomLogger
{
    void Info(string message, string loggerName = "Default", Dictionary<string, object> context = null);
    void Warn(string message, string loggerName = "Default", Exception exception = null, Dictionary<string, object> context = null);
    void Error(string message, string loggerName = "Default", Exception exception = null, Dictionary<string, object> context = null);
}

public class MyCustomFileJsonLogger : IMyCustomLogger
{
    private readonly string _logFilePath;
    private readonly CustomJsonSerializer.CustomJsonSerializer _serializer = new CustomJsonSerializer.CustomJsonSerializer();
    private static readonly object _fileLock = new object();

    public MyCustomFileJsonLogger(string logFileName = "D:\\Work\\csharp\\railway_service\\railway_service\\backend\\railwayApp.Web\\logs\\custom_application_log.jsonl")
    {
        string logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
        Directory.CreateDirectory(logDirectory);
        _logFilePath = Path.Combine(logDirectory, logFileName);
    }

    private void Log(LogEntry entry)
    {
        if (entry == null) return;

        try
        {
            string jsonLogEntry = _serializer.Serialize(entry);

            lock (_fileLock)
            {
                File.AppendAllText(_logFilePath, jsonLogEntry + Environment.NewLine);
            }

            Console.WriteLine(jsonLogEntry);
        }
        catch (Exception ex)
        {
           Console.WriteLine($"FATAL ERROR WRITING LOG: {ex.Message}. Original log: {entry?.Message}");
        }
    }

    public void Info(string message, string loggerName = "Default", Dictionary<string, object> context = null)
    {
        Log(new LogEntry
        {
            Level = "INFO",
            Message = message,
            LoggerName = loggerName,
            ContextData = context ?? new Dictionary<string, object>()
        });
    }

    public void Warn(string message, string loggerName = "Default", Exception exception = null, Dictionary<string, object> context = null)
    {
        Log(new LogEntry
        {
            Level = "WARN",
            Message = message,
            LoggerName = loggerName,
            ExceptionInfo = ExceptionDetails.FromException(exception),
            ContextData = context ?? new Dictionary<string, object>()
        });
    }

    public void Error(string message, string loggerName = "Default", Exception exception = null, Dictionary<string, object> context = null)
    {
        Log(new LogEntry
        {
            Level = "ERROR",
            Message = message,
            LoggerName = loggerName,
            ExceptionInfo = ExceptionDetails.FromException(exception),
            ContextData = context ?? new Dictionary<string, object>()
        });
    }
}