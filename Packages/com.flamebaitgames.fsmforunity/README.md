# FSM For Unity
This package contains a lightweight, simple finite state machine implementation that can be used to create complex machines. With it, you define your own states and transitions between those states, and the machine will handle resolving these transitions and moving between its states.

FSM For Unity is designed for the state machine to be largely autonomous, which allows for predictable execution flows.

It is also designed to have a robust method of constructing a machine, exposing a builder that does safety checks to ensure a stable machine is output. This builder is also highly extensible, allowing you to define your own extension methods to define your particular workflow when building state machines.

## Features
* Simple and safe construction method to ensure the machine is configured correctly.
* Low overhead as it does not derive from MonoBehaviour.
* Very simple contract for implementing your own states.
* Well documented.
* Utilizes composition to create complex relationships.
* Supports coroutines in states without relying on MonoBehaviours.
* Supports nesting state machines to define complex behavior.
* Includes several built-in state and transition types for common scenarios.
* Supports quick prototyping by defining states and transitions with expressions.
* Includes a visual debugger for viewing your state machines in play mode.

## Table of Contents
TODO
## Installation
TODO
## Getting Started
The general pattern to constructing a state machine is:
1. Get a builder via FSMMachine.Build()
2. Add states
3. Add transitions
4. Output the machine via builder.Complete();
If we look at the Simple Ping Pong FSM Sample, we can see a very simple example of this:
```
// in the Awake() method
var builder = FSMMachine.Build();

var pingState = builder.AddState(new PingState());
var pongState = builder.AddState(new PongState());

var transition = new AutoTimedTransition(cooldown);

builder.AddTransition(transition, pingState, pongState);
builder.AddTransition(transition, pongState, pingState);

fsm = builder.Complete();
```
Here we have a machine that wishes to alternate between the PingState and the PongState, with our transition acting as the rule and the path between them. With these methods you can add any number of states, and any number of transitions between these states.
### Executing the Machine
Since the machine is not itself a MonoBehaviour, it needs something to host it. Generally, like with the Simple Ping Pong FSM example, this is done by housing the machine within a MonoBehaviour. The machine exposes four methods:
* Enable
* Update
* Disable
* Destroy
The machine must be enabled for it to perform anything in Update. When constructed it's disabled by default. You can either Enable it and leave it as so, or let the machine follow the lifecycle of the host object. With the latter example looking back at our example it becomes:
```
using FSMForUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingPongHost : MonoBehaviour
{
	private FSMMachine fsm;
	[SerializeField]
	private float cooldown = 1f;

	private void Awake()
	{
		var builder = FSMMachine.Build();

		var pingState = builder.AddState(new PingState());
		var pongState = builder.AddState(new PongState());

		var transition = new AutoTimedTransition(cooldown);

		builder.AddTransition(transition, pingState, pongState);
		builder.AddTransition(transition, pongState, pingState);

		fsm = builder.Complete();
	}

	private void OnEnable()
	{
		fsm.Enable();
	}

	private void OnDisable()
	{
		fsm.Disable();
	}

	private void Update()
	{
		fsm.Update(Time.deltaTime);
	}

	private void OnDestroy()
	{
		fsm.Destroy();
		fsm = null;
	}
}
```
### Why States before Transitions
As part of the builder's safety checks, you can't add transitions to states unless those states have been explicitly added to the builder beforehand. You can certainly interweave adding states and transitions as long as transitions map to already added states.
### The Any Transition
Sometimes you don't want to map a transition as going from one explicit state to another. This is possible by calling `builder.AddAnyTransition(transition, to)`. This effectively means the transition maps from any of the states in the machine to the destination state.
### Circular Transitions
The builder allows you to map a transition from a state back to the same state. This effectively acts as a reset condition as the state will exit, then enter again.
## State Machine Lifecycle
This section will go into depth on how the state machine works, and how this relates to its states and transitions.

While not a MonoBehaviour, the state machine exposes methods that mirrors the lifecycle of such an object. This means we can Enable/Disable it, Update it, and Destroy it.
### State activity
For purposes of this document, a state is considered active when it's Enter is called, and will remain active until it's Exit is called.
### Enable/Disable
The machine can exist in either an enabled or disabled state.

When disabled, no states are active, and if we call Disable while the machine is enabled, it will Exit its active state.

When enabled, a single state in the machine is active. If we call Enable, the following will happen:
1. It will enter the default state of the machine, this is typically the first state added to the machine
2. It will check all transitions relevant to the active state, and transition to the next state accordingly.
3. If a transition was passed through, repeat step 2.
## Implementing Your Own State
To implement your own state, create a new class inheriting the IFSMState interface, then implement its Enter, Exit, Update, and Destroy methods. That's it! The general assumptions the state machine makes in regards to your state are:
* The state performs any logic required as it's activated in its Enter method
* The state performs any logic required as it's deactivated in its Exit method
For instance, if your state shows a particular UI element. It sets showing=true on Enter, then showing=false on Exit. This way you avoid having any unintended side effects persist after the state is left.

Some states will make heavy use of Enter/Exit, while some might only be interested in the Update. implementation may leave any or all of these methods empty.

If your state contains data managed by the state, such as a GameObject instantiated as the state is constructed, be sure to Dispose of any such data in the state's Destroy method.

Finally, since IFSMState is an interface, you are free to apply it to any class, regardless of inheritance. As such, you can define classes that are ScriptableObjects and MonoBehaviours as states.
## Implementing Your Own Transition
To implement a transition, create a class and inherit the IFSMTransition interface. It wil require you to implement three different methods:
* `ShouldTransition()` - this evaluates when we should pass through this transition, and is the most essential for a transition.
* `PassThrough()` - this is called as we pass through this transition, moving to a new state. This is useful if you have some data to reset between each transition.
* `Destroy()` - dispose of any data managed by this transition, if any.

Important to note regarding `PassThrough()` is that transitions can be passed through even if their `ShouldTransition()` evaluates to false. This is because some transition implementations modify the `ShouldTransition()` behaviour, for example the InvertFSMTransition which inverts the condition.
## Using the Debugger
TBD
## Limitations
Does not support any event based paradigm for state transitions

Does not inherently support states blocking transition away from them
