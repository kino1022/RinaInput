using R3;
using RinaInput.Signal;
using UnityEngine;

namespace RinaInput.Controller.Module {
    /// <summary>
    /// VR機器などの三次元座標を入力として扱うモジュール
    /// </summary>
    [CreateAssetMenu(menuName = "RinaInput/モジュール/三次元座標")]
    public class PositionModule : AInputModule<Vector3> {
        protected override Observable<InputSignal<Vector3>> InputContext(Observable<InputSignal<Vector3>> stream) {
            return stream
                .DistinctUntilChanged();
        }
    }
}