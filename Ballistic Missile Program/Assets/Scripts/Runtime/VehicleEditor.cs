using UnityEngine;

public class VehicleEditor : MonoBehaviour {
    [SerializeField] private VehicleBlueprint bp;
    [SerializeField] private Vector3 defaultPosition;
    [SerializeField] private int count;

    void Start() {
        for (int i = 0; i < count; i++) {
            VehicleSpawner.instance.Build(bp, defaultPosition + new Vector3(0, i*30, 0), Quaternion.identity);
        }
    }   
}
