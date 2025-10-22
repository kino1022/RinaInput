using R3;

namespace RinaInput.Controller.Command {
    public interface IInputCommand
    {

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
        
    }
}