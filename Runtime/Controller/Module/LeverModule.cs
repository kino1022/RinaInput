using R3;
using RinaInput.Lever.Operator;
using RinaInput.Lever.Signal;
using RinaInput.Signal;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RinaInput.Controller.Module {
    [CreateAssetMenu(menuName = "RinaInput/モジュール/レバー")]
    public class LeverModule : AInputModule<Vector2>
    {

        [SerializeField]
        [LabelText("デットゾーン")]
        [ProgressBar(0.0f, 1.0f)]
        private float m_deadZone = 0.3f;

        protected override Observable<InputSignal<Vector2>> InputContext(Observable<InputSignal<Vector2>> stream) {
            return stream
                .Select(signal => {
                    // Canceled フェーズ、または値がデッドゾーン内なら Vector2.zero を返す
                    if (signal.Phase == InputActionPhase.Canceled ||
                        signal.Value.sqrMagnitude < m_deadZone * m_deadZone) {
                        // 新しいInputSignalを作成して返す（元のTimeは保持しても良い）
                        return new InputSignal<Vector2>(signal.Phase, Vector2.zero, signal.Time);
                    }

                    // デッドゾーン外なら元の値をそのまま返す
                    return signal;
                });
        }
    }
}