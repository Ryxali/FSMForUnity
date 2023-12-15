using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace FSMForUnity
{
    /// <summary>
    /// Used to minimize GC from the builder itself, instead we recycle the object
    /// </summary>
    internal class PooledFSMMachineBuilder : FSMMachine.IBuilder
    {
        private FSMMachine.IBuilder builder;
        private readonly Stack<FSMMachine.IBuilder> pool;
        private readonly Mutex mutex;
        public PooledFSMMachineBuilder(FSMMachine.IBuilder builder, Stack<FSMMachine.IBuilder> pool, Mutex mutex)
        {
            this.builder = builder;
            this.pool = pool;
            this.mutex = mutex;
        }

        public IFSMTransition AddAnyTransition(string name, IFSMTransition transition, IFSMState to)
        {
            return builder.AddAnyTransition(name, transition, to);
        }

        public IFSMState AddState(string name, IFSMState state)
        {
            return builder.AddState(name, state);
        }

        public IFSMTransition AddTransition(string name, IFSMTransition transition, IFSMState from, IFSMState to)
        {
            return builder.AddTransition(name, transition, from, to);
        }

        public void SetDefaultState(IFSMState state)
        {
            builder.SetDefaultState(state);
        }

        public FSMMachine Complete(FSMMachineFlags behaviourParameters = FSMMachineFlags.Default)
        {
            var c = builder.Complete(behaviourParameters);
            builder.Clear();
            mutex.WaitOne();
            pool.Push(builder);
            mutex.ReleaseMutex();
            builder = null;
            return c;
        }

        public void SetDebuggingInfo(string machineName, Object associatedObject)
        {
            ((FSMMachine.IBuilder)builder).SetDebuggingInfo(machineName, associatedObject);
        }

        void FSMMachine.IBuilder.Clear()
        {
            // This is never invoked for this class due to how pooling works
        }
    }
}
