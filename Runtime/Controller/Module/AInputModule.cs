using System;
using UnityEngine;
using R3;
using RinaInput.Provider;
using RinaInput.Signal;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace RinaInput.Controller.Module {
    public abstract class AInputModule<T> : ScriptableObject, IInputModule<T>, IDisposable where T : struct {

        [SerializeField]
        [LabelText("入力ソース")]
        private InputActionReference m_actionRef;

        private ReactiveProperty<bool> _isEnable = new(true);
        
        protected Observable<InputSignal<T>> m_stream;

        public Observable<InputSignal<T>> Stream => m_stream != null
            ? m_stream.Where(_ => _isEnable.CurrentValue == true)
            : Observable.Empty<InputSignal<T>>();
        
        public ReadOnlyReactiveProperty<bool> IsEnable => _isEnable;


        public void Start() {
            m_actionRef?.action?.Enable();
        }

        public void Dispose() {
            m_actionRef?.action?.Disable();
        }

        public void GenerateStream(IInputStreamProvider provider) {

            var stream = provider
                .GetStream<T>(m_actionRef) ?? throw new NullReferenceException("Stream is null");

            m_stream = InputContext(stream);
            
        }
        
        public void ChangeEnable(bool isEnable) {
            _isEnable.Value = isEnable;
        }
        
        /// <summary>
        /// m_streamの流れる条件を記述するメソッド
        /// </summary>
        /// <param name="stream">モジュール自体の作動ストリーム</param>
        /// <returns></returns>
        protected abstract Observable<InputSignal<T>> InputContext (Observable<InputSignal<T>> stream);
        
    }
}