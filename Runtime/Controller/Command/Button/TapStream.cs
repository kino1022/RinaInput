using System;
using R3;
using RinaInput.Controller.Module;
using RinaInput.Operators;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace RinaInput.Controller.Command.Button
{
    [CreateAssetMenu(menuName = "RinaInput/コマンド/連続入力")]
    public class TapStream : AInputCommand
    {
        [OdinSerialize]
        [LabelText("入力するモジュール")]
        private IInputModule<float> m_module;

        [SerializeField]
        [LabelText("入力回数")]
        [ProgressBar(0, 10)]
        private int m_count = 2;

        protected override Observable<Unit> CreateStream()
        {
            return m_module
                .Stream
                .TapInSpan(
                    m_count,
                    TimeSpan.FromMilliseconds(InputGrace)
                );
        }
    }
}