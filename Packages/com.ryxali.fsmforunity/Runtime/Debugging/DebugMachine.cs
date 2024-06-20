using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FSMForUnity
{
    internal struct DebugMachine
    {
        public bool IsValid => machine != null;

        public string Name => machine.GetName();

        public IFSMState DefaultState => machine.GetDefaultState();

        public IFSMState[] States => machine.GetAllStates();

        private readonly IDebuggableMachine machine;
        private readonly Dictionary<IFSMState, string> stateNames;
        private readonly Dictionary<FromToTransition, string> transitionNames;
        private readonly Dictionary<AnyTransition, string> anyTransitionNames;
        private readonly EventTrail eventHistory;
        /// <summary>
        /// The line where this machine was completed.
        /// </summary>
        private readonly StackTrace stackTrace;

        public DebugMachine(IDebuggableMachine machine,
            Dictionary<IFSMState, string> stateNames,
            Dictionary<FromToTransition, string> transitionNames,
            Dictionary<AnyTransition, string> anyTransitionNames,
            EventTrail eventHistory, StackTrace stackTrace)
        {
            this.machine = machine;
            this.stateNames = stateNames;
            this.transitionNames = transitionNames;
            this.anyTransitionNames = anyTransitionNames;
            this.eventHistory = eventHistory;
            this.stackTrace = stackTrace;
        }

        public DebugMachine(IDebuggableMachine machine)
        {
            this.machine = machine;
            stateNames = null;
            transitionNames = null;
            anyTransitionNames = null;
            eventHistory = null;
            stackTrace = null;
        }

        public bool TryGetActive(out IFSMState state) => machine.TryGetActive(out state);

        public bool TryGetTransitionsFrom(IFSMState state, out TransitionMapping[] transitions) => machine.TryGetTransitionsFrom(state, out transitions);

        public bool TryGetAnyTransitions(out TransitionMapping[] anyTransitions) => machine.TryGetAnyTransitions(out anyTransitions);

        public string GetStateName(IFSMState state) => stateNames[state];

        public string GetTransitionName(IFSMTransition transition, IFSMState from, IFSMState to)
        {
            var tuple = new FromToTransition
            {
                transition = transition,
                from = from,
                to = to
            };
            if (transitionNames.TryGetValue(tuple, out var name))
            {
                return name;
            }
            else
            {
                return GetAnyTransitionName(transition, to);
            }
        }

        public string GetAnyTransitionName(IFSMTransition transition, IFSMState to)
        {
            var tuple = new AnyTransition
            {
                transition = transition,
                to = to
            };
            return anyTransitionNames[tuple];
        }

        public bool PollEvent(out MachineEvent evt)
        {
            return eventHistory.Dequeue(out evt);
        }

        public IEnumerable<MachineEvent> GetHistory() => eventHistory.GetHistory();

        public override bool Equals(object obj)
        {
            return obj is DebugMachine machine &&
                   EqualityComparer<IDebuggableMachine>.Default.Equals(this.machine, machine.machine);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(machine);
        }

        public static bool operator ==(DebugMachine a, DebugMachine b)
        {
            return Object.ReferenceEquals(a.machine, b.machine);
        }

        public static bool operator !=(DebugMachine a, DebugMachine b)
        {
            return !Object.ReferenceEquals(a.machine, b.machine);
        }
    }
}
