using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implements a very basic player controller to
/// interact with the AI agent.
/// </summary>
[AddComponentMenu("")]
public class PlayerAgent : MonoBehaviour
{
    [SerializeField]
    private new Camera camera;
    [SerializeField]
    private float moveSpeed = 1f;

    void Update()
    {
        // Do simple movement logic
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");

        var camForward = Vector3.ProjectOnPlane(camera.transform.forward, transform.up).normalized;
        var camRight = -Vector3.Cross(camForward, transform.up);

        transform.position += (camForward * v + camRight * h).normalized * moveSpeed * Time.deltaTime;

    }
}
