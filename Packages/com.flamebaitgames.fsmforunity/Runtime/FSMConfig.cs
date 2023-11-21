using UnityEngine;

namespace FSMForUnity
{
    [System.Serializable]
	internal static class FSMConfig
    {
        /// <summary>
        /// Maximum number of transitions allowed in a single
        /// update cycle for a state machine
        /// </summary>
        public const int MaxTransitionIterations = 8;

        public static int test = MaxTransitionIterations;
    }


    internal class FSMConfigAsset : ScriptableObject
    {
        public int test;

#if UNITY_EDITOR
		private void OnEnable()
		{
            FSMConfig.test = test;
		}
#endif
	}
}
