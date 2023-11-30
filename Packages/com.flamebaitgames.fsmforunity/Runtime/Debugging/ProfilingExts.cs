using Unity.Profiling;

namespace FSMForUnity
{
	internal static class ProfilingExts
    {
        private static readonly ProfilerMarker enterMarker = new ProfilerMarker("Enter");
        private static readonly ProfilerMarker updateMarker = new ProfilerMarker("Update");
        private static readonly ProfilerMarker exitMarker = new ProfilerMarker("Exit");
        private static readonly ProfilerMarker destroyMarker = new ProfilerMarker("Destroy");
        private static readonly ProfilerMarker shouldTransitionMarker = new ProfilerMarker("ShouldTransition");
        private static readonly ProfilerMarker passThroughMarker = new ProfilerMarker("PassThrough");

        public static void EnterProfiled(this IFSMState state, ProfilerMarker stateMarker)
        {
            stateMarker.Begin();
            enterMarker.Begin();
            state.Enter();
            enterMarker.End();
            stateMarker.End();
        }

        public static void ExitProfiled(this IFSMState state, ProfilerMarker stateMarker)
        {
            stateMarker.Begin();
            exitMarker.Begin();
            state.Exit();
            exitMarker.End();
            stateMarker.End();
        }

        public static void UpdateProfiled(this IFSMState state, float delta, ProfilerMarker stateMarker)
        {
            stateMarker.Begin();
            updateMarker.Begin();
            state.Update(delta);
            updateMarker.End();
            stateMarker.End();
        }

        public static void DestroyProfiled(this IFSMState state, ProfilerMarker stateMarker)
        {
            stateMarker.Begin();
            destroyMarker.Begin();
            state.Destroy();
            destroyMarker.End();
            stateMarker.End();
        }

        public static void DestroyProfiled(this IFSMTransition transition, ProfilerMarker stateMarker)
        {
            stateMarker.Begin();
            destroyMarker.Begin();
            transition.Destroy();
            destroyMarker.End();
            stateMarker.End();
        }

        public static bool ShouldTransitionProfiled(this IFSMTransition transition, ProfilerMarker transitionMarker)
        {
            transitionMarker.Begin();
            shouldTransitionMarker.Begin();
            var b = transition.ShouldTransition();
            shouldTransitionMarker.End();
            transitionMarker.End();
            return b;
        }

        public static void PassThroughProfiled(this IFSMTransition transition, ProfilerMarker transitionMarker)
        {
            transitionMarker.Begin();
            passThroughMarker.Begin();
            transition.PassThrough();
            passThroughMarker.End();
            transitionMarker.End();
        }
    }
}
