using R3;
using RinaInput.Controller.Module;
using UnityEngine;

namespace RinaInput.Operators.Position {
    
    public static partial class InputStreamOperator {
        
        /// <summary>
        /// 特定の振り幅でPositionModuleを振った際に出力されるストリーム
        /// </summary>
        /// <param name="module"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static Observable<Unit> OnSwing (this IInputModule<Vector3> module, float threshold) {
            
            return module
                .SwingSpeed()
                .Where(speed => speed >= threshold)
                .AsUnitObservable();
            
        }
        
        /// <summary>
        /// 三次元機器を振った際のスピードを出力するストリーム
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public static Observable<float> SwingSpeed (this IInputModule<Vector3> module) {

            return module
                .Stream
                .Pairwise()
                .Select(x =>
                    (float)(Vector3.Distance(x.Current.Value, x.Previous.Value) / (x.Current.Time - x.Previous.Time)));
        }
        
    }
}