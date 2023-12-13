namespace FSMForUnity.Editor
{
    internal class DebuggerFSMStateData
    {
        public DebugMachine currentlyInspecting;
        public DebugMachine wantToInspectNext;
        public IFSMState selectedState;
        public readonly EventBroadcaster eventBroadcaster = new EventBroadcaster();
    }
}
