using R3;
using RinaInput.Lever.Signal;
using RinaInput.Operators;
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
                .OnMove(m_deadZone);
        }
    }
}