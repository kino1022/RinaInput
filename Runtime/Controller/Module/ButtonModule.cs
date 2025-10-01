using UnityEngine;
using System;
using R3;
using RinaInput.Operators;
using RinaInput.Signal;
using Sirenix.OdinInspector;

namespace RinaInput.Controller.Module
{
    [CreateAssetMenu(menuName = "RinaInput/モジュール/ボタン")]
    public class ButtonModule : AInputModule<float>
    {
        [SerializeField]
        [LabelText("デットゾーン")]
        [ProgressBar(0.0f, 1.0f)]
        private float m_deadZone = 0.2f;

        protected override Observable<InputSignal<float>> InputContext(Observable<InputSignal<float>> stream) {
            return stream
                .OnPressed()
                //デットゾーン未満の入力を除外
                .Where(x => x.Value > m_deadZone);
        }

    }
}