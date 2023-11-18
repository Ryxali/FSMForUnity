namespace FSMForUnity
{
    /// <summary>
    /// Converts a <see cref="FSMMachine"/> into an <see cref="IFSMState"/>.
    /// With this, you can create complex state machines with states that in-themselves have multiple different states.
    /// This allows you to do things like submenus or sequences of tasks that can be aborted as a whole.
    /// </summary>
	public sealed class SubstateFSMState : IFSMState
    {
        private readonly FSMMachine stateMachine;

        public SubstateFSMState(FSMMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public void Enter()
        {
            stateMachine.Enable();
        }

        public void Exit()
        {
            stateMachine.Disable();
        }

        public void Update(float delta)
        {
            stateMachine.Update(delta);
        }

        public void Destroy()
        {
            stateMachine.Destroy();
        }

        public override string ToString()
        {
            return $"({stateMachine.DumpState()})";
        }
    }
}
