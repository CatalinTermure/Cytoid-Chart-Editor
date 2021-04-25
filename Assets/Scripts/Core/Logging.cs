using System.IO;

namespace CCE.Core
{
    public static class Logging
    {
        public static void CreateLog(string logPath, string message)
        {
            if (GlobalState.Config.DebugMode)
            {
                File.WriteAllText(logPath, message);
                GlobalState.InAppLogString += message;
            }
        }

        public static void AddToLog(string logPath, string message)
        {
            if (GlobalState.Config.DebugMode)
            {
                File.AppendAllText(logPath, message);
                GlobalState.InAppLogString += message;
            }
        }
    }
}