using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBehaviour : MonoBehaviour {
    public ExplosiveDefinition def;

    public void ProcessChildCollision(Collision collision) {
        if (collision.transform.root == transform.root) return;

        PartInstance pi = GetComponent<PartInstance>();
        VehicleInstance viParent = transform.root.GetComponent<VehicleInstance>();
        Rigidbody rb = transform.root.GetComponent<Rigidbody>();

        if (rb == null) return;
        if (rb.velocity.magnitude <= def.explosionVelocity) return;

        pi.Detach();

        for (int i = 0; i < pi.LocalAttachNodes.Count; i++) {
            // TODO: Unify this stuff into a single function it's repeated from decouple behaviour

            if (pi.LocalAttachNodes[i] == null) continue;
            if (pi.LocalAttachNodes[i].CurrentOccupant == null) continue;

            PartInstance childPI = pi.LocalAttachNodes[i].CurrentOccupant;
            childPI.Detach();

            VehicleInstance childVI = VehicleSpawner.instance.MakeNewParent(childPI);

            Rigidbody childRB = childVI.GetComponent<Rigidbody>();
            childRB.velocity = rb.velocity;

            childVI.UpdateParts();
            childVI.RecalculateMass();
        }

        viParent.UpdateParts();
        viParent.RecalculateMass();

        for (int i = 0; i < def.particles.Length; i++) {
            GameObject go = Instantiate(def.particles[i], transform.position, Quaternion.identity);
            Destroy(go, 10);
        }
        Destroy(gameObject);
    }
}
