using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BallisticMissileProgram/Part Behaviours/Engine")]
public class EngineDefinition : PartBehaviourDefinition {
    public float maxThrustNewtons;
    public Vector2 maxGimbalAngles;

    public override float GetMassContribution() => 0f;
}