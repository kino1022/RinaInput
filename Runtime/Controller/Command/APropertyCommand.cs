using R3;
using RinaInput.Controller.Command.Interface;

namespace RinaInput.Controller.Command
{
    public abstract class APropertyCommand<T> : AInputCommand, IPropertyCommand<T> where T : struct {

        private Observable<T> m_property;

        public Observable<T> PropertyStream => m_property;
        

        public override void GenerateStream()
        {
            base.GenerateStream();

            m_property = CreateProperty();
        }
        
        /// <summary>
        /// 付加情報を含んだストリームの生成を行う
        /// </summary>
        /// <returns></returns>
        protected abstract Observable<T> CreateProperty();
        
    }
}