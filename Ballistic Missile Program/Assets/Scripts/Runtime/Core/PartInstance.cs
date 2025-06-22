/*  PartInstance.cs
 *  Runtime companion for a PartDefinition.
 *  Keeps only the mutable, per-flight state.
 * -------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

// Behaviours inherit this
// "I" is a naming convention for interface like IEnumerable
public interface IMassProvider {
    float CurrentMass { get; }
    event Action OnMassChanged; // Fire this whenever mass changes
}


[DisallowMultipleComponent]
public class PartInstance : MonoBehaviour {
    #region Variables

    // Design-time Data
    public PartDefinition definition; // set by the spawner

    [HideInInspector] public AttachNode parentAttachNode; // null when root
    [HideInInspector] public AttachNode childAttachNode; // null when not used
    private AttachNode[] _localAttachNodes;

    private float totalMassKg;
    private bool _massDirty = true;

    // Public read-only properties
    public float CurrentMassKg => totalMassKg;
    public IReadOnlyList<AttachNode> LocalAttachNodes => _localAttachNodes; // IReadOnlyList is an interface in C#

    // cached links to scripts
    private IMassProvider[] _massProviders;
    private VehicleInstance _vehicleInstance;

    #endregion

    #region Unity Lifecycle Functions

    private void Awake() {
        // Find vehicle instance in root parent
        // This requires that the vehicle instance is on the root transform
        _vehicleInstance = transform.root.GetComponent<VehicleInstance>();
        if ( _vehicleInstance == null) Debug.LogWarning($"Vehicle Instance Parent Not Found. {gameObject}");

        _localAttachNodes = GetComponentsInChildren<AttachNode>(includeInactive: true);
        if (_localAttachNodes.Length <= 0 ) _localAttachNodes = new AttachNode[0];

        _massProviders = GetComponents<IMassProvider>();
        for (int i = 0; i < _massProviders.Length; i++) _massProviders[i].OnMassChanged += () => _massDirty = true;
    }

    private void LateUpdate() {
        if (_massDirty) RecalculateMass();
    }

    #endregion

    #region API

    // Attach this part to a parent node (called by builder/decoupler).
    // Child node is the one used on this part
    public void AttachTo(AttachNode parentNode, AttachNode childNode) {
        parentAttachNode = parentNode;
        parentNode.CurrentOccupant = this;
        childAttachNode = childNode;
        childNode.CurrentOccupant = this;
        transform.SetParent(parentNode.transform, worldPositionStays: false);
        transform.SetLocalPositionAndRotation(-childNode.transform.localPosition, Quaternion.identity); // Local position and rotation
    }

    // Detach from current parent node (if any).
    // Careful using this you need a new VehicleInstance
    public void Detach() {
        if (parentAttachNode == null) return;

        parentAttachNode.CurrentOccupant = null;
        childAttachNode.CurrentOccupant = null;
        parentAttachNode = null;
        childAttachNode = null;
        transform.SetParent(null, worldPositionStays: true); // new root
    }

    #endregion

    #region Private Functions

    private void RecalculateMass() {
        float total = definition.dryMassKg;
        for (int i = 0; i < _massProviders.Length; i++) total += _massProviders[i].CurrentMass;
        totalMassKg = total;
        _massDirty = false;
        _vehicleInstance.MarkMassDirty();
    }

    #endregion

#if UNITY_EDITOR
    /* Gizmo: green wire cube showing part bounds when selected */
    private void OnDrawGizmosSelected() {
        var col = GetComponentInChildren<Collider>();
        if (!col) return;

        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(col.bounds.center, Quaternion.identity, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, col.bounds.size);
    }
    #endif
}