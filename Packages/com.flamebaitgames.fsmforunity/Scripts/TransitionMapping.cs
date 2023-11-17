namespace FSMForUnity
{
    /// <summary>
    /// Used internally in the FSMMachine
    /// </summary>
    internal struct TransitionMapping
    {
        public IFSMTransition transition;
        public IFSMState to;
    }
}
