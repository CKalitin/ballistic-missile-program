using System;
using UnityEngine;

#region DATA

/// <summary>
/// Pure-data description of an attachment node on a part prefab.
/// </summary>
[Serializable]
public struct AttachNodeDef {
    [Tooltip("Unique node ID string inside this part prefab (e.g. engine_mount_1)")]
    public string id;

    [Tooltip("Bitmask of PartCategories that are allowed to occupy this node")]
    public PartCategory acceptsMask;

    [Tooltip("Local forward direction used for automatic alignment")]
    public Vector3 localDirection;

    public AttachNodeDef(string id,
                          PartCategory mask,
                          Vector3 pos,
                          Vector3 dir) {
        this.id = id;
        this.acceptsMask = mask;
        this.localDirection = dir == Vector3.zero ? Vector3.forward : dir.normalized;
    }
}

#endregion

#region MONOBEHAVIOUR

/// <summary>
///  Lightweight wrapper placed on a child Transform to expose an AttachPointDef
///  plus runtime info (who is currently docked to this node).
/// </summary>
public class AttachNode : MonoBehaviour {
    public AttachNodeDef def; // For editor

    [HideInInspector] public PartInstance CurrentOccupant; // For runtime

    // Set local position relative to parent part
    public Vector3 LocalPosition => transform.localPosition;

    // If it accepts a certain part (supports combination of part categories)
    public bool AcceptsPart(PartDefinition candidate)
        => (def.acceptsMask & candidate.category) != 0;

    // Snaps 'child' Transform to this node using definition pose.
    public void SnapTransform(Transform child) {
        child.SetPositionAndRotation(
            transform.position,
            Quaternion.LookRotation(
                transform.TransformDirection(
                    def.localDirection == Vector3.zero
                        ? Vector3.forward
                        : def.localDirection.normalized),
                transform.up)
        );
    }

#if UNITY_EDITOR
    // Pretty gizmos in the Scene view
    private void OnDrawGizmos() {
        // Direction arrow
        Gizmos.DrawLine(transform.position,
                        transform.position +
                        transform.TransformDirection(def.localDirection) * 0.01f);
    }
#endif
}

#endregion