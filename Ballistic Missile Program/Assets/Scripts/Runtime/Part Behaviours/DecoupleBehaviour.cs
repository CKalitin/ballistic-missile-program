using UnityEngine;

public class DecoupleBehaviour : MonoBehaviour {
    /// <summary>Consumes all parts whose stageIndex == CurrentStageIndex,
    /// spawns a new VehicleInstance for them, and bumps CurrentStageIndex.</summary>
    /*public VehicleInstance DecoupleCurrentStage(float separationImpulse = 0f) {
        // 1. Find departing parts
        var departing = Parts.Where(p => p.stageIndex == CurrentStageIndex).ToList();
        if (departing.Count == 0) { CurrentStageIndex++; return null; } // nothing to shed

        // 2. Create new root GO
        var newRoot = new GameObject($"Vehicle_Stage{CurrentStageIndex}");
        newRoot.transform.SetPositionAndRotation(transform.position, transform.rotation);

        // 3. Move parts under new root
        foreach (var part in departing)
            part.transform.SetParent(newRoot.transform, true);

        // 4. Add VehicleInstance + Rigidbody
        var vi = newRoot.AddComponent<VehicleInstance>();
        var rb = newRoot.AddComponent<Rigidbody>();       // compound from colliders

        vi.Parts.AddRange(departing);
        vi.blueprintGuid = blueprintGuid;
        vi.CurrentStageIndex = CurrentStageIndex;            // 'spent' stage index
        vi._rb = rb;
        vi.RecalculateMass();

        // 5. Clean up original vehicle
        Parts.RemoveAll(p => departing.Contains(p));
        CurrentStageIndex++;
        MarkMassDirty();   // update mass for the remaining core

        // 6. Give both stacks their inherited momentum
        rb.linearVelocity = _rb.linearVelocity;
        rb.angularVelocity = _rb.angularVelocity;

        // 7. Separation impulse (optional)
        if (separationImpulse != 0f) {
            Vector3 dir = -transform.forward;   // tweak as desired
            _rb.AddForce(dir * separationImpulse, ForceMode.Impulse);
            rb.AddForce(-dir * separationImpulse, ForceMode.Impulse);
        }

        return vi;    // caller can register it with game-manager if needed
    }*/
}
