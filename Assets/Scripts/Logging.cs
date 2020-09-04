using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Logging
{
    public static void CreateLog(string logpath, string message)
    {
        if(GlobalState.Config.DebugMode)
        {
            File.WriteAllText(logpath, message);
            GlobalState.InAppLogString += message;
        }
    }

    public static void AddToLog(string logpath, string message)
    {
        if(GlobalState.Config.DebugMode)
        {
            File.AppendAllText(logpath, message);
            GlobalState.InAppLogString += message;
        }
    }
}
