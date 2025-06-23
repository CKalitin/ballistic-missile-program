using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAeroSurface {
    float Area { get; }
    AnimationCurve LiftCurve { get; }
    AnimationCurve DragCurve { get; }

}
