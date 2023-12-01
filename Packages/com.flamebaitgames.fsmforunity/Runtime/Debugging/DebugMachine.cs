using System.Collections.Generic;

namespace FSMForUnity
{
	internal struct DebugMachine
    {
		public readonly IDebuggableMachine machine;
		private readonly Dictionary<IFSMState, string> stateNames;
		private readonly Dictionary<FromToTransition, string> transitionNames;
		private readonly Dictionary<AnyTransition, string> anyTransitionNames;

		public DebugMachine(IDebuggableMachine machine,
            Dictionary<IFSMState, string> stateNames,
            Dictionary<FromToTransition, string> transitionNames,
            Dictionary<AnyTransition, string> anyTransitionNames)
        {
			this.machine = machine;
			this.stateNames = stateNames;
			this.transitionNames = transitionNames;
			this.anyTransitionNames = anyTransitionNames;
		}

		public override bool Equals(object obj)
		{
			return obj is DebugMachine machine &&
				   EqualityComparer<IDebuggableMachine>.Default.Equals(this.machine, machine.machine);
		}

		public override int GetHashCode()
		{
			return System.HashCode.Combine(machine);
		}
	}
}
