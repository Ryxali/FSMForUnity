namespace FSMForUnity
{
    internal struct FromToTransition
    {
        public IFSMTransition transition;
        public IFSMState from;
        public IFSMState to;
    }
}
