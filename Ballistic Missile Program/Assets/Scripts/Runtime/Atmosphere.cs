using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atmosphere : MonoBehaviour {
    public static Atmosphere instance;

    public float SeaLevelDensity = 1.225f; // kg/m3
    public float ScaleHeight = 100f;
    public Vector3 WindVelocityWS;
    [Space]
    public bool ShowForceVectors;
    public float ForceVectorScale = 0.01f;
    [Space]
    public bool LiftForceActive = false;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.LogWarning("Multiple Atmospheres");
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.F12)) {
            ShowForceVectors = !ShowForceVectors;
        }
    }

    public float GetAtmosphereicDensity(float altitude) {
        return SeaLevelDensity * Mathf.Exp(-altitude / ScaleHeight);
    }
}
