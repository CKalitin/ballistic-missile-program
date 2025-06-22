using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BallisticMissileProgram/Part Behaviours/Explosive")]
public class ExplosiveDefinition : PartBehaviourDefinition {
    public GameObject[] particles;

    public float explosionVelocity;

    public override float GetMassContribution() => 0f;
}