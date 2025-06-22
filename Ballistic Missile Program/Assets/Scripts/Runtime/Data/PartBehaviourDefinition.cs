using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PartBehaviourDefinition : ScriptableObject {
    public virtual float GetMassContribution() => 0f;
}
