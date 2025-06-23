using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineBehaviour : MonoBehaviour {
    public EngineDefinition def;

    [Range(0f, 1f)]
    public float Throttle = 0f;

    public Vector2 TargetGimbalAngles;

    public Transform rotateObject;

    private Vector2 _currentGimbalAngles;

    Rigidbody _rb; // Cached reference

    private void Awake() {
        _rb = transform.root.GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        _rb = transform.root.GetComponent<Rigidbody>(); // If we get a new parent

        float thrust = Throttle * def.maxThrustNewtons;
        _rb.AddForceAtPosition(rotateObject.up * thrust,
                               transform.position,
                               ForceMode.Force);

        // Fuel-drain (TODO)

        // Gimbal with rate-limit
        Vector2 target = new Vector2(
            Mathf.Clamp(TargetGimbalAngles.x, -def.maxGimbalAngles.x, def.maxGimbalAngles.x),
            Mathf.Clamp(TargetGimbalAngles.y, -def.maxGimbalAngles.y, def.maxGimbalAngles.y)
        );

        // (b) Compute the max step size this frame
        float step = def.maxGimalPerSecond * Time.fixedDeltaTime;

        // (c) Move current toward target, axis-wise
        _currentGimbalAngles.x = Mathf.MoveTowards(_currentGimbalAngles.x, target.x, step);
        _currentGimbalAngles.y = Mathf.MoveTowards(_currentGimbalAngles.y, target.y, step);

        // (d) Apply to the nozzle ‒ note axis mapping
        rotateObject.localRotation =
            Quaternion.Euler(_currentGimbalAngles.y, 0f, -_currentGimbalAngles.x);
    }
}
