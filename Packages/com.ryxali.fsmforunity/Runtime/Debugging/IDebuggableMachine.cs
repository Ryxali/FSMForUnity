namespace FSMForUnity
{
    internal interface IDebuggableMachine
    {
        string GetName();
        IFSMState GetDefaultState();
        IFSMState[] GetAllStates();
        bool TryGetActive(out IFSMState state);
        bool TryGetTransitionsFrom(IFSMState state, out TransitionMapping[] transitions);
        bool TryGetAnyTransitions(out TransitionMapping[] anyTransitions);
    }
}
