/*  PartInstance.cs
 *  Runtime companion for a PartDefinition.
 *  Keeps only the mutable, per-flight state.
 * -------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PartInstance : MonoBehaviour {
    #region Variables

    // Design-time Data
    public PartDefinition definition; // set by the spawner

    [HideInInspector] public AttachNode parentAttachPoint; // null when root
    private AttachNode[] _localAttachNodes;

    // Public read-only properties
    public float CurrentMassKg => definition.dryMassKg;
    public IReadOnlyList<AttachNode> LocalAttachNodes => _localAttachNodes; // IReadOnlyList is an interface in C#

    // cached link to vehicle set in awake
    private VehicleInstance _vehicle;

    #endregion

    #region Unity Lifecycle Functions

    private void Awake() {
        _localAttachNodes = GetComponentsInChildren<AttachNode>(includeInactive: true);
        if (_localAttachNodes.Length <= 0 ) _localAttachNodes = new AttachNode[0];
        _vehicle = GetComponentInParent<VehicleInstance>();
    }

    #endregion

    #region API

    // Attach this part to a parent node (called by builder/decoupler).
    // Child node is the one used on this part
    public void AttachTo(AttachNode parentNode, AttachNode childNode) {
        parentAttachPoint = parentNode;
        parentNode.CurrentOccupant = this;
        childNode.CurrentOccupant = this;
        transform.SetParent(parentNode.transform, worldPositionStays: false);
        transform.SetLocalPositionAndRotation(-childNode.transform.localPosition, Quaternion.identity); // Local position and rotation
    }

    // Detach from current parent node (if any).
    public void Detach() {
        if (parentAttachPoint == null) return;

        parentAttachPoint.CurrentOccupant = null;
        parentAttachPoint = null;
        transform.SetParent(null, worldPositionStays: true); // now root
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