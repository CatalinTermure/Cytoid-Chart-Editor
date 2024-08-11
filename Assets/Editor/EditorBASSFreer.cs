using CCE.Core;
using CCE.Utils;
using ManagedBass;
using UnityEditor;

namespace CCE.Editor
{
#if UNITY_EDITOR
    // The sole purpose of this class is to ascertain that BASS gets freed after exiting play mode.
    // Using OnApplicationQuit() has a lower chance of triggering while in the editor
    // leaving BASS in an unusable state, which then needs an editor restart to fix.
    [InitializeOnLoad]
    public class EditorBassFreer
    {
        static EditorBassFreer()
        {
            EditorApplication.playModeStateChanged += Quit;
        }

        private static void Quit(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode && AudioManager.IsInitialized)
            {
                Bass.Free();
                BassUtils.PrintLastError();
            }
        }
    }
#endif
}