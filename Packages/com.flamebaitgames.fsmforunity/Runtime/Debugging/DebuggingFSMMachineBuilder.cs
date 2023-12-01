using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FSMForUnity
{
	internal sealed class DebuggingFSMMachineBuilder : FSMMachine.IBuilder
	{
		private readonly FSMMachine.IBuilder builder;

		private Dictionary<IFSMState, string> stateNames = new Dictionary<IFSMState, string>(EqualityComparer_IFSMState.constant);
		private Dictionary<FromToTransition, string> transitionNames = new Dictionary<FromToTransition, string>(EqualityComparer_FromToTransition.constant);
		private Dictionary<AnyTransition, string> anyTransitionNames = new Dictionary<AnyTransition, string>(EqualityComparer_AnyTransition.constant);

		private Object debugObject;

		public DebuggingFSMMachineBuilder(FSMMachine.IBuilder builder)
		{
			this.builder = builder;
		}

		public IFSMState AddState(string name, IFSMState state)
		{
			stateNames.Add(state, name);
			return builder.AddState(name, state);
		}

		public IFSMTransition AddTransition(string name, IFSMTransition transition, IFSMState from, IFSMState to)
		{
			transitionNames.Add(new FromToTransition { transition = transition, from = from, to = to }, name);
			return builder.AddTransition(name, transition, from, to);
		}

		public IFSMTransition AddAnyTransition(string name, IFSMTransition transition, IFSMState to)
		{
			anyTransitionNames.Add(new AnyTransition { transition = transition, to = to }, name);
			return builder.AddAnyTransition(name, transition, to);
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

		public FSMMachine Complete(FSMMachineFlags behaviourParameters = FSMMachineFlags.Default)
		{
			var machine = builder.Complete(behaviourParameters);
			DebuggingLinker.Link(new DebugMachine(machine, stateNames, transitionNames, anyTransitionNames), debugObject);
			stateNames = new Dictionary<IFSMState, string>();
			transitionNames = new Dictionary<FromToTransition, string>();
			anyTransitionNames = new Dictionary<AnyTransition, string>();
			return machine;
		}

		void FSMMachine.IBuilder.Clear()
		{
			debugObject = null;
			builder.Clear();
			stateNames.Clear();
			transitionNames.Clear();
			anyTransitionNames.Clear();
		}
	}
}
