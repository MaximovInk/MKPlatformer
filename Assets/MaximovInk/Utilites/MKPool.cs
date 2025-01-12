using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MaximovInk
{
    public class MKPool<T> where T : Object
    {
        public int MaxPoolSize = 10;

        public List<T> Pool => _poolList;
        private readonly List<T> _poolList = new();

        private Func<T> createInstance;
        private Func<T> get;
        private Action<T> release;

        public MKPool(
            Func<T> createInstance,
            Func<T> get,
            Action<T> release 
                )
        {
            this.createInstance = createInstance;
            this.get = get;
            this.release = release;
        }

        public void Initialize()
        {
            for (int i = 0; i < MaxPoolSize; i++)
            {
                var t = createInstance();

                _poolList.Add(t);
            }
        }

        public bool TryGet(out T instance)
        {
            instance = get();
            return get != null;
        }

        public void Release(T instance)
        {
            release(instance);
        }


        /* protected abstract T CreateInstance();

         public abstract bool TryGet(out T instance);

         public abstract void Release(T instance);

         public abstract void CanGet();*/
    }
}
