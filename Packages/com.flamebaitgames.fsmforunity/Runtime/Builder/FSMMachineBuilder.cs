using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace FSMForUnity
{
    /// <summary>
    /// Standard builder for FSMMachine
    /// </summary>
    internal sealed class FSMMachineBuilder : FSMMachine.IBuilder
    {
        private static readonly ProfilerMarker fsmComplete = new ProfilerMarker("FSMMachineBuilder.Complete");
        private static readonly ProfilerMarker fsmNew = new ProfilerMarker("FSMMachineBuilder.New");

        private readonly List<IFSMState> states = new List<IFSMState>();
        private readonly List<(IFSMState, IFSMTransition)> anyTransitions = new List<(IFSMState, IFSMTransition)>();
        private readonly List<(IFSMState, IFSMState, IFSMTransition)> transitions = new List<(IFSMState, IFSMState, IFSMTransition)>();
        private readonly Dictionary<IFSMState, int> stateTransitionCountBuffer = new Dictionary<IFSMState, int>(EqualityComparer_IFSMState.constant);

        private IFSMState defaultState;

        private string machineName;

        public void Begin()
        {
        }

        public IFSMState AddState(string name, IFSMState state)
        {
            states.Add(state);
            if (defaultState == null)
            {
                defaultState = state;
            }
            return state;
        }

        public IFSMTransition AddTransition(string name, IFSMTransition transition, IFSMState from, IFSMState to)
        {
            transitions.Add((from, to, transition));
            return transition;
        }


        public IFSMTransition AddAnyTransition(string name, IFSMTransition transition, IFSMState to)
        {
            anyTransitions.Add((to, transition));
            return transition;
        }

        public FSMMachine Complete(FSMMachineFlags behaviourParameters)
        {
            fsmComplete.Begin();
            if (states.Count == 0)
            {
                Debug.LogWarning("Creating a state machine without any states, adding a single empty state");
                states.Add(new EmptyFSMState());
            }
            // Map all transitions and create dictionaries
            var stateTransitionsDict = new Dictionary<IFSMState, TransitionMapping[]>(EqualityComparer_IFSMState.constant);
            // calculate size for each transition array originating from a state
            for (int i = 0; i < transitions.Count; i++)
            {
                var t = transitions[i];
                if (stateTransitionCountBuffer.TryGetValue(t.Item1, out var counter))
                {
                    stateTransitionCountBuffer[t.Item1] = counter + 1;
                }
                else
                {
                    stateTransitionCountBuffer.Add(t.Item1, 1);
                }
            }
            // Generate arrays for each set of transitions originating from a state
            for (int i = 0; i < states.Count; i++)
            {
                var state = states[i];
                if (stateTransitionCountBuffer.TryGetValue(state, out var count))
                {
                    stateTransitionsDict.Add(state, new TransitionMapping[count]);
                    stateTransitionCountBuffer[state] = 0;
                }
            }
            // Fill arrays with supplied transitions
            for (int i = 0; i < transitions.Count; i++)
            {
                var t = transitions[i];
                if (stateTransitionsDict.TryGetValue(t.Item1, out var stateTransitions))
                {
                    var count = stateTransitionCountBuffer[t.Item1];
                    stateTransitions[count] = new TransitionMapping
                    {
                        to = t.Item2,
                        transition = t.Item3
                    };
                    stateTransitionCountBuffer[t.Item1] = count + 1;
                }
            }

            // Pack any transitions into array
            var anyTransitionsArr = new TransitionMapping[anyTransitions.Count];
            for (int i = 0; i < anyTransitions.Count; i++)
            {
                var t = anyTransitions[i];
                anyTransitionsArr[i] = new TransitionMapping
                {
                    to = t.Item1,
                    transition = t.Item2
                };
            }
            fsmNew.Begin();
            // create machine
            var fsm = new FSMMachine(machineName ?? FSMConfig.DefaultFSMName,
                states: states.ToArray(),
                anyTransitions: anyTransitionsArr,
                stateTransitions: stateTransitionsDict,
                defaultState: defaultState
            );
            fsmNew.End();

            fsm.resetToDefaultStateOnEnable = behaviourParameters.HasFlag(FSMMachineFlags.ResetOnEnable);
            fsm.treatRedundantEnableAsReset = behaviourParameters.HasFlag(FSMMachineFlags.TreatRedundantEnableAsReset);
            fsm.debug = behaviourParameters.HasFlag(FSMMachineFlags.DebugMode);
            fsmComplete.End();
            return fsm;
        }

        public void SetDefaultState(IFSMState state)
        {
            defaultState = state;
        }

        public void SetDebuggingInfo(string machineName, Object associatedObject)
        {
            this.machineName = machineName;
        }
        public void Clear()
        {
            states.Clear();
            anyTransitions.Clear();
            transitions.Clear();
            stateTransitionCountBuffer.Clear();
            machineName = null;
            defaultState = null;
        }
        void FSMMachine.IBuilder.Clear() => Clear();
    }

}
