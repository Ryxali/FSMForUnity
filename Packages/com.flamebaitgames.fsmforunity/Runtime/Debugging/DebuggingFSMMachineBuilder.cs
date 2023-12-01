using UnityEngine;

namespace FSMForUnity
{
	internal sealed class DebuggingFSMMachineBuilder : FSMMachine.IBuilder
	{
		private readonly FSMMachine.IBuilder builder;

		private Object debugObject;

		public DebuggingFSMMachineBuilder(FSMMachine.IBuilder builder)
		{
			this.builder = builder;
		}

		public IFSMTransition AddAnyTransition(string name, IFSMTransition transition, IFSMState to)
		{
			return builder.AddAnyTransition(null, transition, to);
		}

		public IFSMState AddState(string name, IFSMState state)
		{
			return builder.AddState(null, state);
		}

		public IFSMTransition AddTransition(string name, IFSMTransition transition, IFSMState from, IFSMState to)
		{
			return builder.AddTransition(null, transition, from, to);
		}

		public FSMMachine Complete(FSMMachineFlags behaviourParameters = FSMMachineFlags.Default)
		{
			var machine = builder.Complete(behaviourParameters);
			DebuggingLinker.Link(machine, debugObject);
			return machine;
		}

		public void SetDebuggingInfo(string machineName, Object associatedObject)
		{
			debugObject = associatedObject;
			builder.SetDebuggingInfo(machineName, associatedObject);
		}

		public void SetDefaultState(IFSMState state)
		{
			builder.SetDefaultState(state);
		}

		void FSMMachine.IBuilder.Clear()
		{
			debugObject = null;
			builder.Clear();
		}
	}
}
