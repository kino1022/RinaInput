using R3;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace RinaInput.Controller.Command {
    public abstract class AInputCommand : SerializedScriptableObject, IInputCommand {

        private ReactiveProperty<bool> _isEnable = new ReactiveProperty<bool>(true);

        /// <summary>
        /// 最終的に流れるストリーム
        /// </summary>
        [OdinSerialize]
        [ReadOnly]
        private Observable<Unit> m_stream;

        /// <summary>
        /// このコマンドの入力に許された猶予
        /// </summary>
        [SerializeField]
        [LabelText("入力猶予(秒)")]
        [ProgressBar(0, 10)]
        protected float m_inputGrace = 0;

        public Observable<Unit> Stream => m_stream.Where(_ => _isEnable.CurrentValue);

        public float InputGrace => m_inputGrace;
        
        public ReadOnlyReactiveProperty<bool> IsEnable => _isEnable;

        private void OnValidate()
        {
            if (m_inputGrace < 0) Debug.Log($"{GetType().Name}の入力猶予が0秒です、入力が不可能なので見直してください");
        }

        public virtual void GenerateStream()
        {
            Debug.Log("ストリームの生成処理を開始します");
            m_stream = CreateStream();
            Debug.Log("ストリームの生成処理を終了しました");
        }
        

        protected abstract Observable<Unit> CreateStream();

        public void ChangeEnable(bool isEnable) => _isEnable.Value = isEnable;

    }
}