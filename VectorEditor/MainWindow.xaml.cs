using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using VectorEditor.Commands;
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
        private Point moveOriginalPos;
        private BaseShape selectedShape;
        private BaseShape currentDrawingShape;
        private string currentShapeType = "Rect";
        private Color currentColor = Colors.LightBlue;
        private const string DefaultSavePath = "drawing.json";

        private Stack<IEditorCommand> undoStack;
        private Stack<IEditorCommand> redoStack;

        public MainWindow()
        {
            InitializeComponent();
            shapes = new List<BaseShape>();
            undoStack = new Stack<IEditorCommand>();
            redoStack = new Stack<IEditorCommand>();

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

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                BaseShape s = shapes[i];
                if (s.Contains(pos))
                {
                    selectedShape = s;
                    isMoving = true;
                    moveStart = pos;
                    moveOriginalPos = new Point(s.X, s.Y);

                    shapes.RemoveAt(i);
                    shapes.Add(selectedShape);

                    RedrawCanvas();
                    return;
                }
            }

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

            if (isDrawing && currentDrawingShape != null && e.LeftButton == MouseButtonState.Pressed)
            {
                currentDrawingShape.X = Math.Min(startPoint.X, pos.X);
                currentDrawingShape.Y = Math.Min(startPoint.Y, pos.Y);
                currentDrawingShape.Width = Math.Abs(pos.X - startPoint.X);
                currentDrawingShape.Height = Math.Abs(pos.Y - startPoint.Y);
                RedrawCanvas();
            }

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
            if (isDrawing && currentDrawingShape != null)
            {
                if (currentDrawingShape.Width <= 0 || currentDrawingShape.Height <= 0)
                {
                    shapes.Remove(currentDrawingShape);
                }
                else
                {
                    IEditorCommand cmd = new CreateCommand(currentDrawingShape, shapes);
                    undoStack.Push(cmd);
                    redoStack.Clear();
                }
                currentDrawingShape = null;
            }

            if (isMoving && selectedShape != null)
            {
                Point newPos = new Point(selectedShape.X, selectedShape.Y);
                if (moveOriginalPos.X != newPos.X || moveOriginalPos.Y != newPos.Y)
                {
                    IEditorCommand cmd = new MoveCommand(selectedShape, moveOriginalPos.X, moveOriginalPos.Y, newPos.X, newPos.Y);
                    undoStack.Push(cmd);
                    redoStack.Clear();
                }
            }

            isDrawing = false;
            isMoving = false;
            RedrawCanvas();
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

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (undoStack.Count == 0) return;
            IEditorCommand cmd = undoStack.Pop();
            cmd.Unexecute();
            redoStack.Push(cmd);
            selectedShape = null;
            RedrawCanvas();
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            if (redoStack.Count == 0) return;
            IEditorCommand cmd = redoStack.Pop();
            cmd.Execute();
            undoStack.Push(cmd);
            selectedShape = null;
            RedrawCanvas();
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "JSON files (*.json)|*.json";
            dlg.FileName = "drawing.json";
            if (dlg.ShowDialog() == true)
            {
                SaveToJson(dlg.FileName);
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "JSON files (*.json)|*.json";
            if (dlg.ShowDialog() == true)
            {
                LoadFromJson(dlg.FileName);
                undoStack.Clear();
                redoStack.Clear();
            }
        }

        private void SaveToJson(string path)
        {
            List<ShapeData> data = new List<ShapeData>();
            foreach (BaseShape s in shapes)
            {
                ShapeData d = new ShapeData();
                d.Type = s.GetType().Name;
                d.X = s.X;
                d.Y = s.Y;
                d.Width = s.Width;
                d.Height = s.Height;
                d.Fill = s.Fill.ToString();
                data.Add(d);
            }

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        private void LoadFromJson(string path)
        {
            if (!File.Exists(path)) return;

            string json = File.ReadAllText(path);
            List<ShapeData> data = JsonSerializer.Deserialize<List<ShapeData>>(json);

            shapes.Clear();

            foreach (ShapeData d in data)
            {
                BaseShape shape;
                if (d.Type == "EllipseShape")
                    shape = new EllipseShape();
                else if (d.Type == "PolygonShape")
                    shape = new PolygonShape();
                else
                    shape = new RectangleShape();

                shape.X = d.X;
                shape.Y = d.Y;
                shape.Width = d.Width;
                shape.Height = d.Height;

                try
                {
                    shape.Fill = (Color)ColorConverter.ConvertFromString(d.Fill);
                }
                catch
                {
                    shape.Fill = Colors.LightBlue;
                }

                shapes.Add(shape);
            }

            RedrawCanvas();
        }


        private void ApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            if (selectedShape == null) return;

            double oldX = selectedShape.X;
            double oldY = selectedShape.Y;
            double oldW = selectedShape.Width;
            double oldH = selectedShape.Height;
            string oldFill = selectedShape.Fill.ToString();

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

            double newX = selectedShape.X;
            double newY = selectedShape.Y;
            double newW = selectedShape.Width;
            double newH = selectedShape.Height;
            string newFill = selectedShape.Fill.ToString();

            if (oldX != newX || oldY != newY || oldW != newW || oldH != newH || oldFill != newFill)
            {
                IEditorCommand cmd = new PropertyChangeCommand(selectedShape, oldX, oldY, oldW, oldH, oldFill, newX, newY, newW, newH, newFill);
                undoStack.Push(cmd);
                redoStack.Clear();
            }

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
                IEditorCommand cmd = new DeleteCommand(selectedShape, shapes);
                cmd.Execute();
                undoStack.Push(cmd);
                redoStack.Clear();

                selectedShape = null;
                RedrawCanvas();
            }
        }
    }

    public class ShapeData
    {
        public string Type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Fill { get; set; }
    }
}
