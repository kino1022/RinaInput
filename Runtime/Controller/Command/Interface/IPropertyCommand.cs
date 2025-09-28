using R3;

namespace RinaInput.Controller.Command.Interface
{
    /// <summary>
    /// 何かしらの付加情報が必要なコマンドの表現に使用するIInputCommand
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPropertyCommand<T> : IInputCommand where T : struct
    {
        /// <summary>
        /// 付加情報を含んだストリーム
        /// </summary>
        /// <value></value>
        Observable<T> PropertyStream { get; }
    }
}