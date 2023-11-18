using FSMForUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates and hosts the machine that flips between a ping and a pong state.
/// Both the Ping and the Pong state are implemented as coroutines,
/// and are able to use the same transition instance to move between each
/// other.
/// </summary>
[AddComponentMenu("")]
public class PingPongCoroutineHost : MonoBehaviour
{
	private FSMMachine fsm;

	private void Awake()
	{
		var builder = FSMMachine.Build();

		var transition = new TriggeredFSMTransition();

		var pingState = builder.AddState(new PingCoroutineState(transition));
		var pongState = builder.AddState(new PongCoroutineState(transition));

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
}
