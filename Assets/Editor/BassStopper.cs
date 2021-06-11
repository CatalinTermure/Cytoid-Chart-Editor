using ManagedBass;
using UnityEditor;
using UnityEngine;

namespace CCE.Editor
{
    [InitializeOnLoad]
    public class BassStopper : MonoBehaviour
    {
        private static void Quit()
        {
            // Bass.Stop();
        }
        
        static BassStopper()
        {
            EditorApplication.quitting += Quit;
        }
    }
}
