using ManagedBass;
using UnityEngine;

namespace CCE.Utils
{
    public static class BassUtils
    {
        public static void PrintLastError()
        {
            if (Bass.LastError == Errors.OK)
                return;
            
            Debug.LogError($"Error with BASS {Bass.LastError:D}: {Bass.LastError:G}");
        }
    }
}
