using System.Windows.Media;
using System.Windows.Shapes;

namespace VectorEditor.Models
{
    public class EllipseShape : BaseShape
    {
        public override Shape ToUIElement()
        {
            var e = new Ellipse
            {
                Width = Width,
                Height = Height,
                Fill = new SolidColorBrush(Fill),
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness
            };
            return e;
        }
    }
}
