using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VectorEditor.Models
{
    public class PolygonShape : BaseShape
    {
        public int Sides { get; set; } = 5;

        public override Shape ToUIElement()
        {
            var poly = new Polygon();
            var pts = new PointCollection();
            double cx = Width / 2.0;
            double cy = Height / 2.0;
            double r = Math.Min(Width, Height) / 2.0;
            for (int i = 0; i < Sides; i++)
            {
                double a = 2 * Math.PI * i / Sides - Math.PI / 2;
                pts.Add(new Point(cx + r * Math.Cos(a), cy + r * Math.Sin(a)));
            }

            poly.Points = pts;
            poly.Fill = new SolidColorBrush(Fill);
            poly.Stroke = new SolidColorBrush(StrokeColor);
            poly.StrokeThickness = StrokeThickness;
            return poly;
        }
    }
}
