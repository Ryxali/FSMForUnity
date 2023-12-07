namespace FSMForUnity
{
	public static class FSMBuilderStates
    {
        /// <summary>
        /// <inheritdoc cref="FSMMachine.IBuilder.AddState(string, IFSMState)"/>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IFSMState AddState(this FSMMachine.IBuilder builder, IFSMState state)
        {
            return builder.AddState(null, state);
        }
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
        public static IFSMState AddLambdaState(this FSMMachine.IBuilder builder, LambdaFSMState.Enter enter = null, LambdaFSMState.Update update = null, LambdaFSMState.Exit exit = null, LambdaFSMState.Destroy destroy = null)
        {
            return builder.AddState(null, new LambdaFSMState(enter, update, exit, destroy));
        }

        /// <summary>
        /// <inheritdoc cref="AddLambdaState(FSMMachine.IBuilder, LambdaFSMState.Enter, LambdaFSMState.Update, LambdaFSMState.Exit)"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IFSMState AddLambdaState(this FSMMachine.IBuilder builder, string name, LambdaFSMState.Enter enter = null, LambdaFSMState.Update update = null, LambdaFSMState.Exit exit = null, LambdaFSMState.Destroy destroy = null)
        {
            return builder.AddState(name, new LambdaFSMState(enter, update, exit, destroy));
        }

        /// <summary>
        /// Lay several states in parallel. This allows you to split up a single state
        /// into multiple states while still sharing the same life cycle.
        /// <param name="states">a collection of states to run in parallel</param>
        /// </summary>
        public static IFSMState AddParallelState(this FSMMachine.IBuilder builder, params IFSMState[] states)
        {
            return builder.AddState(null, new ParallelFSMState(states));
        }

        /// <summary>
        /// <inheritdoc cref="AddParallelState(FSMMachine.IBuilder, IFSMState[])"/>
        /// </summary>
        public static IFSMState AddParallelState(this FSMMachine.IBuilder builder, string name, params IFSMState[] states)
        {
            return builder.AddState(name, new ParallelFSMState(states));
        }

        /// <summary>
        /// Converts a <see cref="FSMMachine"/> into an <see cref="IFSMState"/>.
        /// With this, you can create complex state machines with states that in-themselves have multiple different states.
        /// This allows you to do things like submenus or sequences of tasks that can be aborted as a whole.
        /// </summary>
        public static IFSMState AddSubstate(this FSMMachine.IBuilder builder, FSMMachine machine)
        {
            return builder.AddState(null, new SubstateFSMState(machine));
        }

        /// <summary>
        /// <inheritdoc cref="AddSubstate(FSMMachine.IBuilder, FSMMachine)"/>
        /// </summary>
        public static IFSMState AddSubstate(this FSMMachine.IBuilder builder, string name, FSMMachine machine)
        {
            return builder.AddState(name, new SubstateFSMState(machine));
        }
    }

}
