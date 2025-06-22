using UnityEngine;

public class VehicleEditor : MonoBehaviour {
    [SerializeField] private VehicleBlueprint bp;

    void Start() {
        FindObjectOfType<VehicleSpawner>().Build(bp, new Vector3(0, 0, 0), Quaternion.identity);
    }   
}
