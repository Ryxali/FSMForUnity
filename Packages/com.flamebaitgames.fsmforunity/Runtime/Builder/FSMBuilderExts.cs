namespace FSMForUnity
{
	public static class FSMBuilderExts
    {
        /// <summary>
        /// Add a state by defining it's enter/update/exit functions rather than an instance of a defined class.
        /// Implementing each of these functions are optional, allowing you to define the state by:
        /// <code>
        /// var builder = FSMMachine.Build();
		/// builder.AddLambdaState(
		///     enter: () => { Debug.Log("Enter"); },
		///	    update: (delta) => { Debug.Log($"Tick {delta}"); }
        ///	    // no exit definition required!
		///	);
        /// </code>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="enter"></param>
        /// <param name="update"></param>
        /// <param name="exit"></param>
        /// <returns></returns>
        public static IFSMState AddLambdaState(this FSMMachine.IBuilder builder, LambdaFSMState.Enter enter = null, LambdaFSMState.Update update = null, LambdaFSMState.Exit exit = null)
        {
            return builder.AddState(new LambdaFSMState(enter, update, exit));
        }

        /// <summary>
        /// Lay several states in parallel. This allows you to split up a single state
        /// into multiple states while still sharing the same life cycle.
        /// <param name="states">a collection of states to run in parallel</param>
        /// </summary>
        public static IFSMState AddParallelState(this FSMMachine.IBuilder builder, params IFSMState[] states)
        {
            return builder.AddState(new ParallelFSMState(states));
        }

        /// <summary>
        /// Converts a <see cref="FSMMachine"/> into an <see cref="IFSMState"/>.
        /// With this, you can create complex state machines with states that in-themselves have multiple different states.
        /// This allows you to do things like submenus or sequences of tasks that can be aborted as a whole.
        /// </summary>
        public static IFSMState AddSubstate(this FSMMachine.IBuilder builder, FSMMachine machine)
        {
            return builder.AddState(new SubstateFSMState(machine));
        }

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
