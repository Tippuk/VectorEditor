using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VectorEditor.Models;

namespace VectorEditor
{
    public partial class MainWindow : Window
    {
        private List<BaseShape> shapes;

        public MainWindow()
        {
            InitializeComponent();
            shapes = new List<BaseShape>();
            RedrawCanvas();
        }

        private void RedrawCanvas()
        {
            DrawingCanvas.Children.Clear();
            foreach (var shape in shapes)
            {
                Shape ui = shape.ToUIElement();
                Canvas.SetLeft(ui, shape.X);
                Canvas.SetTop(ui, shape.Y);
                DrawingCanvas.Children.Add(ui);
            }
        }
    }
}
