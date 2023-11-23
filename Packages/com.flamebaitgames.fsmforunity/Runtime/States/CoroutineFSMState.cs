using System.Collections;
using UnityEngine;
using System.Text;
using System.Reflection;

namespace FSMForUnity
{
    /// <summary>
    /// Base class that executes a Coroutine when it enters this state.
    /// The implementing class gets a callback when the coroutine is done,
    /// which in turn can be used to trigger a transition away from this state.
    /// Notable is that this coroutine will execute as part of this <see cref="IFSMState.Update"/> call.
    /// As such, execution timings and rate will be entirely bound to the state machine.
    /// As with all coroutines, some GC will be generated each time this state is entered.
    /// </summary>
	public abstract class CoroutineFSMState : IFSMState
    {
        private readonly DeltaTime deltaTime = new DeltaTime();

        private IEnumerator activeRoutine;

        void IFSMState.Enter()
        {
            deltaTime.value = 0f;
            activeRoutine = Enter(deltaTime);
        }

        void IFSMState.Exit()
        {
            activeRoutine = null;
            Exit();
        }

        void IFSMState.Update(float delta)
        {
            if(activeRoutine != null)
            {
                deltaTime.value = delta;
                var atEnd = false;
                if(activeRoutine.Current != null)
                {
                    if(activeRoutine.Current is IEnumerator en)
                    {
                        if(en.MoveNext())
                            atEnd = !activeRoutine.MoveNext();
                    }
                    else if(activeRoutine.Current is YieldInstruction)
                    {
                        var t = activeRoutine.GetType();
                        activeRoutine = null;
                        throw new System.InvalidOperationException($"{t} Unity yield instructions like WaitForSeconds, WaitForFixedUpdate, and WaitForEndOfFrame not supported by this coroutine.");
                    }
                    else
                    {
                        var t = activeRoutine.GetType();
                        activeRoutine = null;
                        throw new System.NotSupportedException($"{t} yield value´ '{activeRoutine.Current.GetType().FullName}' was not recognized as something that can be iterated on.");
                    }
                }
                else
                {
                    atEnd = !activeRoutine.MoveNext();
                }
                if(atEnd)
                {
                    activeRoutine = null;
                }
            }
        }

        void IFSMState.Destroy() => Destroy();

        /// <summary>
        /// Called as part of Entering this state,
        /// and acts as the starting point for the
        /// coroutine.
        /// </summary>
        /// <param name="deltaTime">A reference to the container which will contain
        /// the delta time for each iteration in this coroutine.</param>
        protected abstract IEnumerator Enter(DeltaTime deltaTime);

        /// <summary>
        /// Called as we leave this state, same as <see cref="IFSMState.Exit"/>
        /// </summary>
        protected abstract void Exit();

        /// <summary>
        /// Called as the State Machine is destroyed. Dispose of any managed objects here.
        /// </summary>
        protected abstract void Destroy();

        /// <summary>
        /// A special construct for coroutines. The delta time value will be automatically
        /// be updated before each iteration of the coroutine.
        /// </summary>
        protected sealed class DeltaTime
        {
            /// <summary>
            /// The current delta time value, identical to the deltaTime value
            /// recieved in the Update function for other non-coroutine states.
            /// </summary>
            public float value { get; internal set; }

            public static implicit operator float(DeltaTime deltaTime) => deltaTime.value;
        }

    }
}
