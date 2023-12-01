using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Profiling;
using UnityEngine;

namespace FSMForUnity
{

	/// <summary>
	/// A standard Finite State Machine. This object contains a number of states.
	/// Transitions map between these states and acts as the condition for transitioning between them.
	/// The machine 
	/// </summary>
	public sealed class FSMMachine : IDebuggableMachine
    {
		#region Profiling
		private static readonly ProfilerMarker debugOnlyMarker = new ProfilerMarker("Debug_Only");
        private static readonly ProfilerMarker enableMarker = new ProfilerMarker("Enable");
        private static readonly ProfilerMarker disableMarker = new ProfilerMarker("Disable");
        private static readonly ProfilerMarker updateMarker = new ProfilerMarker("Update");
        private static readonly ProfilerMarker destroyMarker = new ProfilerMarker("Destroy");

        private static readonly ProfilerMarker fsmEvaluateTransitions = new ProfilerMarker("EvaluateTransitions");
        private static readonly ProfilerMarker fsmEvaluateTransitionPass = new ProfilerMarker("EvaluateTransitionsPass");

#if DEBUG
        [FSMDebuggerHidden]
        private readonly Dictionary<IFSMState, ProfilerMarker> stateMarkers = new Dictionary<IFSMState, ProfilerMarker>(EqualityComparer_IFSMState.constant);
        [FSMDebuggerHidden]
        private readonly Dictionary<IFSMTransition, ProfilerMarker> transitionMarkers = new Dictionary<IFSMTransition, ProfilerMarker>(EqualityComparer_IFSMTransition.constant);
        [FSMDebuggerHidden]
        private readonly ProfilerMarker machineMarker;
        [FSMDebuggerHidden]
        private ProfilerMarker currentStateMarker;
#endif
		#endregion

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
        private readonly string debugName;
        [FSMDebuggerHidden]
        private readonly IFSMState[] states;
        [FSMDebuggerHidden]
        private readonly TransitionMapping[] anyTransitions;
        [FSMDebuggerHidden]
        private readonly Dictionary<IFSMState, TransitionMapping[]> stateTransitions;
        [FSMDebuggerHidden]
        private readonly IFSMState defaultState;

        private IFSMState current;
        [FSMDebuggerHidden]
        private TransitionMapping[] currentTransitions;

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
            foreach (var t in anyTransitions)
                transitionMarkers.TryAdd(t.transition, new ProfilerMarker(t.GetType().Name));
            foreach (var transitions in stateTransitions.Values)
                foreach(var t in transitions)
                    transitionMarkers.TryAdd(t.transition, new ProfilerMarker(t.GetType().Name));
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
                machineMarker.Begin();
                enableMarker.Begin();
                if (IsEnabled)
                {
#if DEBUG
                    this.TransmitEvent(StateEventType.Exit, current);
                    current.ExitProfiled(currentStateMarker);
#else
                    current.Exit();
#endif
                }
                IsEnabled = true;

                if (current == null || resetToDefaultStateOnEnable)
                {
                    current = defaultState;
                    currentTransitions = stateTransitions.TryGetValue(current, out var t) ? t : null;
                    currentStateMarker = stateMarkers[current];
                }

#if DEBUG
                this.TransmitEvent(StateEventType.Enter, current);
                current.EnterProfiled(currentStateMarker);
#else
                current.Enter();
#endif
                enableMarker.End();
                machineMarker.End();
            }
        }

        /// <summary>
        /// Will cause the machine to exit its current state
        /// </summary>
        public void Disable()
        {
            if (IsEnabled)
            {
                machineMarker.Begin();
                disableMarker.Begin();
                IsEnabled = false;
#if DEBUG
                this.TransmitEvent(StateEventType.Exit, current);
                current.ExitProfiled(currentStateMarker);
#else
                current.Exit();
#endif
                current = null;
                disableMarker.End();
                machineMarker.End();
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
                machineMarker.Begin();
                updateMarker.Begin();

                EvaluateTransitions();
#if DEBUG
                this.TransmitEvent(StateEventType.Update, current);
                current.UpdateProfiled(delta, currentStateMarker);
#else
                current.Update(delta);
#endif
                EvaluateTransitions();

                updateMarker.End();
                machineMarker.End();
            }
        }

        private void EvaluateTransitions()
        {
            fsmEvaluateTransitions.Begin();
            int c = 0;
            var movedNext = false;
            do
            {
                fsmEvaluateTransitionPass.Begin();
                c++;
                if (currentTransitions != null)
                {
                    movedNext = TransitionsMoveNext(currentTransitions);
                }
                if (!movedNext && anyTransitions != null)
                {
                    movedNext = TransitionsMoveNext(anyTransitions);
                }
                fsmEvaluateTransitionPass.End();
            } while (movedNext && c <= FSMConfig.MaxTransitionIterations);
            fsmEvaluateTransitions.End();
        }

        private bool TransitionsMoveNext(TransitionMapping[] transitions)
        {
            for (int i = 0; i < transitions.Length; i++)
            {
                var mapping = transitions[i];
#if DEBUG
                var shouldTransition = mapping.transition.ShouldTransitionProfiled(transitionMarkers[mapping.transition]);
#else
                var shouldTransition = mapping.transition.ShouldTransition();
#endif
                if (shouldTransition)
                {
#if DEBUG
                    this.TransmitEvent(StateEventType.Exit, current, mapping.transition);
                    current.ExitProfiled(currentStateMarker);
#else
                    current.Exit();
#endif
                    current = mapping.to;
                    currentTransitions = stateTransitions.TryGetValue(current, out var t) ? t : null;
#if DEBUG
                    currentStateMarker = stateMarkers[current];
#endif

#if DEBUG
                    mapping.transition.PassThroughProfiled(transitionMarkers[mapping.transition]);
#else
                    mapping.transition.PassThrough();
#endif

#if DEBUG
                    this.TransmitEvent(StateEventType.Enter, current, mapping.transition);
                    mapping.to.EnterProfiled(currentStateMarker);
#else
                    mapping.to.Enter();
#endif
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Destroy this state machine. It will cause it to clean up all states and transitions held within.
        /// </summary>
        public void Destroy()
        {
            destroyMarker.Begin();
            DebuggingLinker.Unlink(this);
            for (int i = 0; i < states.Length; i++)
            {
                var state = states[i];
                if (stateTransitions.TryGetValue(state, out var transitions))
                {
                    for (int j = 0; j < transitions.Length; j++)
                    {
#if DEBUG
                        transitions[j].transition.DestroyProfiled(transitionMarkers[transitions[j].transition]);
#else
                        transitions[j].transition.Destroy();
#endif
                    }
                }
#if DEBUG
                state.DestroyProfiled(stateMarkers[state]);
#else
                state.Destroy();
#endif
            }
            for (int i = 0; i < anyTransitions.Length; i++)
            {
#if DEBUG
                anyTransitions[i].transition.DestroyProfiled(transitionMarkers[anyTransitions[i].transition]);
#else
                anyTransitions[i].transition.Destroy();
#endif
            }
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
        /// Call Complete once you're done to recieve the finished machine.
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
			/// <param name="name"></param>
			/// <returns>The state added</returns>
			/// <param name="state"></param>
			IFSMState AddState(string name, [NotNull] IFSMState state);

			/// <summary>
			/// Add a transition that maps from a state to the desired state.
			/// The states passed must have been added to the builder via <see cref="AddState(string, IFSMState)"/>
			/// </summary>
			/// <param name="transition"></param>
			/// <param name="to"></param>
			/// <returns>The transition added</returns>
			IFSMTransition AddTransition(string name, [NotNull] IFSMTransition transition, [NotNull] IFSMState from, [NotNull] IFSMState to);

			/// <summary>
			/// Add a transition that maps from any state in the machine to the desired state.
			/// The state passed must have been added to the builder via <see cref="AddState(string, IFSMState)"/>
			/// </summary>
			/// <param name="name"></param>
			/// <param name="transition"></param>
			/// <returns>The transition added</returns>
			/// <param name="to"></param>
			IFSMTransition AddAnyTransition(string name, [NotNull] IFSMTransition transition, [NotNull] IFSMState to);

            /// <summary>
            /// Set the new default state. The state passed must already be added in the builder.
            /// </summary>
            /// <param name="state"></param>
            void SetDefaultState([NotNull] IFSMState state);

            /// <summary>
            /// Finishes building the machine. This should be your final call.
            /// </summary>
            /// <returns>The built machine</returns>
            FSMMachine Complete(FSMMachineFlags behaviourParameters = FSMMachineFlags.Default);

            internal void Clear();
        }


        public static IBuilder Build() => FSMMachineBuilderPool.Take();

#region Debugging
		string IDebuggableMachine.GetName()
        {
            return debugName;
        }

        IFSMState[] IDebuggableMachine.GetAllStates()
        {
            return states;
        }

		bool IDebuggableMachine.TryGetActive(out IFSMState state)
		{
            state = current;
            return IsEnabled;
        }

        IFSMState IDebuggableMachine.GetDefaultState()
        {
            return defaultState;
        }

        bool IDebuggableMachine.TryGetTransitionsFrom(IFSMState state, out TransitionMapping[] transitions)
		{
            return stateTransitions.TryGetValue(state, out transitions);
		}

		bool IDebuggableMachine.TryGetAnyTransitions(out TransitionMapping[] anyTransitions)
		{
            anyTransitions = this.anyTransitions;
            return true;
		}
#endregion
	}
}
