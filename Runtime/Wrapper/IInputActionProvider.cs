using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace RinaInput.Wrapper.Interface {
    public interface IInputActionProvider : IEnumerable<InputAction> {
        void Enable();
        void Disable();
        InputAction FindAction(string actionNameOrId);
    }
}