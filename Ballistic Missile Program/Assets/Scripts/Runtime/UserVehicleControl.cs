using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple player input → engine-gimbal driver.
/// Attach this to the same root GameObject that owns a VehicleInstance.
/// </summary>
[RequireComponent(typeof(VehicleInstance))]
public class UserVehicleControl : MonoBehaviour {
    [Header("Throttle settings")]
    [Tooltip("How fast throttle changes per second when key is held")]
    public float throttleRate = 0.5f;     // 0.5 → 2 s from 0 to 1
    public float minThrottle = 0f;        // Idle
    public float maxThrottle = 1f;        // Full

    [HideInInspector] public float throttle;   // 0‒1, public so UI can read

    private readonly List<EngineBehaviour> _engines = new();

    private void Update() {
        _engines.Clear();
        _engines.AddRange(GetComponentsInChildren<EngineBehaviour>(includeInactive: true));

        Gimbal();
        Throttle();
    }

    private void Gimbal() {
        float yawInput = Input.GetAxisRaw("Horizontal"); // A = –1, D = +1
        float pitchInput = Input.GetAxisRaw("Vertical");   // W = +1, S = –1

        // Convert to degrees and target max possible gimbal
        float yawDeg = yawInput * 90f;
        float pitchDeg = -pitchInput * 90f;

        Vector2 gimbal = new(pitchDeg, yawDeg); // (Pitch, Yaw)

        foreach (var eng in _engines)
            eng.TargetGimbalAngles = gimbal;
    }

    private void Throttle() {
        float delta = 0f;
        if (Input.GetKey(KeyCode.LeftShift)) delta += throttleRate * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftControl)) delta -= throttleRate * Time.deltaTime;
        if (Input.GetKey(KeyCode.Z)) delta = 5;
        if (Input.GetKey(KeyCode.X)) delta = -5;

        throttle = Mathf.Clamp(throttle + delta, minThrottle, maxThrottle);

        foreach (var eng in _engines)
            eng.Throttle = throttle;
    }
}