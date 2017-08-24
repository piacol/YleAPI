using System;

namespace YleAPI
{
    public class Singleton<T> where T : class, new()
    {
        private static volatile T instance = null;
        private static object syncRoot = new Object();

        protected Singleton() { }

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new T();
                    }
                }

                return instance;
            }
        }

        public static void Destory()
        {
            instance = null;
        }
    }
}