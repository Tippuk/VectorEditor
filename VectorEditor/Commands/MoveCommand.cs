using VectorEditor.Models;

namespace VectorEditor.Commands
{
    public class MoveCommand : IEditorCommand
    {
        private BaseShape shape;
        private double oldX;
        private double oldY;
        private double newX;
        private double newY;

        public MoveCommand(BaseShape shape, double oldX, double oldY, double newX, double newY)
        {
            this.shape = shape;
            this.oldX = oldX;
            this.oldY = oldY;
            this.newX = newX;
            this.newY = newY;
        }

        public void Execute()
        {
            shape.X = newX;
            shape.Y = newY;
        }

        public void Unexecute()
        {
            shape.X = oldX;
            shape.Y = oldY;
        }
    }
}
