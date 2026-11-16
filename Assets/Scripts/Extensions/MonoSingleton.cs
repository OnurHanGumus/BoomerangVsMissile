using UnityEngine;

namespace Extensions
{
    public class MonoSingleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;
        private static bool _isQuitting;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_2023_1_OR_NEWER
                    T found = FindFirstObjectByType<T>();
#else
                    T found = FindObjectOfType<T>();
#endif
                    if (found != null)
                    {
                        _instance = found;
                        _isQuitting = false;
                    }
                    else if (!_isQuitting && ReferenceEquals(_instance, null))
                    {
                        GameObject newGo = new GameObject(typeof(T).Name);
                        _instance = newGo.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            _isQuitting = false;
            if (_instance == null)
            {
                _instance = this as T;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        protected virtual void OnDestroy()
        {
        }
    }
}
