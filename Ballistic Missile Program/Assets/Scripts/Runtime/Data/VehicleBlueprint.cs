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
public struct PartField {
    public PartDefinition PartDefinition;
    public string ParentAttachNodeID;
    [Space]
    public PartField[] ChildPartDefinitions;

    public PartField(PartDefinition _partDefinition, string _parentAttachNodeID = "", PartField[] _childPartDefinitions = null) {
        PartDefinition = _partDefinition;
        ParentAttachNodeID = _parentAttachNodeID;
        ChildPartDefinitions = _childPartDefinitions;
        if (ChildPartDefinitions == null) ChildPartDefinitions = new PartField[0];
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
    public List<PartField> parts = new();

    /// <summary>Total dry mass of all parts (kg).</summary>
    public float DryMassKg {
        get {
            if (parts.Count == 0) return 0.0f; // If parts[0] doesn't exist
            return GetPartFieldDryMass(parts[0]);
        }
    }

    private float GetPartFieldDryMass(PartField pf) {
        float total = pf.PartDefinition.dryMassKg;
        // Recursively get all parts in the tree
        for (int i = 0; i < pf.ChildPartDefinitions.Length; i++) {
            total += GetPartFieldDryMass(pf.ChildPartDefinitions[i]);
        }
        return total;
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