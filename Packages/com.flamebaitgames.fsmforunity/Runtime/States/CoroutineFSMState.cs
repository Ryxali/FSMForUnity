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
    /// </summary>
	public abstract class CoroutineFSMState : IFSMState
    {
        protected float deltaTime { get; private set; }

        private IEnumerator activeRoutine;

        public void Enter()
        {
            deltaTime = 0f;
            activeRoutine = OnEnter();
        }

        public void Exit()
        {
            activeRoutine = null;
        }

        public void Update(float delta)
        {
            if(activeRoutine != null)
            {
                deltaTime = delta;
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
                    OnCoroutineEnd();
                }
            }
        }

        public abstract void Destroy();


        /// <summary>
        /// Called as part of Entering this state,
        /// and acts as the starting point for the
        /// coroutine.
        /// </summary>
        protected abstract IEnumerator OnEnter();

        /// <summary>
        /// Called when the state has reached the
        /// end of the coroutine. At this point
        /// it might be appropriate to trigger something
        /// to transition away from this state.´
        /// </summary>
        protected abstract void OnCoroutineEnd();
    }
}
