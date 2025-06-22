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

    [HideInInspector] public AttachNode parentAttachNode; // null when root
    [HideInInspector] public AttachNode childAttachNode; // null when not used
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