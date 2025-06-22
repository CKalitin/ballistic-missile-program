using System.Collections.Generic;
using UnityEngine;

public class DecoupleBehaviour : MonoBehaviour {
    [SerializeField] private float decoupleImpulse;

    [SerializeField] private bool decouple;
    [SerializeField] private bool decoupled = false;

    private void Update() {
        if (decouple && !decoupled) {
            Decouple();
            decoupled = true;
        }
    }

    public void Decouple() {
        PartInstance pi = GetComponent<PartInstance>();
        VehicleInstance viParent = GetVehicleInstanceOfPart(pi);
        Rigidbody rbParent = viParent.GetComponent<Rigidbody>();

        Vector3 contactPoint = transform.position;
        Vector3 impulseDir = transform.up;
        Debug.Log(impulseDir);

        pi.Detach();

        VehicleInstance viChild = FindObjectOfType<VehicleSpawner>().MakeNewParent(pi);
        Rigidbody rbChild = viChild.GetComponent<Rigidbody>();

        viParent.UpdateParts();
        viChild.UpdateParts();

        viParent.RecalculateMass();
        viChild.RecalculateMass();

        // Copy velocity to new object
        rbChild.velocity = rbParent.velocity;

        if (decoupleImpulse > 0f) {
            rbParent.AddForceAtPosition(impulseDir * decoupleImpulse, contactPoint, ForceMode.Impulse);
            rbChild.AddForceAtPosition(-impulseDir * decoupleImpulse, contactPoint, ForceMode.Impulse);
        }
    }

    private VehicleInstance GetVehicleInstanceOfPart(PartInstance pi) {
        Transform parent = transform; // Start with current transform
        while (parent.parent != null) {
            parent = parent.parent;
            if (parent.GetComponent<VehicleInstance>() != null) {
                return parent.GetComponent<VehicleInstance>();
            }
        }
        Debug.LogWarning($"Could not find Vehicle Instance of part {pi.name}. {pi.gameObject}");
        return null;
    }
}
