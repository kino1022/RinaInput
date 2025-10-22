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

            if (m_count <= 0) {
                Debug.Log("連打の入力回数が0以下でした、ストリームの生成を中断します");
                return Observable.Empty<Unit>();
            }

            if (m_module is null) {
                Debug.Log("入力モジュールの指定が存在しませんでした、ストリームの生成を中断します");
                return Observable.Return(Unit.Default);
            }
            
            return m_module
                .Stream
                .TapInSpan(
                    m_count,
                    TimeSpan.FromSeconds(InputGrace)
                );
        }
    }
}