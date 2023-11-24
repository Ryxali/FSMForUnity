using System.Linq;
using Unity.Profiling;

namespace FSMForUnity
{

	/// <summary>
	/// Lay several states in parallel. This allows you to split up a single state
	/// into multiple states while still sharing the same life cycle.
	/// </summary>
	public sealed class ParallelFSMState : IFSMState
    {
        private readonly IFSMState[] states;
#if DEBUG
        /// <summary>
        /// Here we have a profiler marker for each underlying state
        /// </summary>
        [FSMDebuggerHidden]
        private readonly ProfilerMarker[] markers;
#endif

        /// <summary>
        /// combine all these states into one state, where each of the states run in parallel
        /// </summary>
        /// <param name="states"></param>
        public ParallelFSMState(params IFSMState[] states)
        {
            this.states = states;
#if DEBUG
            markers = new ProfilerMarker[states.Length];
            for(int i = 0; i < markers.Length; i++)
            {
                markers[i] = new ProfilerMarker(states[i].GetType().Name);
            }
#endif
        }

        /// <summary>
        /// Enter each state
        /// </summary>
        public void Enter()
        {
            var len = states.Length;
            for (int i = 0; i < len; i++)
            {
#if DEBUG
                var m = markers[i];
                m.Begin();
                states[i].Enter();
                m.End();
#else
                states[i].Enter();
#endif
            }
        }

        /// <summary>
        /// Exit each state
        /// </summary>
        public void Exit()
        {
            var len = states.Length;
            for (int i = 0; i < len; i++)
            {
#if DEBUG
                var m = markers[i];
                m.Begin();
                states[i].Exit();
                m.End();
#else
                states[i].Exit();
#endif
            }
        }

        /// <summary>
        /// Each state is updated
        /// </summary>
        /// <param name="delta"></param>
        public void Update(float delta)
        {
            var len = states.Length;
            for (int i = 0; i < len; i++)
            {
#if DEBUG
                var m = markers[i];
                m.Begin();
                states[i].Update(delta);
                m.End();
#else
                states[i].Update(delta);
#endif
            }
        }

        /// <summary>
        /// Each state is destroyed
        /// </summary>
        public void Destroy()
        {
            var len = states.Length;
            for (int i = 0; i < len; i++)
            {
#if DEBUG
                var m = markers[i];
                m.Begin();
                states[i].Destroy();
                m.End();
#else
                states[i].Destroy();
#endif
            }
        }
	}
}
