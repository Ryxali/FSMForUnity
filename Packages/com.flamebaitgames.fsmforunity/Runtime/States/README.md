# Runtime/States
Herein you will find some of the most fundamental patterns required for complex state machine behaviours. By Convention they are all suffixed as an "FSMState". A short description of each you will find below:
## EmptyFSMState
A state that does nothing. This can be useful while prototyping or when doing complex control flow. An example for complex flow could be:
```
var builder = FSMMachine.Build();

var a = builder.AddState(new MyState1());
var b = builder.AddState(new MyState2());
var c = builder.AddState(new MyState3());

var middle = builder.AddState(new EmptyFSMState());

var d = builder.AddState(new MyState4());
var e = builder.AddState(new MyState5());

// Join from a, b, or c, into our empty state
builder.AddTransition(() => myCondition, a, middle);
builder.AddTransition(() => myCondition, b, middle);
builder.AddTransition(() => myCondition, c, middle);

// From our empty state, move as appropriate to d or e
builder.AddTransition(() => dCondition, middle, d);
builder.AddTransition(() => eCondition, middle, e);
```
Note with the example above to achieve the same effect without this middle state you'd need to map transitions for every combination of a,b,c & d,e. Of course, the memory overhead of a transition is very small, so this is left to preference for your project.
## ParallelFSMState
This state takes a number of states and executes them in parallel. This allows you to effectively bundle in any number of states and execute them as though they were a single state. This does not mean that the state is multithreaded, rather for ParallelFSMState.Enter it will call Enter for each underlying state, with the same for Update, Exit, and Destroy.

This promotes code reuse, but is arguably most effective when nesting state machines, as it allows you to have states thate are active in the parent state while the sub state machine executes.
## SubstateFSMState
With this state, you effectively wrap another state machine and treat it as a singular state. When we Enter, the machine is set to Enable, with similar happening with Update, Exit, and Destroy.

Using substates is the most effective way to reduce the number of transitions required when building the machine, as a single transition is sufficient to enter the entire subset of states, and a single transition to exit it.
## LamdbdaFSMState
Lambda states are most effectively used for prototyping or for very simple state behaviours. It allows you to define it by simply adding definitions for its various Enter, Update, Exit, and Destroy methods. This can save you the work of defining an explicit class for your behaviour.

Note that Lambda states will be more difficult to debug due to their anonymous nature. For complex behaviours, consider making defined classes instead.
## CoroutineFSMState
Coroutines are commonly used to describe a sequence of events that transpire over time. This state adds support for executing coroutines within a state. You do this by implementing a class and deriving from this class rather than the base IFSMState interface. Doing so will expose the Enter method as an IEnumerator instead, which is the entry point for your coroutine.

This coroutine will execute as part of the state machine lifecycle. It will advance one step for each Update, and will always be terminated on Exit. This means that no MonoBehaviours are neccessary to host the coroutine, and execution will follow the timings of all other states in the machine.

As this state uses a custom iterator for executing the coroutine, some Unity yield instructions aren't supported within the coroutine like:
* WaitForSeconds
* WaitForSecondsRealtime
* WaitForEndOfFrame
* WaitForFixedUpdate

This is due to that the Unity engine has internal implementations for these classes that are not exposed for packages, but they also do not fall within the lifecycle of a state machine. Time and Update execution point is up to the user, after all. Accepted yield instructions are the null value as well as any type that implements IEnumerator, which includes any CustomYieldInstruction.
