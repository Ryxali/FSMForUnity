using System;
using System.Collections.Generic;

namespace FSMForUnity.Editor
{
    internal class BatchResetObjectPool<T> where T : class
    {
        private readonly Stack<T> inactiveElements = new Stack<T>();
        private readonly Stack<T> activeElements = new Stack<T>();

        private readonly Func<T> factory;
        private readonly Action<T> reset;

        public BatchResetObjectPool(Func<T> factory, Action<T> reset)
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
                activeElements.Push(elem);
                return elem;
            }
            else
            {
                var elem = factory();
                activeElements.Push(elem);
                return elem;
            }
        }

        public void ReturnAll()
        {
            while (activeElements.Count > 0)
            {
                var elem = activeElements.Pop();
                reset(elem);
                inactiveElements.Push(elem);
            }
        }
    }
}
