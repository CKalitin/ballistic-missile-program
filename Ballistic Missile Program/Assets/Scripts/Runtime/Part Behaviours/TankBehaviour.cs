using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankBehaviour : MonoBehaviour, IMassProvider {
    [SerializeField] private float fuelMass;

    public float CurrentMass => fuelMass;
    public event Action OnMassChanged;

    private void Update() {
        OnMassChanged?.Invoke();
    }
}
