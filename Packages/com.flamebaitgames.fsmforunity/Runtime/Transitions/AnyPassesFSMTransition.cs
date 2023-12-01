namespace FSMForUnity
{
	/// <summary>
	/// A composite transition. We can only pass through this transition if any transition
	/// in the composite returns <see cref="FSMForUnity.IFSMTransition.ShouldTransition"/> as true.
	/// </summary>
	public sealed class AnyPassesFSMTransition : IFSMTransition
	{
		private readonly IFSMTransition[] transitions;

		public AnyPassesFSMTransition(params IFSMTransition[] transitions)
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
			// return true if any of the transitions are true
			bool anyWantTransition = false;
			var transitionsLength = transitions.Length;
			for (int i = 0; !anyWantTransition && i < transitionsLength; i++)
			{
				anyWantTransition |= transitions[i].ShouldTransition();
			}
			return anyWantTransition;
		}

		void IFSMTransition.Destroy()
		{
			for (int i = 0; i < transitions.Length; i++) transitions[i].Destroy();
		}
	}
}
