using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// Static helper that turns a VehicleBlueprint into a ready-to-fly VehicleInstance.
public class VehicleSpawner :MonoBehaviour {
    [SerializeField] private GameObject defaultVehiclePrefab;

    public VehicleInstance Build(VehicleBlueprint bp, Vector3 position, Quaternion rotation, Transform parent = null) {
        GameObject rootGO = GameObject.Instantiate(defaultVehiclePrefab, position, rotation);
        if (parent) rootGO.transform.SetParent(parent, worldPositionStays: true); // Keep same world space position and different local position, worldPositionStays

        VehicleInstance vehicle = rootGO.AddComponent<VehicleInstance>();

        List<PartInstance> partInstances = new List<PartInstance>(bp.parts.Count);

        for (int i = 0; i < bp.parts.Count; i++) {
            GameObject go = GameObject.Instantiate(bp.parts[i].PartDefinition.runtimePrefab, position, Quaternion.identity, rootGO.transform);
            partInstances.Add(go.GetComponent<PartInstance>());
        }

        for (int i = 0; i < bp.parts.Count; i++) {
            PartEntry pe = bp.parts[i];

            if (pe.parentIndex < 0) continue; // Skip root, it already has the right parent

            PartInstance childPI = partInstances[i];
            PartInstance parentPI = partInstances[pe.parentIndex];

            int parentAttachNodeIndex = GetAttachNodeIndexFromID(parentPI, pe.ParentAttachNodeID);
            AttachNode parentAttachNode = parentPI.LocalAttachNodes[parentAttachNodeIndex];

            int childAttachNodeIndex = GetAttachNodeIndexFromID(childPI, pe.ChildAttachNodeID);
            AttachNode childAttachNode = childPI.LocalAttachNodes[childAttachNodeIndex];

            childPI.AttachTo(parentAttachNode, childAttachNode);
        }

        SetLayerRecursively(rootGO.transform, LayerMask.NameToLayer("Vehicle"));

        vehicle.UpdateParts();
        vehicle.MarkMassDirty();
        vehicle.RecalculateMass();
        return vehicle;
    }

    // Take an existing piece of a vehicle and make it its own parent
    public VehicleInstance MakeNewParent(PartInstance pi) {
        Transform t = pi.transform;
        GameObject go = pi.gameObject;

        GameObject rootGO = GameObject.Instantiate(defaultVehiclePrefab, t.position, t.rotation);

        VehicleInstance vehicle = rootGO.AddComponent<VehicleInstance>();

        t.SetParent(rootGO.transform, worldPositionStays: true);

        SetLayerRecursively(rootGO.transform, LayerMask.NameToLayer("Vehicle"));

        return vehicle;
    }

    private int GetAttachNodeIndexFromID(PartInstance pi, string id) {
        for (int i = 0; i < pi.LocalAttachNodes.Count; i++) {
            if (pi.LocalAttachNodes[i].def.id == id) {
                if (pi.LocalAttachNodes[i].CurrentOccupant != null) {
                    Debug.LogWarning($"Attach Node {id} on {pi.name} is already in use. GameObject: {pi.gameObject}");
                }
                return i;
            }
        }
        Debug.LogWarning($"Could not find Attach Node {id} on {pi.name}. GameObject: {pi.gameObject}");
        return 0;
    }

    private static void SetLayerRecursively(Transform t, int layer) {
        t.gameObject.layer = layer;
        for (int i = 0; i < t.childCount; i++) {
            SetLayerRecursively(t.GetChild(i), layer);
        }
    }
}
