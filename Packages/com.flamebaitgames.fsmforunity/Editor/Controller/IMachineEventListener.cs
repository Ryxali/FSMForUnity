namespace FSMForUnity
{
    internal interface IMachineEventListener
    {
        void OnTargetChanged(in DebugMachine machine);
        void OnStateEnter(IFSMState state);
        void OnStateEnter(IFSMState state, IFSMTransition through);
        void OnStateExit(IFSMState state);
        void OnStateExit(IFSMState state, IFSMTransition from);
        void OnStateUpdate(IFSMState state);
    }
}
