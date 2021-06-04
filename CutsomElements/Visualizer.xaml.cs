using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrawlerCraneVisual.CutsomElements
{
    public class Camera
    {
        public Point pos = new Point();
        public double scale = 1.0f;
        private double scale_coef = 1.05;
        public Panel view;
        public double pixelPerMeter = 10;
        public Point virtualSize = new Point();

        public Camera(Panel view)
        {
            this.view = view;
        }
        public Point getMousePos()
        {
            Point pos = Mouse.GetPosition(view);
            pos.X -= view.ActualWidth / 2.0;
            pos.Y = -pos.Y + view.ActualHeight / 2.0;
            return pos;
        }
        public void scaleUp()
        {
            Point mousePos = getMousePos();
            pos.X += (mousePos.X - mousePos.X / scale_coef) / scale;
            pos.Y += (mousePos.Y - mousePos.Y / scale_coef) / scale;
            scale *= scale_coef;
        }
        public void scaleDown()
        {
            Point mousePos = getMousePos();
            pos.X += (mousePos.X - mousePos.X * scale_coef) / scale;
            pos.Y += (mousePos.Y - mousePos.Y * scale_coef) / scale;
            scale /= scale_coef;
        }
        public Point screenToVirtual(Point point)
        {
            return new Point(point.X / scale + pos.X, point.Y / scale + pos.Y);
        }
        public Point virtualToScreen(Point point)
        {
            return new Point((point.X - pos.X) * scale, (point.Y - pos.Y) * scale);
        }
        public void update()
        {
            virtualSize = new Point(view.ActualWidth / scale / pixelPerMeter, view.ActualHeight / scale / pixelPerMeter);
        }
    }

    public abstract class VirtualObject
    {
        protected Camera camera;
        public VirtualObject(Camera camera) 
        {
            this.camera = camera;
        }
        public abstract void update();
        public abstract void remove();
        public abstract bool isInCamera();
    }
    public class VirtualLine : VirtualObject
    {
        public Point from = new Point();
        public Point to = new Point();
        public Color color { set { line.Stroke = new SolidColorBrush(value); } }
        public Visibility visibility { get { return line.Visibility; } set { line.Visibility = value; } }
        private Line line = new Line();
        public VirtualLine(Camera camera) : base (camera)
        {
            line.HorizontalAlignment = HorizontalAlignment.Left;
            line.VerticalAlignment = VerticalAlignment.Top;
            line.Stroke = new SolidColorBrush(Colors.Black);
            camera.view.Children.Add(line);
        }
        public override void update()
        {
            if (!isInCamera())
            {
                if (line.Visibility == Visibility.Visible)
                    line.Visibility = Visibility.Hidden;
                return;
            }
            else if (line.Visibility == Visibility.Hidden)
                line.Visibility = Visibility.Visible;
            Point temp = camera.virtualToScreen(new Point(from.X * camera.pixelPerMeter, from.Y * camera.pixelPerMeter));
            line.X1 = temp.X + camera.view.ActualWidth / 2.0;
            line.Y1 = -temp.Y + camera.view.ActualHeight / 2.0;
            temp = camera.virtualToScreen(new Point(to.X * camera.pixelPerMeter, to.Y * camera.pixelPerMeter));
            line.X2 = temp.X + camera.view.ActualWidth / 2.0;
            line.Y2 = -temp.Y + camera.view.ActualHeight / 2.0;
            line.StrokeThickness = camera.scale;
        }
        public override void remove()
        {
            camera.view.Children.Remove(line);
        }
        private bool allEqual<T>(params T[] values)
        {
            if (values == null || values.Length == 0)
                return true;
            return values.All(value => value.Equals(values[0]));
        }
        public override bool isInCamera()
        {
            double upperBorder = camera.pos.Y / camera.pixelPerMeter - camera.virtualSize.Y / 2.0;
            double lowerBorder = camera.pos.Y / camera.pixelPerMeter + camera.virtualSize.Y / 2.0;
            double leftBorder = camera.pos.X / camera.pixelPerMeter - camera.virtualSize.X / 2.0;
            double rightBorder = camera.pos.X / camera.pixelPerMeter + camera.virtualSize.X / 2.0;
            if (from.Y < upperBorder && to.Y < upperBorder ||
                from.Y > lowerBorder && to.Y > lowerBorder ||
                from.X < leftBorder && to.X < leftBorder ||
                from.X > rightBorder && to.X > rightBorder)
                return false;
            Point point1, point2, point3, point4;
            point1 = new Point(upperBorder, leftBorder);
            point2 = new Point(upperBorder, rightBorder);
            point3 = new Point(lowerBorder, rightBorder);
            point4 = new Point(lowerBorder, leftBorder);
            Vector line = to - from;
            if (allEqual(Vector.CrossProduct(line, point1 - from), 
                         Vector.CrossProduct(line, point2 - from), 
                         Vector.CrossProduct(line, point3 - from),
                         Vector.CrossProduct(line, point4 - from)))
                return false;
            return true;
        }
    }
    public class VirtualLabel : VirtualObject
    {
        public Point position = new Point();
        public double size = 0.1;
        public string text { get { return (string)label.Content; } set { label.Content = value; } }
        public Visibility visibility { get { return label.Visibility; } set { label.Visibility = value; } }
        private Label label = new Label();
        public VirtualLabel(Camera camera) : base(camera)
        {
            if (!isInCamera())
            {
                if (label.Visibility == Visibility.Visible)
                    label.Visibility = Visibility.Hidden;
                return;
            }
            else if (label.Visibility == Visibility.Hidden)
                label.Visibility = Visibility.Visible;
            text = "";
            label.HorizontalAlignment = HorizontalAlignment.Left;
            label.VerticalAlignment = VerticalAlignment.Bottom;
            label.VerticalContentAlignment = VerticalAlignment.Center;
            label.HorizontalContentAlignment = HorizontalAlignment.Center;
            label.Foreground = new SolidColorBrush(Colors.Black);
            camera.view.Children.Add(label);
        }
        public override void update()
        {
            label.FontSize = camera.scale * size * camera.pixelPerMeter;
            Point temp = camera.virtualToScreen(new Point(position.X * camera.pixelPerMeter, position.Y * camera.pixelPerMeter));
            label.Margin = new Thickness(temp.X - label.ActualWidth / 2.0 + camera.view.ActualWidth / 2.0, 0, 0, temp.Y - label.ActualHeight / 2.0 + camera.view.ActualHeight / 2.0);
        }
        public override void remove()
        {
            camera.view.Children.Remove(label);
        }
        public override bool isInCamera()
        {
            return !(position.X + label.ActualWidth / camera.scale/ camera.pixelPerMeter / 2.0 < camera.pos.X / camera.pixelPerMeter - camera.virtualSize.X / 2.0 ||
                     position.X - label.ActualWidth / camera.scale / camera.pixelPerMeter / 2.0 > camera.pos.X / camera.pixelPerMeter + camera.virtualSize.X / 2.0 ||
                     position.Y + label.ActualHeight / camera.scale / camera.pixelPerMeter / 2.0 < camera.pos.Y / camera.pixelPerMeter - camera.virtualSize.Y / 2.0 ||
                     position.Y - label.ActualHeight / camera.scale / camera.pixelPerMeter / 2.0 > camera.pos.Y / camera.pixelPerMeter + camera.virtualSize.Y / 2.0);
        }
    }
    public class VirtualRectangle : VirtualObject
    {
        public Point position = new Point();
        public Point size = new Point();
        public Color color { set { rect.Fill = new SolidColorBrush(value); } }
        public Visibility visibility { get { return rect.Visibility; } set { rect.Visibility = value; } }
        private Rectangle rect = new Rectangle();
        public VirtualRectangle(Camera camera) : base(camera)
        {
            if (!isInCamera())
            {
                if (rect.Visibility == Visibility.Visible)
                    rect.Visibility = Visibility.Hidden;
                return;
            }
            else if (rect.Visibility == Visibility.Hidden)
                rect.Visibility = Visibility.Visible;
            rect.HorizontalAlignment = HorizontalAlignment.Left;
            rect.VerticalAlignment = VerticalAlignment.Bottom;
            rect.Stroke = new SolidColorBrush(Colors.Black);
            rect.Fill = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
            camera.view.Children.Add(rect);
        }
        public override void update()
        {
            Point temp = camera.virtualToScreen(new Point(position.X * camera.pixelPerMeter, position.Y * camera.pixelPerMeter));
            rect.Width = size.X * camera.scale * camera.pixelPerMeter;
            rect.Height = size.Y * camera.scale * camera.pixelPerMeter;
            rect.StrokeThickness = camera.scale;
            rect.Margin = new Thickness(temp.X + camera.view.ActualWidth / 2.0, 0, 0, temp.Y + camera.view.ActualHeight / 2.0);
        }
        public override void remove()
        {
            camera.view.Children.Remove(rect);
        }
        public override bool isInCamera()
        {
            return !(position.X + size.X / 2.0 < camera.pos.X / camera.pixelPerMeter - camera.virtualSize.X / 2.0 ||
                     position.X - size.X / 2.0 > camera.pos.X / camera.pixelPerMeter + camera.virtualSize.X / 2.0 ||
                     position.Y + size.Y / 2.0 < camera.pos.Y / camera.pixelPerMeter - camera.virtualSize.Y / 2.0 ||
                     position.Y - size.Y / 2.0 > camera.pos.Y / camera.pixelPerMeter + camera.virtualSize.Y / 2.0);
        }
    }
    public enum VisualizerType
    {
        Crane,
        Capacity
    }
    public partial class Visualizer : UserControl
    {
        public string Title { get; set; }
        public VisualizerType Type { get; set; }
        private bool dragging = false;
        private Camera camera;
        private Point prevMousePos;
        private List<VirtualObject> objects = new List<VirtualObject>();

        private VirtualRectangle craneBase;
        private VirtualRectangle craneCrawlers;
        private VirtualRectangle building;
        private VirtualLine craneL1;
        private VirtualLine craneL2;
        private List<VirtualLine> mainFunctionLines;
        private List<VirtualLine> subFunctionLines;
        private VirtualRectangle minBorder;
        private VirtualRectangle maxBorder;

        private int poolSize = 1000;

        public Visualizer()
        {
            InitializeComponent();
            DataContext = this;
            camera = new Camera(MainGrid);
            createCoordinatePlane();
            craneBase = new VirtualRectangle(camera);
            craneCrawlers = new VirtualRectangle(camera);
            building = new VirtualRectangle(camera);
            craneL1 = new VirtualLine(camera);
            craneL2 = new VirtualLine(camera);
            minBorder = new VirtualRectangle(camera);
            maxBorder = new VirtualRectangle(camera);
            mainFunctionLines = new List<VirtualLine>();
            subFunctionLines = new List<VirtualLine>();
            craneBase.visibility = Visibility.Hidden;
            craneCrawlers.visibility = Visibility.Hidden;
            building.visibility = Visibility.Hidden;
            craneL1.visibility = Visibility.Hidden;
            craneL2.visibility = Visibility.Hidden;
            minBorder.visibility = Visibility.Hidden;
            maxBorder.visibility = Visibility.Hidden;
            minBorder.color = Color.FromArgb(102, 255, 0, 0);
            maxBorder.color = Color.FromArgb(102, 255, 0, 0);
            for (int i = 0; i < poolSize; i++)
            {
                mainFunctionLines.Add(new VirtualLine(camera) { visibility = Visibility.Hidden });
                subFunctionLines.Add(new VirtualLine(camera) { visibility = Visibility.Hidden });
            }
        }

        private void MainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                updateObjects();
                if (ErrorLabel.Visibility == Visibility.Visible)
                    ErrorLabel.Visibility = Visibility.Hidden;
            }
            catch
            {
                ErrorLabel.Visibility = Visibility.Visible;
            }
        }

        public void updateCrane(double buildingWidth, double buildingHeight, double distToSite, double siteLength, double distToL1, double craneHeight, double l1, double l2, double distToBack, double totalWidth, double l1_angle, double l2_angle, double distanceToBuilding)
        {
            if (craneBase.visibility == Visibility.Hidden)
            {
                craneBase.visibility = Visibility.Visible;
                craneCrawlers.visibility = Visibility.Visible;
                building.visibility = Visibility.Visible;
                craneL1.visibility = Visibility.Visible;
                craneL2.visibility = Visibility.Visible;
                objects.Add(craneBase);
                objects.Add(craneCrawlers);
                objects.Add(building);
                objects.Add(craneL1);
                objects.Add(craneL2);
            }
            Point offset = new Point(2.0, 0.0);
            craneBase.position.X = Math.Max(distanceToBuilding + buildingWidth, Math.Cos(l1_angle) * l1 + Math.Cos(l2_angle) * l2) + offset.X;
            craneBase.position.Y = craneHeight + offset.Y;
            craneBase.size.X = distToBack + distToL1;
            craneBase.size.Y = craneHeight * 1.5;
            craneCrawlers.position.X = craneBase.position.X + distToBack + distToL1 - totalWidth;
            craneCrawlers.position.Y = offset.Y;
            craneCrawlers.size.X = (totalWidth - distToBack) * 2.0;
            craneCrawlers.size.Y = craneHeight;
            craneL1.from.X = craneBase.position.X;
            craneL1.from.Y = craneHeight + offset.Y;
            craneL1.to.X = craneL1.from.X - Math.Cos(l1_angle) * l1;
            craneL1.to.Y = craneL1.from.Y + Math.Sin(l1_angle) * l1;
            craneL2.from.X = craneL1.to.X;
            craneL2.from.Y = craneL1.to.Y;
            craneL2.to.X = craneL2.from.X - Math.Cos(l2_angle) * l2;
            craneL2.to.Y = craneL2.from.Y + Math.Sin(l2_angle) * l2;

            building.position.X = craneBase.position.X - distanceToBuilding - buildingWidth;
            building.position.Y = offset.Y;
            building.size.X = buildingWidth;
            building.size.Y = buildingHeight;

            if (distToSite != 0)
            {
                minBorder.visibility = Visibility.Visible;
                minBorder.position.X = craneBase.position.X - distanceToBuilding;
                minBorder.position.Y = offset.Y;
                minBorder.size.X = distToSite;
                minBorder.size.Y = craneHeight * 1.5;
                objects.Add(minBorder);
            }
            else
            {
                minBorder.visibility = Visibility.Hidden;
                objects.Remove(minBorder);
            }

            if (siteLength != 0)
            {
                maxBorder.visibility = Visibility.Visible;
                maxBorder.position.X = building.position.X + building.size.X + distToSite + siteLength;
                maxBorder.position.Y = offset.Y;
                maxBorder.size.X = totalWidth * 4.0;
                maxBorder.size.Y = craneHeight * 1.5;
                objects.Add(maxBorder);
            }
            else
            {
                maxBorder.visibility = Visibility.Hidden;
                objects.Remove(maxBorder);
            }

            updateObjects();
        }

        public void clearCrane()
        {
            if (craneBase != null)
            {
                craneBase.visibility = Visibility.Hidden;
                craneCrawlers.visibility = Visibility.Hidden;
                building.visibility = Visibility.Hidden;
                craneL1.visibility = Visibility.Hidden;
                craneL2.visibility = Visibility.Hidden;
                minBorder.visibility = Visibility.Hidden;
                maxBorder.visibility = Visibility.Hidden;
                objects.Remove(craneBase);
                objects.Remove(craneCrawlers);
                objects.Remove(building);
                objects.Remove(craneL1);
                objects.Remove(craneL2);
                objects.Remove(minBorder);
                objects.Remove(maxBorder);
            }
        }

        public void updateFunction(CapacityFunction mainFunction, CapacityFunction subFunction)
        {
            {
                int i = -1;
                while (i < poolSize - 1 && mainFunctionLines[++i].visibility == Visibility.Visible)
                {
                    objects.Remove(mainFunctionLines[i]);
                    mainFunctionLines[i].visibility = Visibility.Hidden;
                }
                i = -1;
                while (i < poolSize - 1 && subFunctionLines[++i].visibility == Visibility.Visible)
                {
                    objects.Remove(subFunctionLines[i]);
                    subFunctionLines[i].visibility = Visibility.Hidden;
                }
            }
            addFunction(mainFunction, mainFunctionLines);
            addFunction(subFunction, subFunctionLines);
            updateObjects();
        }

        private void addFunction(CapacityFunction function, List<VirtualLine> pool)
        {
            if (function == null)
                return;
            Vector[] points = function.getPoints();
            if (points.Length > 1)
            {
                for (int i = 0; i < points.Length - 1 && i < poolSize; i++)
                {
                    pool[i].visibility = Visibility.Visible;
                    pool[i].from = new Point(points[i].X, points[i].Y);
                    pool[i].to = new Point(points[i + 1].X, points[i + 1].Y);
                    objects.Add(pool[i]);
                }
            }
        }

        private void createCoordinatePlane()
        {
            VirtualLine line = new VirtualLine(camera);
            line.from.X = -100;
            line.from.Y = 0;
            line.to.X = 100;
            line.to.Y = 0;
            objects.Add(line);
            line = new VirtualLine(camera);
            line.from.X = 0;
            line.from.Y = -100;
            line.to.X = 0;
            line.to.Y = 100;
            objects.Add(line);

            Color color = Color.FromArgb(51, 0, 0, 0);
            for (int i = -100; i <= 100; i++)
            {
                if (i == 0)
                    continue;
                line = new VirtualLine(camera);
                line.color = color;
                line.from.X = -100;
                line.from.Y = i;
                line.to.X = 100;
                line.to.Y = i;
                objects.Add(line);
                line = new VirtualLine(camera);
                line.color = color;
                line.from.X = i;
                line.from.Y = -100;
                line.to.X = i;
                line.to.Y = 100;
                objects.Add(line);
            }

            VirtualLabel label = new VirtualLabel(camera);
            label.text = "0";
            label.position.X = -0.3;
            label.position.Y = -0.3;
            label.size = 0.5;
            objects.Add(label);

            for (int i = -100; i <= 100; i++)
            {
                if (i == 0)
                    continue;
                label = new VirtualLabel(camera);
                label.text = (Math.Sign(i) == 1 ? "+" : "") + i.ToString();
                label.position.X = i;
                label.position.Y = -0.3;
                label.size = 0.5;
                objects.Add(label);

                label = new VirtualLabel(camera);
                label.text = (Math.Sign(i) == 1 ? "+" : "") + i.ToString();
                label.position.X = -0.5;
                label.position.Y = i;
                label.size = 0.5;
                objects.Add(label);
            }
        }

        private void updateObjects()
        {
            camera.update();
            foreach (VirtualObject obj in objects)
                obj.update();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragging = true;
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dragging = false;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = camera.getMousePos();
            if (dragging)
                camera.pos -= (mousePos - prevMousePos) / camera.scale;
            prevMousePos = mousePos;
            updateObjects();
        }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                camera.scaleUp();
            else
                camera.scaleDown();
            updateObjects();
        }

        private void ResetCamera_button_Click(object sender, RoutedEventArgs e)
        {
            camera.pos.X = 0;
            camera.pos.Y = 0;
            camera.scale = 1.0;
            updateObjects();
        }

        private void ErrorLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Initialize();
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            updateObjects();
        }
    }
}
