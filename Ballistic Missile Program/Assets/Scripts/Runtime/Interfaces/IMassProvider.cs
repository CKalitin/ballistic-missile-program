using System;
using System.Collections.Generic;
using UnityEngine;

// Behaviours inherit this
// "I" is a naming convention for interface like IEnumerable
public interface IMassProvider {
    float CurrentMass { get; }
    event Action OnMassChanged; // Fire this whenever mass changes
}