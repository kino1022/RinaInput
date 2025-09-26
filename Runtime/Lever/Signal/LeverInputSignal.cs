using RinaInput.Runtime.Signal;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RinaInput.Lever.Signal {
    /// <summary>
    /// レバー入力を表現するためのInputSignal
    /// </summary>
    /// <param name="Phase">入力の状態</param>
    /// <param name="Value">入力されている方向</param>
    /// <param name="Time">入力され続けている時間</param>
    public record LeverInputSignal(InputActionPhase Phase, Vector2 Value, double Time)
        : InputSignal<Vector2>(Phase, Value, Time);
}