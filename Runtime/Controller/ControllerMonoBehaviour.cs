using System;
using System.Collections.Generic;
using RinaInput.Controller.Command;
using RinaInput.Controller.Module;
using RinaInput.Provider;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;

namespace RinaInput.Controller {
    [DefaultExecutionOrder(-4000)]
    public class ControllerMonoBehaviour : SerializedMonoBehaviour {

        [TitleGroup("入力モジュール")]
        [OdinSerialize]
        [LabelText("ボタン入力モジュール")]
        private List<IInputModule<float>> m_buttonModules = new();
        
        [TitleGroup("入力モジュール")]
        [OdinSerialize]
        [LabelText("レバー入力モジュール")]
        private List<IInputModule<Vector2>> m_leverModules = new();
        
        [TitleGroup("コマンド")]
        [OdinSerialize]
        [LabelText("有効化コマンド")]
        private List<IInputCommand> m_commands = new();
        
        [TitleGroup("参照")]
        [OdinSerialize]
        [LabelText("ストリーム供給クラス")]
        protected IInputStreamProvider m_streamProvider;
        
        protected IObjectResolver m_resolver;

        [Inject]
        public void Construct(IObjectResolver resolver) {
            
            m_resolver = resolver ?? throw new ArgumentNullException();
            
            m_streamProvider = m_resolver.Resolve<IInputStreamProvider>() 
                               ?? throw new NullReferenceException();
            
        }
        
        protected void Start() {
            
            m_buttonModules?.ForEach(x => {
                x?.Start();
                x?.GenerateStream(m_streamProvider);
            });

            m_leverModules?.ForEach(x => {
                x?.Start();
                x?.GenerateStream(m_streamProvider);
            });
            
            m_commands?.ForEach(x => {
                x?.GenerateStream();
            });
            
        }
    }
}