using R3;
using RinaInput.Runtime.Signal;
using UnityEngine.InputSystem;

namespace RinaInput.Runtime.Provider {
    public interface IInputStreamProvider {
        
        Observable<InputSignal<T>> GetStream<T> (InputAction action) where T : struct;
        
        Observable<InputSignal<T>> GetStream<T> (InputActionReference actionReference) where T : struct;
    }
}