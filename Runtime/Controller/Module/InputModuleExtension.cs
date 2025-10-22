using System.Collections.Generic;
using System.Linq;
using R3;
using RinaInput.Signal;

namespace RinaInput.Controller.Module {
    public static class InputModuleExtension {

        /// <summary>
        /// モジュールのリストをシグナルとインデックスのストリームのリストに変換して提供する
        /// </summary>
        /// <param name="modules"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Observable<(InputSignal<T> signal, int index)>> StreamWithIndex<T>(this List<IInputModule<T>> modules)
            where T : struct 
        {

            if (modules is null || modules.Count is 0) {
                throw new System.ArgumentNullException(nameof(modules));
            }
            
            return modules
                .Select((module, index) => 
                    module
                        .Stream
                        .Select(signal => (signal, index))
                )
                .ToList();
        }
    }
}