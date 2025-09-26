using UnityEngine.InputSystem;
using System.Runtime.CompilerServices;

namespace RinaInput.Runtime.Signal {
    public record InputSignal<T>(InputActionPhase Phase, T Value, double Time) where T : struct;
}