
using System;
using UnityEngine;

namespace BoBao.Framework
{
    public abstract class BaseMonoSingleton<T> : MonoBehaviour where T : BaseMonoSingleton<T>
    {
        static T m_Instance;

        public static T instance
        {
            get
            {
                if (BaseMonoSingleton<T>.m_Instance == null)
                {
                    BaseMonoSingleton<T>.m_Instance = (FindObjectOfType(typeof(T)) as T);
                    if (BaseMonoSingleton<T>.m_Instance == null)
                    {
                        GameObject ob = new GameObject();
                        ob.name = typeof(T).ToString();

                        BaseMonoSingleton<T>.m_Instance = ob.AddComponent<T>();
                        if (BaseMonoSingleton<T>.m_Instance == null)
                            Debug.LogError("Problem during the creation of " + typeof(T).ToString());
                    }
                }

                return BaseMonoSingleton<T>.m_Instance;
            }
        }

        public static T GetInstance()
        {
            return instance;
        }

		public static T InstanceOrNull() {
			return m_Instance;
		}

        protected virtual void Awake()
        {
            if (BaseMonoSingleton<T>.m_Instance == null)
                BaseMonoSingleton<T>.m_Instance = (this as T);

            BaseMonoSingleton<T>.m_Instance.Init();
            DontDestroyOnLoad(base.gameObject);
        }

        public virtual void Init() { }

        void OnApplicationQuit()
        {
            BaseMonoSingleton<T>.m_Instance = (T)((object)null);
        }
    }
}

