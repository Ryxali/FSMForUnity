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

## Wiki
See [Quick Start](https://github.com/Ryxali/FSMForUnity/wiki) for the wiki.
## Installation
Add it in package manager with github url: `https://github.com/Ryxali/FSMForUnity.git?path=/Packages/com.ryxali.fsmforunity`
## Getting Started
See [Quick Start](https://github.com/Ryxali/FSMForUnity/wiki/Quick-Start) on how to get started.
## Performance
While designed to be lean and performant, this solution still is fundamentally object oriented which has some scaling limitations. Ultimately this depends on computations in states and transitions, but as a ballpark measure, you can affort to have somewhere between 100-1000 active machines updating each frame. This is a limitation of any object oriented FSM solution. If you, for instance, intend to simulate more than 100 agents every frame, consider a data oriented approach instead.

Since machine updates are entirely up to the host object, it's up to you how ofter you wish to update them. This is an important performance feature, as you can update heavier though less responsive states once every 2 or 4 frames instead. Better yet you can update these machines in a looping buffer, so each frame you only update 1/N of the machines.
## Limitations
Does not support any event based paradigm for state transitions

Does not inherently support states blocking transition away from them
