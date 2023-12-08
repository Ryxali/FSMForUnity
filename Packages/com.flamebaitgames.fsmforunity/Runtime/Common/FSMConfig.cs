namespace FSMForUnity
{
    internal static class FSMConfig
    {
        /// <summary>
        /// Maximum number of transitions allowed in a single
        /// update cycle for a state machine
        /// </summary>
        public const int MaxTransitionIterations = 8;

        public const string DefaultFSMName = "FSM Machine";

        public const int DebugCyclicEventBufferSize = 100;
    }
}
