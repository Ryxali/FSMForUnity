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
			builder.Clear();
		}
	}
}
