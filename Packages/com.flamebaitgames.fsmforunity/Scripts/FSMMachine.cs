using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace FSMForUnity
{
	/// <summary>
	/// A standard Finite State Machine. This object contains a number of states.
	/// Transitions map between these states and acts as the condition for transitioning between them.
	/// The machine 
	/// </summary>
	public sealed class FSMMachine
    {
        private static readonly ProfilerMarker fsmEnter = new ProfilerMarker("FSMState.Enter");
        private static readonly ProfilerMarker fsmUpdate = new ProfilerMarker("FSMState.Update");
        private static readonly ProfilerMarker fsmExit = new ProfilerMarker("FSMState.Exit");
        private static readonly ProfilerMarker fsmDestroy = new ProfilerMarker("FSMState.Destroy");
        private static readonly ProfilerMarker fsmMachineUpdate = new ProfilerMarker("FSMMachine.Update");
        private static readonly ProfilerMarker fsmEvaluateTransitions = new ProfilerMarker("FSMMachine.EvaluateTransitions");
        private static readonly ProfilerMarker fsmEvaluateTransitionPass = new ProfilerMarker("FSMMachine.EvaluateTransitionsPass");

        /// <summary>
        /// Is the State Machine currently in its enabled state?
        /// </summary>
        public bool IsEnabled { get; private set; } = false;

        /// <summary>
        /// When the State Machine is enabled, revert back to the default state.
        /// </summary>
        public bool resetToDefaultStateOnEnable;
        /// <summary>
        /// If Enable is called on the State Machine while the machine is enabled,
        /// treat that as though the machine was disabled, then re-enabled.
        /// </summary>
        public bool treatRedundantEnableAsReset;
        /// <summary>
        /// When on, the machine will automatically output what state it is in into
        /// the console whenever any transition in the machine occur.
        /// </summary>
        public bool debug = false;

        private readonly IFSMState[] states;
        private readonly TransitionMapping[] anyTransitions;
        private readonly IReadOnlyDictionary<IFSMState, TransitionMapping[]> stateTransitions;

#if DEBUG
        private readonly Dictionary<IFSMState, ProfilerMarker> stateMarkers = new Dictionary<IFSMState, ProfilerMarker>();
#endif
        private IFSMState current;
        private readonly IFSMState defaultState;

        internal FSMMachine(IFSMState[] states, TransitionMapping[] anyTransitions, IReadOnlyDictionary<IFSMState, TransitionMapping[]> stateTransitions, IFSMState defaultState)
        {
            this.states = states;
            this.anyTransitions = anyTransitions;
            this.stateTransitions = stateTransitions;
            this.defaultState = defaultState;
#if DEBUG
            foreach (var state in states)
            {
                stateMarkers.Add(state, new ProfilerMarker(state.GetType().Name));
            }
#endif
        }

        /// <summary>
        /// Enable the state machine. This will cause it to Enter the default state,
        /// or if <see cref="resetToDefaultStateOnEnable"/> is false, the last state it was in
        /// </summary>
        public void Enable()
        {
            if (!IsEnabled || treatRedundantEnableAsReset)
            {
                if (IsEnabled)
                {
                    fsmExit.Begin();
#if DEBUG
                    stateMarkers[current].Begin();
#endif
                    current.Exit();
#if DEBUG
                    stateMarkers[current].End();
#endif
                    fsmExit.End();
                }
                IsEnabled = true;
                if (current == null || resetToDefaultStateOnEnable)
                {
                    current = defaultState;
                }
                if (debug)
                    Debug.Log($"Enable Enter: {DumpState()}");

                fsmEnter.Begin();
#if DEBUG
                stateMarkers[current].Begin();
#endif
                current.Enter();
#if DEBUG
                stateMarkers[current].End();
#endif
                fsmEnter.End();
            }
        }

        /// <summary>
        /// Will cause the machine to exit its current state
        /// </summary>
        public void Disable()
        {
            if (IsEnabled)
            {
                IsEnabled = false;
                if (debug)
                    Debug.Log($"Disable Exit: {DumpState()}");
                fsmExit.Begin();
#if DEBUG
                stateMarkers[current].Begin();
#endif
                current.Exit();
#if DEBUG
                stateMarkers[current].End();
#endif
                fsmExit.End();
            }
        }

        /// <summary>
        /// Update the machine. It will evaluate all transitions and pass through them if appropriate.
        /// It will continue evaluating transitions, moving between states, until all transitions evaluate
        /// as false or the <see cref="FSMConfig.MaxTransitionIterations"/> has been reached.
        /// After transitioning, it will call <see cref="IFSMState.Update(float)"/> on the active state.
        /// </summary>
        /// <param name="delta"></param>
        public void Update(float delta)
        {
            if (IsEnabled)
            {
                fsmMachineUpdate.Begin();
                EvaluateTransitions();
                fsmUpdate.Begin();
#if DEBUG
                stateMarkers[current].Begin();
#endif
                current.Update(delta);
#if DEBUG
                stateMarkers[current].End();
#endif
                fsmUpdate.End();
                EvaluateTransitions();
                fsmMachineUpdate.End();
            }
        }

        private void EvaluateTransitions()
        {
            fsmEvaluateTransitions.Begin();
            int c = 0;
            while (c < FSMConfig.MaxTransitionIterations && EvaluateTransitionPass())
            {
                c++;
            }
            fsmEvaluateTransitions.End();
        }

        private bool EvaluateTransitionPass()
        {
            fsmEvaluateTransitionPass.Begin();
            if (stateTransitions.TryGetValue(current, out var transitions))
            {
                for (int i = 0; i < transitions.Length; i++)
                {
                    var transition = transitions[i];
                    if (transition.transition.ShouldTransition())
                    {
                        fsmExit.Begin();
#if DEBUG
                        stateMarkers[current].Begin();
#endif
                        current.Exit();
#if DEBUG
                        stateMarkers[current].End();
#endif
                        fsmExit.End();
                        current = transition.to;
                        transition.transition.PassThrough();
                        if (debug)
                            Debug.Log($"Transition To: {DumpState()}");
                        fsmEnter.Begin();
#if DEBUG
                        stateMarkers[current].Begin();
#endif
                        current.Enter();
#if DEBUG
                        stateMarkers[current].End();
#endif
                        fsmEnter.End();
                        fsmEvaluateTransitionPass.End();
                        return true;
                    }
                }
            }
            for (int i = 0; i < anyTransitions.Length; i++)
            {
                var transition = anyTransitions[i];
                if (transition.to != current && transition.transition.ShouldTransition())
                {
                    fsmExit.Begin();
#if DEBUG
                    stateMarkers[current].Begin();
#endif
                    current.Exit();
#if DEBUG
                    stateMarkers[current].End();
#endif
                    fsmExit.End();
                    current = transition.to;
                    fsmEnter.Begin();
#if DEBUG
                    stateMarkers[current].Begin();
#endif
                    current.Enter();
#if DEBUG
                    stateMarkers[current].End();
#endif
                    fsmEnter.End();
                    fsmEvaluateTransitionPass.End();
                    return true;
                }
            }
            fsmEvaluateTransitionPass.End();
            return false;
        }

        /// <summary>
        /// Destroy this state machine. It will cause it to clean up all states and transitions held within.
        /// </summary>
        public void Destroy()
        {
            fsmDestroy.Begin();
            for (int i = 0; i < states.Length; i++)
            {
                var state = states[i];
                if (stateTransitions.TryGetValue(state, out var transitions))
                {
                    for (int j = 0; j < transitions.Length; j++)
                    {
                        transitions[j].transition.Destroy();
                    }
                }
                state.Destroy();
            }
            for (int i = 0; i < anyTransitions.Length; i++)
            {
                anyTransitions[i].transition.Destroy();
            }
            fsmDestroy.End();
        }

        /// <summary>
        /// Output the current state as a debug readable string
        /// </summary>
        /// <returns></returns>
        public string DumpState()
        {
            return current != null ? current.ToString() : "None";
        }

        /// <summary>
        /// Exposes functions to call to build the FSMMachine.
        /// Call Complete once you're done to recieve the finished machine.1
        /// </summary>
        public interface IBuilder
        {
            /// <summary>
            /// Typically the first state added becomes the default state for the machine.
            /// Call <see cref="SetDefaultState(IFSMState)"/> to change this
            /// </summary>
            /// <param name="state"></param>
            /// <returns>The state added</returns>
            IFSMState AddState(IFSMState state);
            /// <summary>
            /// Add a transition that maps from a state to the desired state.
            /// The states passed must have been added to the builder via <see cref="AddState(IFSMState)"/>
            /// </summary>
            /// <param name="transition"></param>
            /// <param name="to"></param>
            /// <returns>The transition added</returns>
            IFSMTransition AddTransition(IFSMTransition transition, IFSMState from, IFSMState to);
            /// <summary>
            /// Add a transition that maps from any state in the machine to the desired state.
            /// The state passed must have been added to the builder via <see cref="AddState(IFSMState)"/>
            /// </summary>
            /// <param name="transition"></param>
            /// <param name="to"></param>
            /// <returns>The transition added</returns>
            IFSMTransition AddAnyTransition(IFSMTransition transition, IFSMState to);
            /// <summary>
            /// Set the new default state. The state passed must already be added in the builder.
            /// </summary>
            /// <param name="state"></param>
            void SetDefaultState(IFSMState state);
            /// <summary>
            /// Finishes building the machine. This should be your final call.
            /// </summary>
            /// <returns>The built machine</returns>
            FSMMachine Complete(FSMMachineFlags behaviourParameters = FSMMachineFlags.Default);
        }


        public static IBuilder Build() => FSMMachineBuilderPool.Take();
    }
}
