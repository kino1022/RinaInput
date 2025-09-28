using R3;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RinaInput.Controller.Command {
    public abstract class AInputCommand : SerializedScriptableObject, IInputCommand
    {

        /// <summary>
        /// 最終的に流れるストリーム
        /// </summary>
        private Observable<Unit> m_stream;

        /// <summary>
        /// このコマンドの入力に許された猶予
        /// </summary>
        [SerializeField]
        [LabelText("入力猶予")]
        [ProgressBar(0, 10000)]
        protected int m_inputGrace = 0;

        public Observable<Unit> Stream => m_stream;

        public int InputGrace => m_inputGrace;

        private void Oalidate()
        {
            if (m_inputGrace < 0) Debug.Log($"{GetType().Name}の入力猶予が0ミリ秒です、入力が不可能なので見直してください");
        }

        public virtual void GenerateStream()
        {
            m_stream = CreateStream();
        }
        

        protected abstract Observable<Unit> CreateStream();
        
    }
}