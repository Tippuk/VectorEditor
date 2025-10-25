using VectorEditor.Models;
using System.Windows.Media;

namespace VectorEditor.Commands
{
    public class PropertyChangeCommand : IEditorCommand
    {
        private BaseShape shape;
        private double oldX;
        private double oldY;
        private double oldW;
        private double oldH;
        private string oldFill;

        private double newX;
        private double newY;
        private double newW;
        private double newH;
        private string newFill;

        public PropertyChangeCommand(BaseShape shape,
            double oldX, double oldY, double oldW, double oldH, string oldFill,
            double newX, double newY, double newW, double newH, string newFill)
        {
            this.shape = shape;
            this.oldX = oldX;
            this.oldY = oldY;
            this.oldW = oldW;
            this.oldH = oldH;
            this.oldFill = oldFill;

            this.newX = newX;
            this.newY = newY;
            this.newW = newW;
            this.newH = newH;
            this.newFill = newFill;
        }

        public void Execute()
        {
            shape.X = newX;
            shape.Y = newY;
            shape.Width = newW;
            shape.Height = newH;
            try
            {
                shape.Fill = (Color)ColorConverter.ConvertFromString(newFill);
            }
            catch
            {
            }
        }

        public void Unexecute()
        {
            shape.X = oldX;
            shape.Y = oldY;
            shape.Width = oldW;
            shape.Height = oldH;
            try
            {
                shape.Fill = (Color)ColorConverter.ConvertFromString(oldFill);
            }
            catch
            {
            }
        }
    }
}
