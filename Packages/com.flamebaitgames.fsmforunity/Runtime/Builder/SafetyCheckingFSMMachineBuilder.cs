using System.Collections.Generic;
using UnityEngine;

namespace FSMForUnity
{
	internal sealed class SafetyCheckingFSMMachineBuilder : FSMMachine.IBuilder
    {
        private readonly HashSet<IFSMState> addedStates = new HashSet<IFSMState>(EqualityComparer_IFSMState.constant);
        private readonly HashSet<FromToTransition> addedFromToTransitions = new HashSet<FromToTransition>(EqualityComparer_FromToTransition.constant);
        private readonly HashSet<AnyTransition> addedAnyTransitions = new HashSet<AnyTransition>(EqualityComparer_AnyTransition.constant);

        private readonly FSMMachine.IBuilder builder;

		public SafetyCheckingFSMMachineBuilder(FSMMachine.IBuilder builder)
		{
			this.builder = builder;
		}


        public IFSMState AddState(string name, IFSMState state)
		{
            if (state == null)
                throw new System.ArgumentNullException(nameof(state), "You cannot add null states to a machine. Consider adding an EmptyState instead.");
            else if (addedStates.Contains(state))
                throw new System.ArgumentException(nameof(state), "Cannot add the same state twice to a machine.");
            else
            {
                addedStates.Add(state);
                return builder.AddState(name, state);
            }
		}

		public void SetDefaultState(IFSMState state)
		{
            if(addedStates.Contains(state))
                throw new System.ArgumentException(nameof(state), "Only states added to the machine can be set as default.");
            else
                builder.SetDefaultState(state);
		}

		public IFSMTransition AddTransition(string name, IFSMTransition transition, IFSMState from, IFSMState to)
		{
            var tuple = new FromToTransition
            {
                from = from,
                to = to,
                transition = transition
            };
            if (transition == null)
                throw new System.ArgumentNullException(nameof(transition), "The added transition cannot be null.");
            else if (transition == null)
                throw new System.ArgumentNullException(nameof(from), "The from state cannot be null.");
            else if (transition == null)
                throw new System.ArgumentNullException(nameof(to), "The to state cannot be null.");
            else if (!addedStates.Contains(from))
                throw new System.ArgumentException(nameof(from), "You must add the state via AddState before adding transitions from it");
            else if (!addedStates.Contains(to))
                throw new System.ArgumentException(nameof(to), "You must add the state via AddState before adding transitions to it");
            else if(addedFromToTransitions.Contains(tuple))
                throw new System.ArgumentException("This transition has already been added connecting to the from state to the to state.");
            else
            {
                addedFromToTransitions.Add(tuple);
                return builder.AddTransition(name, transition, from, to);
            }
        }

        public IFSMTransition AddAnyTransition(string name, IFSMTransition transition, IFSMState to)
		{
            var tuple = new AnyTransition
            {
                to = to,
                transition = transition
            };
            if (transition == null)
                throw new System.ArgumentNullException(nameof(transition), "The added transition cannot be null.");
            else if (transition == null)
                throw new System.ArgumentNullException(nameof(to), "The to state cannot be null.");
            else if (!addedStates.Contains(to))
                throw new System.ArgumentException(nameof(to), "You must add the state via AddState before adding transitions to it.");
            else if(addedAnyTransitions.Contains(tuple))
                throw new System.ArgumentException("This transition has already been added connecting from any state to this state");
            else
            {
                addedAnyTransitions.Add(tuple);
                return builder.AddAnyTransition(name, transition, to);
            }
        }

        public FSMMachine Complete(FSMMachineFlags behaviourParameters = FSMMachineFlags.Default)
		{
			return builder.Complete(behaviourParameters);
		}

		public void SetDebuggingInfo(string machineName, Object associatedObject)
		{
			builder.SetDebuggingInfo(machineName, associatedObject);
		}

		void FSMMachine.IBuilder.Clear()
		{
            addedStates.Clear();
            addedFromToTransitions.Clear();
            addedAnyTransitions.Clear();
            builder.Clear();
		}
    }
}
