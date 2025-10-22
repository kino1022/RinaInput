using System;
using R3;
using RinaInput.Controller.Command;
using RinaInput.Controller.Command.Interface;
using RinaInput.Controller.Module;
using RinaInput.Operators;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace RinaInput.Controller.Command.Button {
    [CreateAssetMenu(menuName = "RinaInput/コマンド/長押し")]
    public class Hold : AInputCommand, IPropertyCommand<float> {

        [OdinSerialize]
        [LabelText("長押しするモジュール")]
        private IInputModule<float> m_module;
        
        [SerializeField]
        [LabelText("長押し時間")]
        private float m_holdTime;
        
        private Observable<float> m_propertyStream;
        
        public Observable<float> PropertyStream => m_propertyStream;
        
        
        protected override Observable<Unit> CreateStream() {

            m_propertyStream = m_module
                .Stream
                .Hold(TimeSpan.FromSeconds(m_holdTime))
                .Select(x => (float)x.Time);
            
            return m_module
                .Stream
                .Hold(TimeSpan.FromSeconds(m_holdTime))
                .Select(_ => Unit.Default);
            
        }
    }
}