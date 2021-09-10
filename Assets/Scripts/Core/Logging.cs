using System;
using System.IO;
using UnityEngine;

namespace CCE.Core
{
    public static class Logging
    {
        public static void LogError(string caller, string message)
        {
            Debug.LogError($"CCELog: {message}\nIn: {caller}");
        }
    }
}