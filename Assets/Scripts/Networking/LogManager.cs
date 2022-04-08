using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

/// <summary>
/// Enables the client to resend logs that where issued while the client to server connection wasn't yet established
/// Is also useful if for some reason the client connection to the network is not established directly at the start of the application
/// </summary>
public class LogStack
{
    private static List<Log> Logs = new List<Log>();

    public static void AddLog(Log log)
    {
        Logs.Add(log);
    }

    public static List<Log> GetLogs()
    {
        return Logs;
    }

    public static void ClearLogs()
    {
        Logs.Clear();
    }
}

public class Log
{
    public string Timestamp { get; set; }
    public int LogType { get; set; }
    public string Message { get; set; }
}

public class LogManager : MonoBehaviour
{
    public enum LogTypeApp
    {
        Normal = 0,
        Warning = 1,
        Error = 2
    }

    void OnEnable()
    {
        Application.logMessageReceived += LogCallback;
    }

    /// <summary>
    /// Callback whenever a log/exception is issued
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="stackTrace"></param>
    /// <param name="type"></param>
    void LogCallback(string condition, string stackTrace, LogType type)
    {
        LogTypeApp logTypeApp;

        if (type == LogType.Error || type == LogType.Exception)
        {
            logTypeApp = LogTypeApp.Error;
        }
        else if (type == LogType.Warning)
        {
            logTypeApp = LogTypeApp.Warning;
        }
        else
        {
            logTypeApp = LogTypeApp.Normal;
        }

        // ignore normal logs
        if (logTypeApp != LogTypeApp.Normal)
        {
            LogData(type + " - condition: " + condition + ", stacktrace: " + stackTrace, logTypeApp);
        }
    }

    void OnDisable()
    {
        Application.logMessageReceived -= LogCallback;
    }

    /// <summary>
    /// Logs a message to the controller app
    /// </summary>
    /// <param name="message"></param>
    /// <param name="logType"></param>
    public static void LogData(string messageText, LogTypeApp logType)
    {
        if (NetworkManager.Singleton.Client == null || !NetworkManager.Singleton.Client.IsConnected)
        {
            // push the log to the log stack to send it later once the server connection was established
            LogStack.AddLog(new Log { LogType = (int)logType, Message = messageText });
        }
        else
        {
            // got connection to server -> send it directly via riptide
            Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.log);
            message.AddInt((int)logType);
            message.AddString(messageText);
            NetworkManager.Singleton.Client.Send(message);
        }
    }

    /// <summary>
    /// Overladed function -> uses LogTypeApp.Normal as default
    /// </summary>
    /// <param name="message"></param>
    public static void LogData(string message)
    {
        LogData(message, LogTypeApp.Normal);
    }
}
