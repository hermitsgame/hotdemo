
using System;

namespace BoBao.Framework
{
    public abstract class BaseSingleton<T> where T : new()
    {
        static T instance;

        public static T Instance
        {
            get
            {
                if (BaseSingleton<T>.instance == null)
                    BaseSingleton<T>.instance = Activator.CreateInstance<T>();

                return BaseSingleton<T>.instance;
            }
        }
    }
}

