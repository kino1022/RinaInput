using R3;
using RinaInput.Controller.Command;
using RinaInput.Runtime.Provider;
using RinaInput.Runtime.Signal;

namespace RinaInput.Controller.Module {
    public interface IInputModule<T> where T : struct {
        
        /// <summary>
        /// 入力が成功した際に流れるストリーム
        /// </summary>
        Observable<InputSignal<T>> Stream { get; }
        
        /// <summary>
        ///　ストリームの生成を行う
        /// </summary>
        /// <param name="provider"></param>
        void GenerateStream(IInputStreamProvider provider);
        
        /// <summary>
        /// 同時押しなどで不都合が生じないようにキャンセル用のストリームを登録する
        /// </summary>
        /// <param name="cancelCommand"></param>
        void RegisterCancelStream (IInputCommand cancelCommand);
    }
}