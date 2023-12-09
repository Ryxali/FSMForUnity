using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FSMForUnity
{
    /// <summary>
    /// Base class that executes a coroutine when the state is entered. This replaces the Enter:void
    /// signature with an Enter:IEnumerator, which acts as the starting point for implementing class.
    /// <para>
    /// Notable is that this coroutine will execute as part of this <see cref="IFSMState.Update"/> call.
    /// As such, execution timings and step rate will be entirely bound to the state machine.
    /// </para>
    /// <para>Since we don't rely on MonoBehaviours to drive the coroutine, some yield instructions are unsupported:
    /// <list type="bullet">
    /// <item>WaitForSeconds</item>
    /// <item>WaitForSecondsRealtime</item>
    /// <item>WaitForEndOfFrame</item>
    /// <item>WaitForFixedUpdate</item>
    /// </list>
    /// WaitForSeconds and WaitForSecondsRealtime have replacement methods built in as protected members of this class.
    /// WaitForEndOfFrame and WaitForFixedUpdate have no equivalent, as this would undermine the state machine's control
    /// over execution timings.
    /// </para>
    /// <para>
    /// As with all coroutines, some GC will be generated each time this state is entered.
    /// </para>
    /// </summary>
	public abstract class CoroutineFSMState : IFSMState
    {
        private const int DefaultRoutineStackSize = 2;

        private readonly Stack<IEnumerator> routineStack;

        /// <summary>
        /// Default constructor, uses <see cref="DefaultRoutineStackSize"/>
        /// as default routine stack capacity.
        /// </summary>
        public CoroutineFSMState()
        {
            deltaTime = new DeltaTime();
            routineStack = new Stack<IEnumerator>(DefaultRoutineStackSize);
        }

        /// <summary>
        /// Constructor that lets you define routine stack capacity. By giving it an exact value
        /// you can minimize memory overhead as well as GC allocations.l
        /// </summary>
        /// <param name="defaultRoutineDepthCapacity">Supply the maximum depth for the coroutine in your state for best result.</param>
        public CoroutineFSMState(int defaultRoutineDepthCapacity)
        {
            deltaTime = new DeltaTime();
            routineStack = new Stack<IEnumerator>(defaultRoutineDepthCapacity);
        }

        void IFSMState.Enter()
        {
            deltaTime.value = 0f;
            routineStack.Push(Enter(deltaTime));
        }

        void IFSMState.Exit()
        {
            routineStack.Clear();
            Exit();
        }

        void IFSMState.Update(float delta)
        {
            deltaTime.value = delta;

            if (routineStack.Count > 0)
            {
                var activeRoutine = routineStack.Peek();

                if (activeRoutine.Current != null)
                {
                    if (activeRoutine.Current is IEnumerator en)
                    {
                        activeRoutine = en;
                        routineStack.Push(en);
                    }
                    else if (activeRoutine.Current is YieldInstruction)
                    {
                        var t = activeRoutine.GetType();
                        routineStack.Clear();
                        throw new System.InvalidOperationException($"{t} Unity yield instructions like WaitForSeconds, WaitForFixedUpdate, and WaitForEndOfFrame not supported by this coroutine.");
                    }
                    else
                    {
                        var t = activeRoutine.GetType();
                        routineStack.Clear();
                        throw new System.NotSupportedException($"{t} yield value '{activeRoutine.Current.GetType().FullName}' was not recognized as something that can be iterated on.");
                    }
                }
                while (activeRoutine != null && !activeRoutine.MoveNext())
                {
                    routineStack.Pop();
                    if (routineStack.Count > 0)
                        activeRoutine = routineStack.Peek();
                    else
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
        /// The body of this method is empty. You don't need to call base.Destroy() if you override it.
        /// </summary>
        protected virtual void Destroy() { }

        /// <summary>
        /// yield this to wait a set number of seconds. This is Equivalent to
        /// the builtin <see cref="UnityEngine.WaitForSeconds"/> instruction, but compatible with the coroutine in this state.
        /// <para>Note that this will wait for the corresponding <see cref="Time.time"/> elapsed in the Unity engine,
        /// and not the cumulative delta time of each step of the machine.</para>
        /// </summary>
        /// <param name="seconds">Seconds in Unity time to wait.</param>
        /// <returns>an enumerator you can yield to hold the coroutine.</returns>
        protected IEnumerator WaitForSeconds(float seconds)
        {
            var end = Time.time + seconds;
            while (Time.time < end)
                yield return null;
        }
        /// <summary>
        /// yield this to wait a set number of realtime seconds. This is Equivalent to
        /// the builtin <see cref="UnityEngine.WaitForSecondsRealtime"/> instruction, but compatible with the coroutine in this state.
        /// </summary>
        /// <param name="seconds">Seconds in real time to wait.</param>
        /// <returns>an enumerator you can yield to hold the coroutine.</returns>
        protected IEnumerator WaitForSecondsRealtime(float seconds)
        {
            var end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
                yield return null;
        }

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
