using UnityEngine;

namespace FSMForUnity
{
    /// <summary>
    /// A transition that can only be passed through once it has been triggered via
    /// <see cref="TriggeredFSMTransition.Trigger"/>. Once the transition has been passed through,
    /// it must be triggered again to allow subsequent passthroughs.
    /// </summary>
    [System.Serializable]
	public sealed class TriggeredFSMTransition : IFSMTransition
    {
        private bool isSet;

        /// <summary>
        /// Open the path to go through this transition once.
        /// </summary>
        public void Trigger()
        {
            isSet = true;
        }

        public void PassThrough()
        {
            isSet = false;
        }

        public bool ShouldTransition()
        {
            return isSet;
        }
    }
}
