using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WingSurfaceBehaviour : MonoBehaviour, IAeroSurface {
    [SerializeField] private float area;

    [SerializeField] private AnimationCurve liftCurve;
    [SerializeField] private AnimationCurve dragCurve;

    public Vector3 referenceDirection = Vector3.forward;
    public Vector3 spanDirection = Vector3.right;

    public float Area => Area;
    public AnimationCurve LiftCurve => liftCurve;
    public AnimationCurve DragCurve => dragCurve;

    private PartInstance _partInstance;
    private VehicleInstance _vehicleInstance;
    private Rigidbody _rb;

    private void Awake() {
        _partInstance = GetComponent<PartInstance>();
        _vehicleInstance = transform.root.GetComponent<VehicleInstance>();
        _rb = _vehicleInstance.GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        _vehicleInstance = transform.root.GetComponent<VehicleInstance>();
        _rb = _vehicleInstance.GetComponent<Rigidbody>();

        ApplyForces(Atmosphere.instance.GetAtmosphereicDensity(_partInstance.Altitude), Atmosphere.instance.WindVelocityWS);
    }

    /// <summary>Called from a central AerodynamicsManager once per FixedUpdate.</summary>
    public void ApplyForces(float airDensity, Vector3 windVelocityWS) {
        Vector3 r = transform.position - _rb.worldCenterOfMass;   // lever arm, worldspace
        Vector3 vAirspeedWS = _rb.velocity                        // whole-vehicle translation
                            + Vector3.Cross(_rb.angularVelocity, r) // local spin speed
                            - windVelocityWS;                      // subtract wind
        if (vAirspeedWS.sqrMagnitude < 1e-3f) return;                // too slow

        Vector3 chordWS = transform.TransformDirection(referenceDirection);
        Vector3 spanWS = transform.TransformDirection(spanDirection);

        // Angle of attack (α) in degrees, signed around the span axis
        float alphaRad = Mathf.Asin(Vector3.Dot(Vector3.Cross(chordWS, vAirspeedWS).normalized,
                                                spanWS.normalized));
        float alphaDeg = alphaRad * Mathf.Rad2Deg;

        // Coefficients
        float Cl = liftCurve.Evaluate(alphaDeg);
        float Cd = dragCurve.Evaluate(Mathf.Abs(alphaDeg));          // signless drag

        float q = 0.5f * airDensity * vAirspeedWS.sqrMagnitude;    // dynamic pressure
        float L = q * area * Cl;                            // Newtons
        float D = q * area * Cd;
        if (!Atmosphere.instance.LiftForceActive) L = 0;

        // Directions
        Vector3 vDir = vAirspeedWS.normalized;
        Vector3 liftDir = Vector3.Cross(vDir, spanWS).normalized;   // right-hand rule
        Vector3 dragDir = -vDir;

        Vector3 point = transform.position;
        if ((liftDir * L).magnitude > 0) _rb.AddForceAtPosition(liftDir * L, point, ForceMode.Force);
        if ((dragDir * D).magnitude > 0) _rb.AddForceAtPosition(dragDir * D, point, ForceMode.Force);

        if (Atmosphere.instance.ShowForceVectors) {
            Debug.DrawRay(point, liftDir * L * Atmosphere.instance.ForceVectorScale, Color.green, Time.fixedDeltaTime, false); // lift
            Debug.DrawRay(point, dragDir * D * Atmosphere.instance.ForceVectorScale, Color.red, Time.fixedDeltaTime, false); // drag
            Debug.DrawRay(point, spanWS.normalized, Color.cyan, Time.fixedDeltaTime, false); // span axis helper
            Debug.DrawRay(point, vDir * vAirspeedWS.magnitude * Atmosphere.instance.ForceVectorScale, Color.blue, Time.fixedDeltaTime);
        }
    }
}
