using System.Collections.Generic;
using System.Linq;
using R3;
using RinaInput.Controller.Command;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;

namespace System.Runtime.CompilerServices.Controller.Command {
    public abstract class AInputCommand : SerializedScriptableObject, IInputCommand
    {

        private Observable<Unit> m_stream;

        [SerializeField]
        [LabelText("入力猶予")]
        [ProgressBar(0, 10000)]
        private int m_inputGrace = 0;

        [OdinSerialize]
        [LabelText("入力待受キャンセルコマンド")]
        private List<IInputCommand> m_cancelCommands = new List<IInputCommand>();

        protected Observable<Unit> m_cancelStream;

        public Observable<Unit> Stream => m_stream;

        public int InputGrace => m_inputGrace;

        public void GenerateStream()
        {
            m_cancelStream = CreateCancelStream();
            m_stream = CreateStream();
        }

        private Observable<Unit> CreateCancelStream()
        {
            var result = Observable.ReturnUnit().Share();
            if (m_cancelCommands.Count is 0) return result;
            m_cancelCommands.Select(x => x.Stream).ForEach(x => x.Merge(x));
            return result;
        }

        protected abstract Observable<Unit> CreateStream();
        
    }
}