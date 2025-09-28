using UnityEngine;
using R3;
using RinaInput.Controller.Module;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using System;

namespace RinaInput.Controller.Command
{
    /// <summary>
    /// 特定のボタンが押されているかどうかと押されている間の指定したスティックの方向を示すコマンド
    /// </summary>
    [CreateAssetMenu(menuName = "RinaInput/コマンド/サークルメニュー")]
    public class SelectCircleMenu : APropertyCommand<Vector2>
    {

        [OdinSerialize]
        [LabelText("ボタンモジュール")]
        private IInputModule<float> m_buttonModule;

        [OdinSerialize]
        [LabelText("レバーモジュール")]
        private IInputModule<Vector2> m_leverModule;

        protected override Observable<Unit> CreateStream()
        {
            return m_buttonModule
                .Stream
                .Select(_ => Unit.Default);
        }

        protected override Observable<Vector2> CreateProperty()
        {
            //ソースが流れている場合にtrueを流すストリーム
            Observable<bool> trueGate = Stream.Select(_ => true);
            //ソースが流れていない状態が1フレーム続くとfalseを流すストリーム
            Observable<bool> falseGate = Stream
                .Timeout(TimeSpan.FromTicks(1))
                .Select(_ => false);
            
            //二つのストリームを統合してメインストリームの状態を示すストリーム
            Observable<bool> gateStream = Observable.Merge(trueGate, falseGate);

            //レバーモジュールのストリームに対してボタンが入力されているかどうかの制御を加える
            return m_leverModule
                .Stream
                //ボタンの状態とのタプル型を制作
                .WithLatestFrom(gateStream, (sourceValue, isActive) => (sourceValue, isActive))
                //タプルの真偽値でフィルタ
                .Where(pair => pair.isActive)
                //タプルの値部分だけ放流
                .Select(x => x.sourceValue.Value);
        }
    }
}