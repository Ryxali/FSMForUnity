using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Profiling;

namespace FSMForUnity
{
    /// <summary>
    /// Standard builder for FSMMachine
    /// </summary>
	internal sealed class FSMMachineBuilder : FSMMachine.IBuilder
    {
        private static readonly ProfilerMarker fsmBuild = new ProfilerMarker("FSMMachine.Build");

        private List<IFSMState> states = new List<IFSMState>();
        private List<(IFSMState, IFSMTransition)> anyTransitions = new List<(IFSMState, IFSMTransition)>();
        private List<(IFSMState, IFSMState, IFSMTransition)> transitions = new List<(IFSMState, IFSMState, IFSMTransition)>();

        private IFSMState defaultState;

        public void Begin()
        {
            fsmBuild.Begin();
        }

        public void Dispose()
        {
            states.Clear();
            anyTransitions.Clear();
            transitions.Clear();
        }

        public IFSMState AddState(IFSMState state)
        {
            if (state != null)
            {
                states.Add(state);
                if (defaultState == null)
                {
                    defaultState = state;
                }
            }
            else
            {
                Debug.LogWarning("Can't add a null state, adding an empty one instead.");
                state = new EmptyFSMState();
                states.Add(state);
            }
            return state;
        }

        public IFSMTransition AddTransition(IFSMTransition transition, IFSMState from, IFSMState to)
        {
            if (transition != null)
            {
                if (from != null)
                {
                    if (to != null)
                    {
                        transitions.Add((from, to, transition));
                    }
                    else
                    {
                        Debug.LogWarning("Malformed transition, missing 'from' and 'to' state");
                    }
                }
                else
                {
                    Debug.LogWarning("Malformed transition, missing 'from' state. Adding as 'Any' Transition");
                    AddAnyTransition(transition, to);
                }
            }
            else
            {
                Debug.LogWarning("Cannot add null transition");
            }

            return transition;
        }

        public IFSMTransition AddAnyTransition(IFSMTransition transition, IFSMState to)
        {
            if (transition != null)
            {
                if (to != null)
                {
                    anyTransitions.Add((to, transition));
                }
                else
                {
                    Debug.LogWarning("Malformed transition, missing 'to' state");
                }
            }
            else
            {
                Debug.LogWarning("Cannot add null transition");
            }
            return transition;
        }

        public FSMMachine Complete(FSMMachineFlags behaviourParameters)
        {
            if (states.Count == 0)
            {
                Debug.LogWarning("Creating a state machine without any states, adding a single empty state");
                states.Add(new EmptyFSMState());
            }
            // Map all transitions and create dictionaries
            var stateTransitions = (from v in transitions
                                    group v by v.Item1 into grp
                                    select new
                                    {
                                        key = grp.Key,
                                        value = (from g in grp select new { to = g.Item2, transition = g.Item3 })
                                    }).ToDictionary(k => k.key, v => v.value.Select(t => new TransitionMapping { to = t.to, transition = t.transition }).ToArray());
            // create machine
            var fsm = new FSMMachine(
                states: states.ToArray(),
                anyTransitions: anyTransitions.Select(t => new TransitionMapping { to = t.Item1, transition = t.Item2 }).ToArray(),
                stateTransitions: stateTransitions,
                defaultState: defaultState
            );

            fsm.resetToDefaultStateOnEnable = behaviourParameters.HasFlag(FSMMachineFlags.ResetOnEnable);
            fsm.treatRedundantEnableAsReset = behaviourParameters.HasFlag(FSMMachineFlags.TreatRedundantEnableAsReset);
            fsm.debug = behaviourParameters.HasFlag(FSMMachineFlags.DebugMode);
            fsmBuild.End();
            return fsm;
        }

		public void SetDefaultState(IFSMState state)
		{
            defaultState = state;
		}
	}

}
