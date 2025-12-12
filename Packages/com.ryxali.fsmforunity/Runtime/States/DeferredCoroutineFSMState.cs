using System.Collections;
namespace FSMForUnity
{
    /// <summary>
    /// A state that executes the assigned coroutine method. The coroutine cannot be a lambda method (e.g. '() => ...').
    /// See <see cref="CoroutineFSMState"/> for more about coroutine states.
    /// </summary>
    public class DeferredCoroutineFSMState : CoroutineFSMState
    {
        private readonly System.Func<IEnumerator> enterNoDelta;
        private readonly System.Func<DeltaTime, IEnumerator> enter;
        private readonly System.Action exit;

        /// <summary>
        /// Construct the state, disregarding access to the state machine's <see cref="CoroutineFSMState.DeltaTime"/>.
        /// </summary>
        /// <param name="enter"></param>
        /// <param name="exit"></param>
        public DeferredCoroutineFSMState(System.Func<IEnumerator> enter, System.Action exit)
        {
            if (enter != null)
                enterNoDelta = enter;
            else
                enterNoDelta = Empty;

            this.enter = EnterNoDelta;

            if (exit != null)
                this.exit = exit;
            else
                exit = NoExit;
        }

        /// <summary>
        /// Construct the state, targeting a coroutine with <see cref="CoroutineFSMState.DeltaTime"/> as a parameter.
        /// </summary>
        /// <param name="enter"></param>
        /// <param name="exit"></param>
        public DeferredCoroutineFSMState(System.Func<DeltaTime, IEnumerator> enter, System.Action exit)
        {
            if (enter != null)
            {
                this.enter = enter;
            }
            else
            {
                this.enter = EnterNoDelta;
                enterNoDelta = Empty;
            }

            if (exit != null)
                this.exit = exit;
            else
                exit = NoExit;
        }

        protected override IEnumerator Enter(DeltaTime deltaTime)
        {
            return enter.Invoke(deltaTime);
        }

        private IEnumerator EnterNoDelta(DeltaTime deltaTime)
        {
            return enterNoDelta();
        }

        private IEnumerator Empty()
        {
            return null;
        }

        protected override void Exit()
        {
            exit?.Invoke();
        }

        private void NoExit() { }


    }
}
