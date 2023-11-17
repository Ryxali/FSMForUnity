namespace FSMForUnity
{
	/// <summary>
	/// A transition that is always true. Usable when you quickly want to move from one state to another.
	/// The object is immutable, so only a single AlwaysTransition.constant is available.
	/// <code>
	/// // Given a machine with two sample states
	/// FSMMachine.Builder builder = FSMMachine.Build();
	/// var stateA = builder.AddState(new EmptyFSMState());
	/// var stateB = builder.AddState(new EmptyFSMState());
	/// // When in stateA, we will always transition to stateB
	/// var transition = builder.AddTransition(AlwaysTransition.constant, stateA, stateB);
	/// </code>
	/// </summary>
	public sealed class AlwaysTransition : IFSMTransition
	{
		public static readonly AlwaysTransition constant = new AlwaysTransition();

		private AlwaysTransition() { }

		void IFSMTransition.PassThrough() {}

		bool IFSMTransition.ShouldTransition()
		{
			return true;
		}
	}
}
