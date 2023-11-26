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
        private static readonly ProfilerMarker debugOnlyMarker = new ProfilerMarker("Debug_Only");
        private static readonly ProfilerMarker enableMarker = new ProfilerMarker("Enable");
        private static readonly ProfilerMarker disableMarker = new ProfilerMarker("Disable");
        private static readonly ProfilerMarker enterMarker = new ProfilerMarker("Enter");
        private static readonly ProfilerMarker updateMarker = new ProfilerMarker("Update");
        private static readonly ProfilerMarker exitMarker = new ProfilerMarker("Exit");
        private static readonly ProfilerMarker destroyMarker = new ProfilerMarker("Destroy");
        private static readonly ProfilerMarker fsmEvaluateTransitions = new ProfilerMarker("EvaluateTransitions");
        private static readonly ProfilerMarker fsmEvaluateTransitionPass = new ProfilerMarker("EvaluateTransitionsPass");

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

        [FSMDebuggerHidden]
        internal readonly string debugName;
        [FSMDebuggerHidden]
        internal IFSMState Debug_CurrentState => current;
#if DEBUG
        [FSMDebuggerHidden]
        private readonly Dictionary<IFSMState, ProfilerMarker> stateMarkers = new Dictionary<IFSMState, ProfilerMarker>(EqualityComparer_IFSMState.constant);
        [FSMDebuggerHidden]
        private readonly ProfilerMarker machineMarker;
#endif
        [FSMDebuggerHidden]
        internal readonly IFSMState[] states;
        [FSMDebuggerHidden]
        internal readonly TransitionMapping[] anyTransitions;
        [FSMDebuggerHidden]
        internal readonly Dictionary<IFSMState, TransitionMapping[]> stateTransitions;
        [FSMDebuggerHidden]
        internal readonly IFSMState defaultState;

        private IFSMState current;
        [FSMDebuggerHidden]
        private TransitionMapping[] currentTransitions;
#if DEBUG
        [FSMDebuggerHidden]
        private ProfilerMarker currentStateMarker;
        [FSMDebuggerHidden]
        internal IFSMState DebugCurrent => current;
#endif

        internal FSMMachine(string debugName, IFSMState[] states, TransitionMapping[] anyTransitions, Dictionary<IFSMState, TransitionMapping[]> stateTransitions, IFSMState defaultState)
        {
            this.states = states;
            this.anyTransitions = anyTransitions;
            this.stateTransitions = stateTransitions;
            this.defaultState = defaultState;
#if DEBUG
            debugOnlyMarker.Begin();
            this.debugName = debugName;
            machineMarker = new ProfilerMarker(debugName);
            foreach (var state in states)
            {
                stateMarkers.Add(state, new ProfilerMarker(state.GetType().Name));
            }
            debugOnlyMarker.End();
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
#if DEBUG
                enableMarker.Begin();
                {
                    if (IsEnabled)
                    {
                        currentStateMarker.Begin();
                        {
                            exitMarker.Begin();
                            current.Exit();
                            exitMarker.End();
                        }
                        currentStateMarker.End();
                    }
                    IsEnabled = true;

                    if (current == null || resetToDefaultStateOnEnable)
                    {
                        current = defaultState;
                        currentTransitions = stateTransitions.TryGetValue(current, out var t) ? t : null;
                        currentStateMarker = stateMarkers[current];
                    }

                    if (debug)
                        Debug.Log($"Enable Enter: {DumpState()}");

                    currentStateMarker.Begin();
                    {
                        enterMarker.Begin();
                        current.Enter();
                        enterMarker.End();
                    }
                    currentStateMarker.End();
                }
                enableMarker.End();
#else
                if (IsEnabled)
                {
                    current.Exit();
                }
                IsEnabled = true;

                if (current == null || resetToDefaultStateOnEnable)
                {
                    current = defaultState;
                }

                if (debug)
                    Debug.Log($"Enable Enter: {DumpState()}");
                
                current.Enter();
#endif
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
#if DEBUG
                disableMarker.Begin();
                {
                    currentStateMarker.Begin();
                    {
                        exitMarker.Begin();
                        current.Exit();
                        exitMarker.End();
                    }
                    currentStateMarker.End();
                }
                disableMarker.End();

#else
                current.Exit();
#endif
            }
        }

        /// <summary>
        /// Update the machine. It will evaluate all transitions and pass through them if appropriate.
        /// It will continue evaluating transitions, moving between states, until all transitions evaluate
        /// as false or the <see cref="FSMConfig.MaxTransitionIterations"/> has been reached.
        /// After transitioning, it will call <see cref="IFSMState.Update(float)"/> on the active state.
        /// </summary>
        /// <param name="delta">Time that has passed since the last Update call. This can be simply Time.deltaTime in most use cases.</param>
        public void Update(float delta)
        {
            if (IsEnabled)
            {
#if DEBUG
                machineMarker.Begin();
                {
                    updateMarker.Begin();
                    {
                        EvaluateTransitions();
                        currentStateMarker.Begin();
                        {
                            updateMarker.Begin();
                            current.Update(delta);
                            updateMarker.End();
                        }
                        currentStateMarker.End();
                        EvaluateTransitions();
                    }
                    updateMarker.End();
                }
                machineMarker.End();
#else
                EvaluateTransitions();
                current.Update(delta);
                EvaluateTransitions();
#endif
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
            if (currentTransitions != null)
            {
                for (int i = 0; i < currentTransitions.Length; i++)
                {
                    var transition = currentTransitions[i];
                    if (transition.transition.ShouldTransition())
                    {
                        exitMarker.Begin();
#if DEBUG
                        currentStateMarker.Begin();
#endif
                        current.Exit();
#if DEBUG
                        currentStateMarker.End();
#endif
                        exitMarker.End();
                        current = transition.to;
                        currentTransitions = stateTransitions.TryGetValue(current, out var t) ? t : null;
#if DEBUG
                        currentStateMarker = stateMarkers[current];
#endif
                        transition.transition.PassThrough();
                        if (debug)
                            Debug.Log($"Transition To: {DumpState()}");
                        enterMarker.Begin();
#if DEBUG
                        currentStateMarker.Begin();
#endif
                        current.Enter();
#if DEBUG
                        currentStateMarker.End();
#endif
                        enterMarker.End();
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
                    exitMarker.Begin();
#if DEBUG
                    currentStateMarker.Begin();
#endif
                    current.Exit();
#if DEBUG
                    currentStateMarker.End();
#endif
                    exitMarker.End();
                    current = transition.to;
                    currentTransitions = stateTransitions.TryGetValue(current, out var t) ? t : null;
#if DEBUG
                    currentStateMarker = stateMarkers[current];
#endif
                    enterMarker.Begin();
#if DEBUG
                    currentStateMarker.Begin();
#endif
                    current.Enter();
#if DEBUG
                    currentStateMarker.End();
#endif
                    enterMarker.End();
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
            destroyMarker.Begin();
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
#if DEBUG
                stateMarkers[state].Begin();
                destroyMarker.Begin();
                state.Destroy();
                destroyMarker.End();
                stateMarkers[state].End();
#else
                state.Destroy();
#endif
            }
            for (int i = 0; i < anyTransitions.Length; i++)
            {
                anyTransitions[i].transition.Destroy();
            }
            DebuggingLinker.RemoveAllReferences(this);
            destroyMarker.End();
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
            /// Set debugging info. Useful only when playing in editor or development mode.
            /// </summary>
            /// <param name="machineName">The name you'd like this machine to have in profiler and debugger.</param>
            /// <param name="associatedObject">An associated owner of this machine. If you select this object the
            /// machine will automatically be presented in the inspector.</param>
            void SetDebuggingInfo(string machineName, Object associatedObject);
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
