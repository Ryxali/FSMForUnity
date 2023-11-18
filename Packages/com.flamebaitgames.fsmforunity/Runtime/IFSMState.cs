namespace FSMForUnity
{
    /// <summary>
    /// Base interface for all states. It encapsulates the entire lifecycle for a state:
    /// <list type="bullet">
    /// <item>Enter</item>
    /// <item>Exit</item>
    /// <item>Update</item>
    /// <item>Destroy</item>
    /// </list>
    /// General lifecycle of a state is as follows and is controlled entirely by the <see cref="FSMMachine"/>
    /// <code>
    /// IFSMState myState;
    /// ..
    /// myState.Enter();
    /// myState.Update(dt);
    /// myState.Update(dt)
    /// ..
    /// myState.Exit();
    /// ..
    /// myState.Destroy();
    /// </code>
    /// Note that some <see cref="IFSMState"/> implementations currently implement the <see cref="System.Object.ToString"/>
    /// operation. This is for debugging purposes, and you can override it in your implementations as well if you want
    /// any additional information when debugging.
    /// </summary>
    public interface IFSMState
    {
        /// <summary>
        /// This is always called once as the state is entered. It will always be called before any <see cref="IFSMState.Update(float)"/> calls.
        /// </summary>
        void Enter();
        /// <summary>
        /// This is always called once as the state is exited. The state has always been entered before exiting, and will be called before <see cref="IFSMState.Destroy"/> is called.
        /// </summary>
        void Exit();
        /// <summary>
        /// This is called by the State Machine every time it's updated as long as this state has been entered.
        /// delta will in general be tied to Time.deltaTime or similar, though that is up to the behaviour updating the State Machine.
        /// </summary>
        /// <param name="delta">How much time has elapsed since the last State Machine update.</param>
        void Update(float delta);
        /// <summary>
        /// Called as the State Machine is destroyed. Dispose of any managed objects here.
        /// </summary>
        void Destroy();
    }
}
