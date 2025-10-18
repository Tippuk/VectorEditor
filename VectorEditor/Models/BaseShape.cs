using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VectorEditor.Models
{
    public abstract class BaseShape
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Rotation { get; set; } = 0;
        public double Scale { get; set; } = 1.0;

        [JsonIgnore]
        public Color Fill { get; set; } = Colors.LightBlue;

        public double StrokeThickness { get; set; } = 1;
        public Color StrokeColor { get; set; } = Colors.Black;

        public abstract Shape ToUIElement();

        public virtual bool Contains(Point p)
        {
            return p.X >= X && p.X <= X + Width && p.Y >= Y && p.Y <= Y + Height;
        }
    }
}
