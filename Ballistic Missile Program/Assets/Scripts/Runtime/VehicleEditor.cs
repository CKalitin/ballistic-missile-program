using UnityEngine;

public class VehicleEditor : MonoBehaviour {
    [SerializeField] private VehicleBlueprint bp;

    void Start() {
        VehicleSpawner.Build(bp, new Vector3(0, 0, 0), Quaternion.identity);
    }   
}
