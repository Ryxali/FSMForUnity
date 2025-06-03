namespace FSMForUnity.Editor
{
    internal interface IMachineEventListener
    {
        void OnTargetChanged(in DebugMachine machine);
        void OnStateEnter(IFSMState state, int tick);
        void OnStateEnter(IFSMState state, IFSMTransition through, int tick);
        void OnStateExit(IFSMState state, int tick);
        void OnStateExit(IFSMState state, IFSMTransition from, int tick);
        void OnStateUpdate(IFSMState state, int tick);
    }
}
