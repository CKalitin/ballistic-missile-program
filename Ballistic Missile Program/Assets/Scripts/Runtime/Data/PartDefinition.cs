using System;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

// Bit shifting allows for combinations of part category, if I decide to do it this way
[Flags]
public enum PartCategory {
    None = 0,           // 0000
    Engine = 1 << 0,    // 0001
    Tank = 1 << 1,      // 0010
    Decoupler = 1 << 2, // 0100
}

[CreateAssetMenu(menuName = "BallisticMissileProgram/Part Definition")]
public class PartDefinition : ScriptableObject {
    [HideInInspector] public string guid;
    public string displayName;
    [Space]
    public PartCategory category;
    public GameObject runtimePrefab;
    [Space]
    public PartBehaviourDefinition[] partBehavioursDefinitions;
    [Space]
    public float dryMassKg;
    [Tooltip("Including Part Behaviours")]
    [ReadOnly]
    public float totalMassKg;

#if UNITY_EDITOR
    // This runs whenever the asset is changed or created
    private void OnValidate() {
        if (string.IsNullOrEmpty(guid)) {
            guid = Guid.NewGuid().ToString(); // 32-char hex
            EditorUtility.SetDirty(this); // Mark asset dirty so Unity saves it
        }

        float mass = dryMassKg;
        if (partBehavioursDefinitions != null) {
            for (int i = 0; i < partBehavioursDefinitions.Length; i++) {
                if (partBehavioursDefinitions[i] == null) continue;
                mass += partBehavioursDefinitions[i].GetMassContribution();
            }
        }
        totalMassKg = mass;
    }
#endif
}
