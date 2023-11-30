namespace FSMForUnity
{
    internal class DebuggerFSMStateData
    {
        public IDebuggableMachine currentlyInspecting;
        public IDebuggableMachine wantToInspectNext;
        public IFSMState selectedState;
    }
}
