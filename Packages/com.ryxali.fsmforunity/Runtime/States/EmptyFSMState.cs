namespace FSMForUnity
{
    /// <summary>
    /// An state that does nothing. Useful for when you want a StateMachine to idle when it's in a particular state.
    /// </summary>
    public sealed class EmptyFSMState : IFSMState
    {

        public void Enter()
        {
        }

        public void Exit()
        {
        }

        public void Update(float delta)
        {
        }
    }
}
