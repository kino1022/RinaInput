using R3;
using RinaInput.Controller.Module;
using RinaInput.Signal;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RinaInput.Operators {
    public static partial class InputStreamOperator {

        public static Observable<InputSignal<Vector2>> OnMove(this Observable<InputSignal<Vector2>> stream, float threshold = 0.2f) {
            return stream
                .Select(signal => {
                    if (signal.Phase == InputActionPhase.Canceled ||
                        signal.Value.sqrMagnitude < threshold * threshold) {
                        return new InputSignal<Vector2>(signal.Phase, Vector2.zero, signal.Time);
                    }

                    return signal;
                })
                .DistinctUntilChanged();
        }
    }
}