using System.Collections.Generic;
using RinaInput.Controller.Module;
using R3;
using UnityEngine;
using RinaInput.Operators;
using System.Linq;
using Sirenix.Serialization;
using Sirenix.OdinInspector;

namespace RinaInput.Controller.Command
{
    [CreateAssetMenu(menuName = "RinaInput/コマンド/非精密同時入力")]
    public class NonIntervalChord : AInputCommand
    {
        [OdinSerialize]
        [LabelText("同時入力するモジュール")]
        private List<IInputModule<float>> m_modules = new();

        protected override Observable<Unit> CreateStream()
        {
            if (m_modules.Count is 0 || m_modules is null) {
                Debug.Log("ストリームの生成時に要素がありませんでした");
                return Observable.Empty<Unit>();
            }

            var streamWithIndex = m_modules
                //モジュールとそのインデックスにストリームを変更する
                .Select((module, index) => module.Stream.Select(signal => (signal, index)))
                .ToList();

            return Observable
                .Merge(streamWithIndex)
                //同時に流れた際は一つを受け取る
                //.Take(1)
                //最初に押されたストリームとその他を分離して、その他を引数としてChordInIntervalを使用
                .SelectMany(firstPressed =>
                {
                    //最初に入力されたストリーム
                    var sourceStream = m_modules[firstPressed.index].Stream;
                    //他に入力されたストリーム
                    var others = m_modules
                        //最初のストリームを分離
                        .Where((_, index) => index != firstPressed.index)
                        //ストリームのみを抽出
                        .Select(module => module.Stream)
                        .ToArray();
                    
                    return sourceStream
                        .Chord(others);
                });
        }
    }
}