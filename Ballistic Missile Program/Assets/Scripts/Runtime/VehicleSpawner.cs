using System;
using System.Collections.Generic;
using UnityEngine;

/// Static helper that turns a VehicleBlueprint into a ready-to-fly VehicleInstance.
public class VehicleSpawner :MonoBehaviour {
    [SerializeField] private GameObject defaultVehiclePrefab;

    public VehicleInstance Build(VehicleBlueprint bp, Vector3 position, Quaternion rotation, Transform parent = null) {
        GameObject rootGO = GameObject.Instantiate(defaultVehiclePrefab, position, rotation);
        rootGO.transform.SetPositionAndRotation(position, rotation);
        if (parent) rootGO.transform.SetParent(parent, worldPositionStays: true); // Keep same world space position and different local position, worldPositionStays

        VehicleInstance vehicle = rootGO.AddComponent<VehicleInstance>();

        List<PartInstance> partInstances = new List<PartInstance>(bp.parts.Count);

        for (int i = 0; i < bp.parts.Count; i++)
        {
            GameObject go = GameObject.Instantiate(bp.parts[i].PartDefinition.runtimePrefab, Vector3.zero, Quaternion.identity, rootGO.transform);
            partInstances.Add(go.GetComponent<PartInstance>());
            vehicle.Parts.Add(partInstances[i]);
        }

        for (int i = 0; i < bp.parts.Count; i++)
        {
            PartEntry pe = bp.parts[i];

            if (pe.parentIndex < 0) continue; // Skip root, it already has the right parent

            PartInstance childPI = partInstances[i];
            PartInstance parentPI = partInstances[pe.parentIndex];

            int attachNodeIndex = GetAttachNodeIndexFromID(parentPI, pe.ParentAttachNodeID);
            AttachNode parentAttachNode = parentPI.LocalAttachNodes[attachNodeIndex];

            childPI.AttachTo(parentAttachNode);
        }

        vehicle.MarkMassDirty();
        vehicle.RecalculateMass();
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
}
