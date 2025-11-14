using R3;

namespace RinaInput.Controller.Command {
    public interface IInputCommand
    {
        /// <summary>
        /// コマンドの入力が有効化どうか
        /// </summary>
        ReadOnlyReactiveProperty<bool> IsEnable { get; }

        /// <summary>
        /// 設ける入力猶予(ms)
        /// </summary>
        float InputGrace { get; }

        /// <summary>
        /// 入力が成功した際に流れるストリーム
        /// </summary>
        Observable<Unit> Stream { get; }

        /// <summary>
        /// ストリームの生成を行う
        /// </summary>
        void GenerateStream();
        
        /// <summary>
        /// コマンドが有効かどうかを切り替える
        /// </summary>
        /// <param name="isEnable"></param>
        void ChangeEnable(bool isEnable);
        
    }
}