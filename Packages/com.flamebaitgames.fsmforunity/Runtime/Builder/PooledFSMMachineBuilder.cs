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
        private FSMMachineBuilder builder;
        private readonly Stack<FSMMachineBuilder> pool;
        private readonly Mutex mutex;
		public PooledFSMMachineBuilder(FSMMachineBuilder builder, Stack<FSMMachineBuilder> pool, Mutex mutex)
		{
			this.builder = builder;
			this.pool = pool;
			this.mutex = mutex;
		}

		public IFSMTransition AddAnyTransition(IFSMTransition transition, IFSMState to)
		{
			return builder.AddAnyTransition(transition, to);
		}

		public IFSMState AddState(IFSMState state)
		{
			return builder.AddState(state);
		}

		public IFSMTransition AddTransition(IFSMTransition transition, IFSMState from, IFSMState to)
		{
			return builder.AddTransition(transition, from, to);
		}

		public void SetDefaultState(IFSMState state)
		{
			builder.SetDefaultState(state);
        }

        public FSMMachine Complete(FSMMachineFlags behaviourParameters = FSMMachineFlags.Default)
        {
            var c = builder.Complete(behaviourParameters);
            builder.Dispose();
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
	}
}
