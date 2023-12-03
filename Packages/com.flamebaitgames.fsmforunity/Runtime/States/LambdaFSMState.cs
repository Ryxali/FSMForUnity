namespace FSMForUnity
{
	/// <summary>
	/// Usable if you want to quickly prototype states for your machine, as
	/// you can define new behaviours without needing to define a new class.
	/// It also allows you to define all state functions in a single class with
	/// a shared state, if that's a preferred workflow.
	/// </summary>
	public sealed class LambdaFSMState : IFSMState
    {
        public delegate void Enter();
        public delegate void Update(float delta);
        public delegate void Exit();


        private readonly Enter enter;
		private readonly Update update;
        private readonly Exit exit;

        public LambdaFSMState(Enter enter, Update update = null, Exit exit = null)
        {
            this.enter = enter ?? Default;
            this.update = update ?? Default;
            this.exit = exit ?? Default;
        }

        void IFSMState.Enter() => enter();

        void IFSMState.Exit() => exit();

        void IFSMState.Update(float delta) => update(delta);

		public void Destroy() {}

        private static void Default() { }
        private static void Default(float delta) { }
    }
}
