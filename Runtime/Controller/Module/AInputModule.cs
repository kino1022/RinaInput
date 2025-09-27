using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using RinaInput.Controller.Command;
using RinaInput.Operators;
using RinaInput.Runtime.Provider;
using RinaInput.Signal;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace RinaInput.Controller.Module {
    public abstract class AInputModule<T> : SerializedScriptableObject, IInputModule<T>, IStartable, IDisposable where T : struct {

        /// <summary>
        /// 入力自体に優先度を定義して管理すると、全体の動きに遅延が入るのと同時に、デザイナーの自由度が低下するので
        /// 入力データを送った先で管理するようにする
        /// よってキャンセル関連のストリーム処理は廃して単純な入力処理のみに注力する
        /// </summary>

        private InputActionReference m_actionRef;
        
        protected Observable<InputSignal<T>> m_stream;
        
        
        public Observable<InputSignal<T>> Stream => m_stream;

        public void Start() {
            m_actionRef?.action?.Enable();
        }

        public void Dispose() {
            m_actionRef?.action?.Disable();
        }

        public void GenerateStream(IInputStreamProvider provider) {

            var stream = provider?.GetStream<T>(m_actionRef) ?? throw new NullReferenceException("Stream is null");

            InputContext(stream);
        }
        
        /// <summary>
        /// m_streamの流れる条件を記述するメソッド
        /// </summary>
        /// <param name="stream">モジュール自体の作動ストリーム</param>
        /// <returns></returns>
        protected abstract Observable<InputSignal<T>> InputContext (Observable<InputSignal<T>> stream);
        
    }
}