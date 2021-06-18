using System;
using System.IO;
using UnityEngine;

namespace CCE.Core
{
    public static class Logging
    {
        public static void CreateLog(string logPath, string message)
        {
            if (!GlobalState.Config.DebugMode) return;
            File.WriteAllText(logPath, message);
            GlobalState.InAppLogString += message;
        }

        public static void AddToLog(string logPath, string message)
        {
            if (!GlobalState.Config.DebugMode) return;
            File.AppendAllText(logPath, message);
            GlobalState.InAppLogString += message;
        }

        public static void LogError(string caller, string message)
        {
            Debug.LogError($"CCELog: {message}\nIn: {caller}");
        }
    }
}