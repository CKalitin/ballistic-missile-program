/*  VehicleBlueprint.cs
 *  Design-time recipe for assembling a full launch vehicle.
 *  Runtime code loads this asset, instantiates every PartDefinition,
 *  and plugs them together via AttachPoint nodes.
 * ----------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct PartEntry {
    public PartDefinition PartDefinition;
    [Space]
    [Tooltip("Index of the parent PartEntry in the parts list, or -1 for the root part.")]
    public int parentIndex;
    [Space]
    public string ParentAttachNodeID;
    [Tooltip("The node ID on THIS part that is attached to the parent.")]
    public string ChildAttachNodeID;

    public PartEntry(PartDefinition _partDefinition, int _parentIndex = -1, string _parentAttachNodeID = "", string _childAttachNodeID = "") {
        PartDefinition = _partDefinition;
        parentIndex = _parentIndex;
        ParentAttachNodeID = _parentAttachNodeID;
        ChildAttachNodeID = _childAttachNodeID;
    }
}

/// ScriptableObject that lists every part, its stage index, and quantity.
[CreateAssetMenu(menuName = "BallisticMissileProgram/Vehicle Blueprint")]
public class VehicleBlueprint : ScriptableObject {
    #region Variables

    [HideInInspector] public string guid;

    public string displayName = "New Vehicle";

    public string displayDescription = "New Vehicle Description";

    [Tooltip("Hierarchical list of parts and their attach nodes. Ensure there's only one root.")]
    public List<PartEntry> parts = new();

    /// <summary>Total dry mass of all parts (kg).</summary>
    public float DryMassKg {
        get {
            float total = 0f;
            for (int i = 0; i < parts.Count; i++) {
                total += parts[i].PartDefinition.dryMassKg;
            }
            return total;
        }
    }

    #endregion

#if UNITY_EDITOR
    /* Auto-generate GUID once and validate entries in the Inspector */
    private void OnValidate() {
        if (string.IsNullOrEmpty(guid)) {
            guid = Guid.NewGuid().ToString(); // 32-char hex
            EditorUtility.SetDirty(this); // Mark asset dirty so Unity saves it
        }
    }
#endif
}