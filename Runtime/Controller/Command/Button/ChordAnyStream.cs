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

            return Observable
                .Merge(streamWithIndex)
                //同時に流れた際は一つを受け取る
                //.Take(1)
                //最初に押されたストリームとその他を分離して、その他を引数としてChordInIntervalを使用
                .SelectMany(firstPressed =>
                {
                    //最初に入力されたストリーム
                    var sourceStream = m_modules[firstPressed.index]
                        .Stream;
                    
                    //他に入力されたストリーム
                    var others = m_modules
                        //最初のストリームを分離
                        .Where((_, index) => index != firstPressed.index)
                        //ストリームのみを抽出
                        .Select(module => module.Stream)
                        //配列に
                        .ToArray();

                    return sourceStream
                        .ChordInInterval(
                        TimeSpan.FromSeconds(m_inputGrace),
                        others
                        );
                });
        }
    }
}