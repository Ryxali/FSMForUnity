namespace FSMForUnity
{
    /// <summary>
    /// Takes a <see cref="LambdaFSMTransition.Criteria"/> and transforms it into a transition.
    /// This exists as a handy shorthand for most transitions. It does not invoke anything on
    /// <see cref="IFSMTransition.PassThrough"/> or <see cref="IFSMTransition.Destroy"/>.
    /// </summary>
	public sealed class LambdaFSMTransition : IFSMTransition
    {
        public delegate bool Criteria();

        private readonly Criteria transitionCondition;

        /// <summary>
        /// Construct a transition, taking an expression as a parameter.
        /// If a null parameter is passed, it will default to an expression evaluating to false. Sample:
        /// <code>
        /// // a transition that's always true
        /// var myLambdaTransition = new LambdaFSMTransition(() => true);
        /// // a transition that points to a particular boolean value
        /// var myLambdaTransition = new LambdaFSMTransition(() => myBool);
        /// </code>
        /// </summary>
        /// <param name="transitionCondition">any parameterless function that returns bool</param>
        public LambdaFSMTransition(Criteria transitionCondition)
        {
            this.transitionCondition = transitionCondition != null ? transitionCondition : NullCriteria;
        }

        void IFSMTransition.PassThrough() { }

        bool IFSMTransition.ShouldTransition()
        {
            return transitionCondition();
        }

        private static bool NullCriteria() => false;
    }
}
