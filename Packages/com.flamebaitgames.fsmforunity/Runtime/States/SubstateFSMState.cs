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

        /// <summary>
        /// Create a state containing the supplied state machine.
        /// <para>
        /// This state will assume ownership of the supplied state machine.
        /// Manipulating the state machine by calling its Enable/Disable/Update/Destroy methods
        /// will result in undefined behaviour.
        /// </para>
        /// </summary>

        public SubstateFSMState(FSMMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        /// <summary>
        /// When we enter this state the underlying machine becomes enabled.
        /// </summary>
        public void Enter()
        {
            stateMachine.Enable();
        }

        /// <summary>
        /// When we exit this state the underlying machine becomes disabled.
        /// </summary>
        public void Exit()
        {
            stateMachine.Disable();
        }

        /// <summary>
        /// Each update call is passed through to the underlying machine.
        /// </summary>
        public void Update(float delta)
        {
            stateMachine.Update(delta);
        }

        /// <summary>
        /// Will destroy the underlying machine.
        /// </summary>
        public void Destroy()
        {
            stateMachine.Destroy();
        }
    }
}
