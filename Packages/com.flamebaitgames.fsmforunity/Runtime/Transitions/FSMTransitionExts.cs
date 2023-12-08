namespace FSMForUnity
{
    public static class FSMTransitionExts
    {
        /// <summary>
        /// Create a <see cref="InvertFSMTransition"/> using the supplied transition.
        /// The ShouldTransition evaluation becomes inverted, and is useful if two
        /// states should mirror a binary value.
        /// </summary>
        /// <param name="transition"></param>
        /// <returns></returns>
        public static IFSMTransition Invert(this IFSMTransition transition)
        {
            return new InvertFSMTransition(transition);
        }
    }
}
