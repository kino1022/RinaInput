using RinaInput.Controller.Command;

namespace RinaInput.Controller.Interface {
    public interface IController {
        
        void AddCommand(IInputCommand command);
        
        void RemoveCommand(IInputCommand command);
    }
}