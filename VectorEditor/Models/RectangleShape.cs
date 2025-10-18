using System.Windows.Media;
using System.Windows.Shapes;

namespace VectorEditor.Models
{
    public class RectangleShape : BaseShape
    {
        public override Shape ToUIElement()
        {
            var r = new Rectangle
            {
                Width = Width,
                Height = Height,
                Fill = new SolidColorBrush(Fill),
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness
            };
            return r;
        }
    }
}
