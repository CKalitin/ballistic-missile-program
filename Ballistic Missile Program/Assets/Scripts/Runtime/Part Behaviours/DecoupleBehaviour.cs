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
        VehicleInstance viParent = transform.root.GetComponent<VehicleInstance>();
        if (viParent == null) Debug.LogWarning($"Vehicle Instance Parent Not Found. {gameObject}");
        Rigidbody rbParent = viParent.GetComponent<Rigidbody>();


        Vector3 contactPoint = transform.position;
        Vector3 impulseDir = transform.up; // Local up

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
}
