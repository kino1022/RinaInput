using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using RinaInput.Controller.Command;
using RinaInput.Runtime.Operators;
using RinaInput.Runtime.Provider;
using RinaInput.Runtime.Signal;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace RinaInput.Controller.Module {
    public abstract class AInputModule<T> : SerializedScriptableObject, IInputModule<T>, IStartable, IDisposable where T : struct {

        private InputActionReference m_actionRef;
        
        protected Observable<InputSignal<T>> m_stream;

        [OdinSerialize]
        [LabelText("キャンセル用ストリーム")]
        private List<IInputCommand> m_commands = new();
        
        protected Observable<Unit> m_cancelStream;
        
        public Observable<InputSignal<T>> Stream => m_stream;

        public void Start() {
            m_actionRef?.action?.Enable();

            m_cancelStream = CreateCancelStream();
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

        public void RegisterCancelStream(IInputCommand cancelCommand) {
            
            if (cancelCommand is null) {
                throw new ArgumentNullException(nameof(cancelCommand));
            }
            
            m_commands.Add(cancelCommand);
            
        }

        /// <summary>
        /// キャンセル用ストリームを登録解除する
        /// </summary>
        /// <param name="cancelCommand"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void DisRegisterCancelStream(IInputCommand cancelCommand) {
            
            if (cancelCommand is null) {
                throw new ArgumentNullException(nameof(cancelCommand));
            }
            
            m_commands.Remove(cancelCommand);
        }

        private Observable<Unit> CreateCancelStream() {
            var result = Observable.ReturnUnit().Share();
            
            if (m_commands is null || m_commands.Count is 0) {
                return result;
            }

            m_commands.Select(x => x.Stream).ForEach(x => result.Merge(x));
            
            return result;
        }
    }
}