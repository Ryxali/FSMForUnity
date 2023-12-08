namespace FSMForUnity
{
    /// <summary>
    /// A transition that can only be passed through once it has been triggered via
    /// <see cref="TriggeredFSMTransition.Trigger"/>. Once the transition has been passed through,
    /// it must be triggered again to allow subsequent passthroughs.
    /// </summary>
	public sealed class TriggeredFSMTransition : IFSMTransition
    {
        private bool isSet;

        public void Trigger()
        {
            isSet = true;
        }

        void IFSMTransition.PassThrough()
        {
            isSet = false;
        }

        bool IFSMTransition.ShouldTransition()
        {
            return isSet;
        }

        private static bool NullCriteria() => false;
    }
}
