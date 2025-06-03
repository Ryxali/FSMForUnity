namespace FSMForUnity
{
    internal struct MachineEvent
    {
        public bool HasTransition => transition != null;

        public StateEventType type;
        public IFSMState state;
        public IFSMTransition transition;
        public int count;
        public int tick;
    }
}
