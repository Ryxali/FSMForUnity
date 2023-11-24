#  Runtime/Transitions
Contained is a collection of commonly useful Transition types to be used when building a state machine. By convention they are all suffixed by "FSMTransition". A short description for each you will find below:
## AlwaysFSMTransition
A transition that always wants to be passed through. It can be useful if you have a set sequence of states you always wish to pass through. Since this transition contains no internal state, it has a single instance that can be accessed via AlwaysFSMTransition.constant.
## InvertFSMTransition
A decorator that will invert the ShouldTransition condition of the supplied transition. It will call PassThrough and Destroy on the decorated transition as expected.
## LambdaFSMTransition
Takes a boolean expression that is used to resolve its ShouldTransition result. This is the quickest way to define a transition, but it does not offer the callback for PassThrough and Destroy.
```
// Example, a machine that describes two different visual states
// depending on how fast the rigid body is moving
var builder = FSMMachine.Build();

var walkState = builder.AddState(new WalkFSMState());
var dashState = builder.AddState(new DashFSMState());

// move to dash visuals when velocity goes beyond 5
builder.AddTransition(() => rigidbody.velocity.magnitude > 5f, walkState, dashState);
// move back to walk visuals when velocity drops below 2
builder.AddTransition(() => rigidbody.velocity.magnitude < 2f, dashState, walkState);
...
```
## AllPassesFSMTransition
Takes a series of transitions and only wishes to be passed through if ShouldTransition results in true for the entire series of states.
## AnyPassesFSMTransition
Takes a series of transitions and wishes to be passed through if any of the transition's ShouldTransition results in true.
## TriggeredFSMTransition
A transition that exposes an additional Trigger method. When triggered, it acts as a single use ticket to pass through this transition. When the transition has been passed through, it will need to be triggered again for another pass through.
