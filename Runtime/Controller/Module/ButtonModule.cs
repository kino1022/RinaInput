using UnityEngine;
using System;
using R3;
using RinaInput.Signal;

namespace RinaInput.Controller.Module
{
    public class ButtonModule : AInputModule<float>
    {

        [SerializeField]
        [LabelText("キャンセル入力待機時間(ms)")]
        private int m_cancelGrace = 0;

        protected override Observable<InputSignal<float>> InputContext(Observable<InputSignal<float>> stream)
        {
            return stream
                    .SelectMany(x =>
                    {
                        return Observable
                            .ReturnUnit()
                            .Delay(TimeSpan.FromMilliseconds(m_cancelGrace))
                            .TakeUntil(m_cancelStream);
                    });
        }

    }
}