using UnityEngine;

/// Static helper that turns a VehicleBlueprint into a ready-to-fly VehicleInstance.
public static class VehicleSpawner {
    // Build a craft at world position/rotation and parent.
    public static VehicleInstance Build(VehicleBlueprint bp, Vector3 position, Quaternion rotation, Transform parent = null) {
        GameObject rootGO = new GameObject(bp.displayName);
        rootGO.transform.SetPositionAndRotation(position, rotation);
        if (parent) rootGO.transform.SetParent(parent, worldPositionStays: true); // Keep same world space position and different local position, worldPositionStays

        VehicleInstance vehicle = rootGO.AddComponent<VehicleInstance>();

        InstantiatePartAndChildren(bp.parts[0], rootGO.transform);

        vehicle.MarkMassDirty(); // compute mass/CoM once
        return vehicle;
    }

    private static void InstantiatePartAndChildren(PartField pf, Transform parent, AttachNode parentAttachNode=null) {
        GameObject partGO = GameObject.Instantiate(pf.PartDefinition.runtimePrefab, Vector3.zero, Quaternion.identity, parent);
        PartInstance partInstance = partGO.GetComponent<PartInstance>();

        if (parentAttachNode != null) {
            partInstance.AttachTo(parentAttachNode);
        }

        for (int i = 0; i < pf.ChildPartDefinitions.Length; i++) {
            int attachNodeID = GetAttachNodeIndexFromID(partInstance, pf.ChildPartDefinitions[i].ParentAttachNodeID); // Split variables here to make it look nicer, it's all in heap anyway right? C# either way this isn't something to optimize for
            AttachNode an = partInstance.LocalAttachNodes[attachNodeID];
            InstantiatePartAndChildren(pf.ChildPartDefinitions[i], partGO.transform);
        }
    }

    private static int GetAttachNodeIndexFromID(PartInstance pi, string id) {
        for (int i = 0; pi.LocalAttachNodes.Count > 0; i++) {
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
