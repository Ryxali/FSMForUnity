namespace FSMForUnity
{
    /// <summary>
    /// Invert a transition so it's <see cref="IFSMTransition.ShouldTransition"/> returns true when false, and vice versa.
    /// It will still call <see cref="IFSMTransition.PassThrough"/> on the underlying transition.
    /// <code>
    /// var myTransition = new MyTransition();
    /// var invertedTransition = new InvertTransition(myTransition);
    /// // this shorthand exists which produces the same result
    /// var invertedTransitionShortcut = myTransition.Invert();
    /// </code>
    /// Take care as this
    /// could have unintended consequences. As an example:
    /// <code>
    /// var myTransition = new MyTransition();
    /// var invertedTransition = new InvertTransition(myTransition);
    /// /*
    /// * When we call this, myTransition.PassThrough is also called.
    /// * This effectively means they have a shared state.
    /// */
    /// invertedTransition.PassThrough();
    /// </code>
    /// </summary>
	public sealed class InvertFSMTransition : IFSMTransition
    {
        private readonly IFSMTransition transition;

        /// <summary>
        /// Create a new instance, inverting <paramref name="transition"/>
        /// </summary>
        /// <param name="transition">The transition to invert</param>
        public InvertFSMTransition(IFSMTransition transition)
        {
            this.transition = transition;
        }

        void IFSMTransition.PassThrough()
        {
            transition.PassThrough();
        }

        bool IFSMTransition.ShouldTransition()
        {
            return !transition.ShouldTransition();
        }

        void IFSMTransition.Destroy()
        {
            transition.Destroy();
        }
    }
}
