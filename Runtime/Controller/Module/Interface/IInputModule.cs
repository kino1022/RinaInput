using R3;
using RinaInput.Controller.Command;
using RinaInput.Runtime.Provider;
using RinaInput.Signal;

namespace RinaInput.Controller.Module {
    /// <summary>
    /// ボタンやレバーなどの入力装置を表現するクラスに対して約束するメソッド
    /// </summary>
    /// <typeparam name="T">モジュールの入力値に使うデータ型</typeparam>
    public interface IInputModule<T> where T : struct
    {

        /// <summary>
        /// 入力が成功した際に流れるストリーム
        /// </summary>
        Observable<InputSignal<T>> Stream { get; }

        /// <summary>
        ///　ストリームの生成を行う
        /// </summary>
        /// <param name="provider"></param>
        void GenerateStream(IInputStreamProvider provider);

    }
}