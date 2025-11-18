using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using RinaInput.Controller.Module;
using RinaInput.Operators;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace RinaInput.Controller.Command.Button {
    [CreateAssetMenu(menuName = "RinaInput/コマンド/時間制限付き同時押し")]
    public class ChordAnyStream : AInputCommand
    {

        [OdinSerialize]
        [LabelText("同時押しするモジュール")]
        private List<IInputModule<float>> m_modules = new();

        protected override Observable<Unit> CreateStream()
        {

            if (m_modules.Count is 0 || m_modules is null) {
                Debug.Log("同時入力に利用するモジュールが指定されていませんでした、ストリームの生成を中断します");
                return Observable.Empty<Unit>();
            }

            var streamWithIndex = m_modules
                //モジュールとそのインデックスにストリームを変更する
                .StreamWithIndex();

            return m_modules
                .Select(x => x.Stream)
                .ChordInIntervalByEdge(TimeSpan.FromSeconds(m_inputGrace));
        }
    }
}