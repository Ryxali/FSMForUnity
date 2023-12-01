namespace FSMForUnity
{
	/// <summary>
	/// A composite transition. We can only pass through this transition if all transitions
	/// in the composite returns <see cref="FSMForUnity.IFSMTransition.ShouldTransition"/> as true.
	/// </summary>
	public sealed class AllPassesFSMTransition : IFSMTransition
	{
		private readonly IFSMTransition[] transitions;
		public AllPassesFSMTransition(params IFSMTransition[] transitions)
		{
			this.transitions = transitions;
		}

		void IFSMTransition.PassThrough()
		{
			// Pass through all transitions in this composite
			for (int i = 0; i < transitions.Length; i++) transitions[i].PassThrough();
		}
		bool IFSMTransition.ShouldTransition()
		{
			// Iterate through all transitions and return false if any of them are false
			bool allWantTransition = true;
			var transitionsLength = transitions.Length;
			for (int i = 0; allWantTransition && i < transitionsLength; i++)
			{
				// becomes false if any transition is false
				allWantTransition = transitions[i].ShouldTransition();
			}
			return allWantTransition;
		}

		void IFSMTransition.Destroy()
		{
			for (int i = 0; i < transitions.Length; i++) transitions[i].Destroy();
		}
	}
}
