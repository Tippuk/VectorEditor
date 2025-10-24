using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VectorEditor.Models;

namespace VectorEditor
{
    public partial class MainWindow : Window
    {
        private List<BaseShape> shapes;
        private bool isDrawing = false;
        private bool isMoving = false;
        private Point startPoint;
        private Point moveStart;
        private BaseShape selectedShape;
        private BaseShape currentDrawingShape;
        private string currentShapeType = "Rect";
        private Color currentColor = Colors.LightBlue;

        public MainWindow()
        {
            InitializeComponent();
            shapes = new List<BaseShape>();

            DrawingCanvas.MouseLeftButtonDown += Canvas_MouseDown;
            DrawingCanvas.MouseMove += Canvas_MouseMove;
            DrawingCanvas.MouseLeftButtonUp += Canvas_MouseUp;
        }

        private void RedrawCanvas()
        {
            DrawingCanvas.Children.Clear();
            foreach (BaseShape shape in shapes)
            {
                Shape ui = shape.ToUIElement();
                if (shape == selectedShape)
                    ui.StrokeDashArray = new DoubleCollection() { 4, 2 };

                Canvas.SetLeft(ui, shape.X);
                Canvas.SetTop(ui, shape.Y);
                DrawingCanvas.Children.Add(ui);
            }

            UpdateInspector();
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(DrawingCanvas);
            selectedShape = null;
            isMoving = false;
            isDrawing = false;

            // 🔹 Проверяем, попали ли по существующей фигуре (сверху вниз)
            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                BaseShape s = shapes[i];
                if (s.Contains(pos))
                {
                    selectedShape = s;
                    isMoving = true;
                    moveStart = pos;

                    // Помещаем выбранную фигуру наверх (в конец списка)
                    shapes.Remove(s);
                    shapes.Add(s);

                    RedrawCanvas();
                    return;
                }
            }

            // 🔹 Если не попали — начинаем рисование новой фигуры
            isDrawing = true;
            startPoint = pos;

            if (currentShapeType == "Ellipse")
                currentDrawingShape = new EllipseShape();
            else if (currentShapeType == "Polygon")
                currentDrawingShape = new PolygonShape();
            else
                currentDrawingShape = new RectangleShape();

            currentDrawingShape.X = startPoint.X;
            currentDrawingShape.Y = startPoint.Y;
            currentDrawingShape.Width = 0;
            currentDrawingShape.Height = 0;
            currentDrawingShape.Fill = currentColor;

            shapes.Add(currentDrawingShape);
            RedrawCanvas();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point pos = e.GetPosition(DrawingCanvas);

            // 🔸 Рисование
            if (isDrawing && currentDrawingShape != null && e.LeftButton == MouseButtonState.Pressed)
            {
                currentDrawingShape.X = Math.Min(startPoint.X, pos.X);
                currentDrawingShape.Y = Math.Min(startPoint.Y, pos.Y);
                currentDrawingShape.Width = Math.Abs(pos.X - startPoint.X);
                currentDrawingShape.Height = Math.Abs(pos.Y - startPoint.Y);
                RedrawCanvas();
            }

            // 🔸 Перемещение
            if (isMoving && selectedShape != null && e.LeftButton == MouseButtonState.Pressed)
            {
                double dx = pos.X - moveStart.X;
                double dy = pos.Y - moveStart.Y;
                selectedShape.X += dx;
                selectedShape.Y += dy;
                moveStart = pos;
                RedrawCanvas();
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Завершаем действие
            isDrawing = false;
            isMoving = false;
            currentDrawingShape = null;
        }

        private void RectButton_Click(object sender, RoutedEventArgs e)
        {
            currentShapeType = "Rect";
        }

        private void EllipseButton_Click(object sender, RoutedEventArgs e)
        {
            currentShapeType = "Ellipse";
        }

        private void PolygonButton_Click(object sender, RoutedEventArgs e)
        {
            currentShapeType = "Polygon";
        }

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = ColorComboBox.SelectedItem as ComboBoxItem;
            if (item != null)
            {
                currentColor = (Color)ColorConverter.ConvertFromString(item.Content.ToString());
            }
        }

        private void ApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            if (selectedShape == null) return;
            double x, y, w, h;
            if (double.TryParse(XBox.Text, out x)) selectedShape.X = x;
            if (double.TryParse(YBox.Text, out y)) selectedShape.Y = y;
            if (double.TryParse(WidthBox.Text, out w)) selectedShape.Width = w;
            if (double.TryParse(HeightBox.Text, out h)) selectedShape.Height = h;

            try
            {
                selectedShape.Fill = (Color)ColorConverter.ConvertFromString(FillBox.Text);
            }
            catch { }

            RedrawCanvas();
        }

        private void UpdateInspector()
        {
            if (selectedShape == null)
            {
                XBox.Text = "";
                YBox.Text = "";
                WidthBox.Text = "";
                HeightBox.Text = "";
                FillBox.Text = "";
                return;
            }

            XBox.Text = selectedShape.X.ToString("F0");
            YBox.Text = selectedShape.Y.ToString("F0");
            WidthBox.Text = selectedShape.Width.ToString("F0");
            HeightBox.Text = selectedShape.Height.ToString("F0");
            FillBox.Text = selectedShape.Fill.ToString();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Delete && selectedShape != null)
            {
                shapes.Remove(selectedShape);
                selectedShape = null;
                RedrawCanvas();
            }
        }
    }
}
