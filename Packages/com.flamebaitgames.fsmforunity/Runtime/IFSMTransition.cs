
namespace FSMForUnity
{
    /// <summary>
    /// Base interface for all transitions. It consists of three parts:
    /// <list type="bullet">
    /// <item>An evaluation critera method.</item>
    /// <item>A method that is invoked as the transition is passed through.</item>
    /// <item>A method that is invoked as the State Machine is destroyed to allow any cleanup.</item>
    /// </list>
    /// All these methods are automatically used by the State Machine itself as part of its lifecycle.
    /// 
    /// Only an implementation of these methods are required. See <see cref="FSMForUnity.AlwaysFSMTransition"/>
    /// or <see cref="FSMForUnity.LambdaFSMTransition"/> for an example implementation.
    /// </summary>
    public interface IFSMTransition
    {
        /// <summary>
        /// Are we currenly eligble to pass through this transition?
        /// </summary>
        /// <returns>true if this transition wants the transition to happen</returns>
        protected internal bool ShouldTransition();
        /// <summary>
        /// This is called from the State Machine as we move via this transition from one state to another.
        /// Use this to reset any internal state of the transition or perform any additional function as neccessary.
        /// While you could put execution of game logic here, prefer making it a part of the <see cref="FSMForUnity.IFSMState.Enter"/> method of the state
        /// you are entering.
        /// </summary>
        protected internal void PassThrough();
        /// <summary>
        /// Called when the State Machine is destroyed. Destroy any objects managed by this transition here.
        /// Note that if this transition is referenced multiple times its destruction method may be called multiple times.
        /// Implement the <see cref="System.IDisposable"/> pattern as required.
        /// </summary>
        protected internal void Destroy() { }
    }
}