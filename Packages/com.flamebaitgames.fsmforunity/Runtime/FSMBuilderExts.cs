namespace FSMForUnity
{
	public static class FSMBuilderExts
    {

        /// <summary>
        /// Add a transition with using a transition expression that maps from a state to the desired state
        /// The state passed must have been added to the builder via <see cref="AddState(IFSMState)"/>
        /// <code>
        /// FSMMachine.IBuilder builder;
        /// var stateA = builder.AddState(new EmptyFSMState());
        /// var stateB = builder.AddState(new EmptyFSMState());
        /// // Adds effectively an Always transition from stateA to stateB
        /// builder.AddTransition(() => true, stateA, stateB);
        /// // Transition from stateA to stateB if at any point myBool is true
        /// builder.AddTransition(() => myBool, stateA, stateB);
        /// </code>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="criteria"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static IFSMTransition AddTransition(this FSMMachine.IBuilder builder, LambdaFSMTransition.Criteria criteria, IFSMState from, IFSMState to)
        {
            return builder.AddTransition(new LambdaFSMTransition(criteria), from, to);
        }
        /// <summary>
        /// Add a transition with using a transition expression that maps from any state to the desired state
        /// The state passed must have been added to the builder via <see cref="AddState(IFSMState)"/>
        /// <code>
        /// FSMMachine.IBuilder builder;
        /// var stateA = builder.AddState(new EmptyFSMState());
        /// var stateB = builder.AddState(new EmptyFSMState());
        /// // Adds effectively an Always transition to stateB
        /// builder.AddAnyTransition(() => true, stateB);
        /// // Transition to stateB if at any point myBool is true
        /// builder.AddAnyTransition(() => myBool, stateB);
        /// </code>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="criteria"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static IFSMTransition AddAnyTransition(this FSMMachine.IBuilder builder, LambdaFSMTransition.Criteria criteria, IFSMState to)
        {
            return builder.AddAnyTransition(new LambdaFSMTransition(criteria), to);
        }

        /// <summary>
        /// Add a transition from stateA to stateB, and also add an inverted transition from stateB to stateA
        /// The state passed must have been added to the builder via <see cref="AddState(IFSMState)"/>
        /// /// <code>
        /// FSMMachine.IBuilder builder;
        /// var stateA = builder.AddState(new EmptyFSMState());
        /// var stateB = builder.AddState(new EmptyFSMState());
        /// // Transition from stateA to stateB whenever myBool is true
        /// // Transition from stateB to stateA whenever myBool is false
        /// builder.AddBidirectionalTransition(() => myBool, stateB);
        /// </code>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="criteria"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static void AddBidirectionalTransition(this FSMMachine.IBuilder builder, LambdaFSMTransition.Criteria criteria, IFSMState from, IFSMState to)
        {
            var transition = new LambdaFSMTransition(criteria);
            builder.AddTransition(transition, from, to);
            builder.AddTransition(transition.Invert(), to, from);
        }

        /// <summary>
        /// Add a transition from stateA to stateB, and also add an inverted transition from stateB to stateA
        /// The state passed must have been added to the builder via <see cref="AddState(IFSMState)"/>
        /// <code>
        /// FSMMachine.IBuilder builder;
        /// var stateA = builder.AddState(new EmptyFSMState());
        /// var stateB = builder.AddState(new EmptyFSMState());
        /// // Transition from stateA to stateB whenever MyTransition.ShouldTransition evaluates to true
        /// // Transition from stateB to stateA whenever MyTransition.ShouldTransition evaluates to false
        /// builder.AddBidirectionalTransition(new MyTransition(), stateB);
        /// </code>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="transition"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static void AddBidirectionalTransition(this FSMMachine.IBuilder builder, IFSMTransition transition, IFSMState from, IFSMState to)
        {
            builder.AddTransition(transition, from, to);
            builder.AddTransition(transition.Invert(), to, from);
        }
    }

}
