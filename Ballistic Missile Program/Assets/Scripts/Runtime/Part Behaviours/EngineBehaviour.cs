using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineBehaviour : MonoBehaviour {
    public EngineDefinition def;

    [Range(0f, 1f)]
    public float throttle = 0f;

    public Vector2 gimbalAngles;

    Rigidbody _rb; // Cached reference

    private void Awake() {
        _rb = transform.root.GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        _rb = transform.root.GetComponent<Rigidbody>(); // If we get a new parent

        float thrust = throttle * def.maxThrustNewtons;
        _rb.AddForce(transform.up * thrust, ForceMode.Force);

        // Todo: drain fuel

        Vector2 applyGimbal = new Vector2(Mathf.Min(gimbalAngles.x, def.maxGimbalAngles.x), Mathf.Min(gimbalAngles.y, def.maxGimbalAngles.y));

        transform.localRotation = Quaternion.Euler(applyGimbal.y, 0f, -applyGimbal.x);
    }
}
