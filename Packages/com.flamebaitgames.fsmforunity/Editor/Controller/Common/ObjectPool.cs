using System;
using System.Collections.Generic;

namespace FSMForUnity.Editor
{
    internal class ObjectPool<T> where T : class
    {
        private readonly Stack<T> inactiveElements = new Stack<T>();

        private readonly Func<T> factory;
        private readonly Action<T> reset;

        public ObjectPool(Func<T> factory, Action<T> reset)
        {
            this.factory = factory;
            this.reset = reset;
        }

        public T Take()
        {
            if (inactiveElements.Count > 0)
            {
                var elem = inactiveElements.Pop();
                reset(elem);
                return elem;
            }
            else
            {
                var elem = factory();
                return elem;
            }
        }

        public void Return(T item)
        {
            reset(item);
            inactiveElements.Push(item);
        }
    }
}
