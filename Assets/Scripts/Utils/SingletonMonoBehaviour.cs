using UnityEngine;

namespace CCE.Utils
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (!_instance)
                {
                    CreateInstance();
                }

                return _instance;
            }
        }

        private static void CreateInstance()
        {
            _instance = new GameObject().AddComponent<T>();
            _instance.name = "SingletonCarrier" + typeof(T).Name;
            DontDestroyOnLoad(_instance);
        }
    }
}