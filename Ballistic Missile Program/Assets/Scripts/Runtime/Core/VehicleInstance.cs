/*  VehicleInstance.cs
 *  One of these lives on the root GameObject that owns a single Rigidbody.
 *  - Maintains the part list
 *  - Rebuilds mass / centre-of-mass when parts change
 *  - Provides stage-separation helper (no StageInstance MonoBehaviour required)
 * ------------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(Rigidbody))] // Note that this adds a rigidbody on instantiation if it doesn't exist
//[DisallowMultipleComponent]
public class VehicleInstance : MonoBehaviour {
    #region Variables

    [HideInInspector] public string vehicleGuid;

    [HideInInspector] public List<PartInstance> Parts = new List<PartInstance>();

    private Rigidbody _rb;
    private bool _massDirty = true; // recalc in first LateUpdate

    #endregion

    #region Unity Lifecycle Functions

    private void Awake() {
        _rb = GetComponent<Rigidbody>();

        vehicleGuid = Guid.NewGuid().ToString();

        UpdateParts();

        RecalculateMass();
    }

    private void LateUpdate() {
        if (_massDirty) {
            UpdateParts();
            RecalculateMass();
        }

        if (transform.childCount <= 0) Destroy(gameObject);
    }

    #endregion

    #region API

    /// Mark that something changed mass or CoM; will recalc next frame.
    public void MarkMassDirty() => _massDirty = true;

    #endregion

    #region Private Functions

    public void RecalculateMass() {
        float totalMass = 0f;
        Vector3 worldCoM = Vector3.zero;

        for (int i = 0; i < Parts.Count; i++) {
            float mass = Parts[i].CurrentMassKg;
            totalMass += mass;
            worldCoM += mass * Parts[i].transform.position;
        }

        if (totalMass <= 0f) totalMass = 1f; // Avoid NaN values

        worldCoM /= totalMass; // Normalize for vehicle mass

        _rb.mass = totalMass;
        _rb.centerOfMass = transform.InverseTransformPoint(worldCoM); // World space to local space CoM
        _rb.ResetInertiaTensor();

        _massDirty = false;
    }

    public void UpdateParts() {
        Parts.Clear();

        PartInstance[] foundParts = GetComponentsInChildren<PartInstance>();

        for (int i = 0; i < foundParts.Length; i++) {
            Parts.Add(foundParts[i]);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        List<PartInstance> collisionParts = new List<PartInstance>();
        for (int y = 0; y < collision.contacts.Length; y++) {
            for (int i = 0; i < Parts.Count; i++) {
                for (int x = 0; x < Parts[i].transform.childCount; x++) {
                    // If collision object is equal to the one we're currently looking at in the for loop
                    if (collision.contacts[y].thisCollider.gameObject == Parts[i].transform.GetChild(x).gameObject) collisionParts.Add(Parts[i]);
                }
            }

            // This code actually doesn't work
            if (collision.contacts[y].otherCollider.transform.parent == null) continue;
            if (collision.contacts[y].otherCollider.transform.parent.GetComponent<PartInstance>() == null) continue;
            collisionParts.Add(collision.contacts[y].otherCollider.transform.parent.GetComponent<PartInstance>());
        }

        for (int i = 0; i < collisionParts.Count; i++) {
            if (collisionParts[i].GetComponent<ExplosiveBehaviour>() == null) continue;
            collisionParts[i].GetComponent<ExplosiveBehaviour>().ProcessChildCollision(collision);
        }
    }

    #endregion

#if UNITY_EDITOR

    /* Scene-view gizmo: yellow cross at the centre-of-mass */
    private void OnDrawGizmosSelected() {
        if (!_rb) _rb = GetComponent<Rigidbody>();

        Gizmos.color = Color.yellow;
        Vector3 com = transform.TransformPoint(_rb.centerOfMass);

        float scale = 5f;
        Gizmos.DrawLine(com - Vector3.up * scale, com + Vector3.up * scale);
        Gizmos.DrawLine(com - Vector3.right * scale, com + Vector3.right * scale);
        Gizmos.DrawLine(com - Vector3.forward * scale, com + Vector3.forward * scale);
    }

    #endif
}
