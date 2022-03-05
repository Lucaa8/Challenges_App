using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Challenges_App.Controls
{
    /// <summary>
    /// Logique d'interaction pour ButtonControl.xaml
    /// </summary>
    public partial class ButtonControl : UserControl
    {
        private Path RightBorder;
        private Path LeftBorder;
        private Path TopBorder;
        private Path BotBorder;

        private Point TopX1;
        private Point TopX2;
        private Point BotX1;
        private Point BotX2;

        private List<EllipseGeometry> RightBorderPoints = new List<EllipseGeometry>();
        private List<EllipseGeometry> LeftBorderPoints = new List<EllipseGeometry>();
        private List<EllipseGeometry> TopBorderPoints = new List<EllipseGeometry>();
        private List<EllipseGeometry> BotBorderPoints = new List<EllipseGeometry>();

        private Boolean enabled = true;
        private Boolean animation = false;

        public ButtonControl()
        {
            InitializeComponent();
            TopX1 = new Point(rectFont.Margin.Left + rectFont.RadiusX, rectFont.Margin.Top);
            TopX2 = new Point(grid.Width - rectFont.Margin.Right - rectFont.RadiusX, rectFont.Margin.Top);
            BotX1 = new Point(rectFont.Margin.Left + rectFont.RadiusX, grid.Height - rectFont.Margin.Bottom);
            BotX2 = new Point(grid.Width - rectFont.Margin.Right - rectFont.RadiusX, grid.Height - rectFont.Margin.Bottom);
            Enabled = true;
            Text = "Button";
            createBorder(2);
            FrontColor = Brushes.Black;
            BackColor = Brushes.White;
        }

        public Boolean Enabled
        {
            set { 
                enabled = value;
                if (enabled)
                {
                    CursorType = Cursors.Hand;
                }
                else
                {
                    CursorType = Cursors.No;
                }
            }
            get { return enabled; }
        }

        public Brush FrontColor
        {
            set { lblName.Foreground = value; }
            get { return lblName.Foreground; }
        }
        public Brush BackColor
        {
            set { rectFont.Fill = value; }
            get { return lblName.Background; }
        }
        public Brush BorderColor
        {
            set { updateBorder(value); }
            get { return LeftBorder.Stroke; }
        }
        public int BorderSize
        {
            set { updateBorder(value); }
            get { return (int)LeftBorder.StrokeThickness; }
        }

        public String Text
        {
            set { lblName.Content = value; }
            get { return lblName.Content.ToString(); }
        }

        public Cursor CursorType
        {
            set { rectEvent.Cursor = value; }
            get { return rectEvent.Cursor; }
        }

        public Boolean Animation
        {
            set {
                if (!value.Equals(animation))
                {
                    animation = value;
                    if (animation)
                    {
                        runPartAnimation("group_Top", TopBorderPoints, 9);
                    }
                }

            }
            get { return animation; }
        }

        private void runNext(String now, int shift)
        {
            if (!Animation) return;
            if (now.Equals("group_Top"))
            {
                runPartAnimation("group_Right", RightBorderPoints, shift);
            }
            if (now.Equals("group_Right"))
            {
                runPartAnimation("group_Bot", BotBorderPoints, shift);
            }
            if (now.Equals("group_Bot"))
            {
                runPartAnimation("group_Left", LeftBorderPoints, shift);
            }
            if (now.Equals("group_Left"))
            {
                runPartAnimation("group_Top", TopBorderPoints, shift);
            }
        }

        private async void runPartAnimation(String geometry, List<EllipseGeometry> points, int shift)
        {
            GeometryGroup gg = getGeometry(geometry);
            for (int i = 0; i < points.Count + shift; i++)
            {
                if (i >= shift)
                {
                    gg.Children.Add(points[i - shift]);
                }
                if (i < points.Count)
                {
                    gg.Children.Remove(points[i]);
                }
                if (i == points.Count - 1)
                {
                    runNext(geometry, shift);
                }
                await Task.Delay(30);
            }
        }

        private String convert(Double d)
        {
            return Convert.ToString(d).Replace(",", ".");
        }

        private Path buildBorder(String border, int size)
        {
            Path p = new Path();
            if (border.Equals("Right"))
            {
                p = new Bezier(new Point[] {
                new Point(grid.Width - rectFont.Margin.Right - rectFont.RadiusX - 2, grid.Height - rectFont.Margin.Bottom),
                new Point(grid.Width+5, grid.Height-3),
                new Point(grid.Width+5, 3),
                new Point(grid.Width - rectFont.Margin.Right - rectFont.RadiusX - 2, rectFont.Margin.Top) }).getBezierSegment();
            }
            else if (border.Equals("Left"))
            {
                p = new Bezier(new Point[] {
                new Point(rectFont.Margin.Left + rectFont.RadiusX + 2, grid.Height - rectFont.Margin.Bottom),
                new Point(-5, grid.Height-3),
                new Point(-5, 3),
                new Point(rectFont.Margin.Left + rectFont.RadiusX + 2, rectFont.Margin.Top) }).getBezierSegment();
            }
            else if (border.Equals("Top"))
            {
                p.Data = Geometry.Parse("M " + convert(TopX1.X) + "," + convert(TopX1.Y) + " L " + convert(TopX2.X) + "," + convert(TopX2.Y));
            }
            else if (border.Equals("Bot"))
            {
                p.Data = Geometry.Parse("M " + convert(BotX1.X) + "," + convert(BotX1.Y) + " L " + convert(BotX2.X) + "," + convert(BotX2.Y));
            }

            p.Name = border + "Border";
            p.Stroke = Brushes.White;
            p.StrokeThickness = size;

            grid.Resources.Add("geometry_" + border, p.Data);

            GeometryGroup GG = new GeometryGroup();
            GG.SetValue(NameProperty, "group_" + border);
            GG.FillRule = FillRule.Nonzero;
            p.Clip = GG;

            return p;
        }

        private void createBorder(int size)
        {
            TopBorder = buildBorder("Top", size);
            BotBorder = buildBorder("Bot", size);
            RightBorder = buildBorder("Right", size);
            LeftBorder = buildBorder("Left", size);

            grid.Children.Add(LeftBorder);
            grid.Children.Add(RightBorder);
            grid.Children.Add(TopBorder);
            grid.Children.Add(BotBorder);

            fillList(TopBorder, ref TopBorderPoints, size, getGeometry("group_Top"));
            fillList(BotBorder, ref BotBorderPoints, size, getGeometry("group_Bot"));
            fillList(LeftBorder, ref LeftBorderPoints, size, getGeometry("group_Left"));
            fillList(RightBorder, ref RightBorderPoints, size, getGeometry("group_Right"));
        }

        private void fillList(Path pa, ref List<EllipseGeometry> list, int ellipseSize, GeometryGroup toAdd)
        {
            Point p;
            Point tg;
            for (int i = 0; i < 25; i++)
            {
                pa.Data.GetFlattenedPathGeometry().GetPointAtFractionLength(i / 25f, out p, out tg);
                EllipseGeometry eg = new EllipseGeometry(p, ellipseSize, ellipseSize);
                list.Add(eg);
            }
            fillPath(ref list, toAdd);
        }

        private void fillPath(ref List<EllipseGeometry> list, GeometryGroup gg)
        {
            if (list == RightBorderPoints || list == BotBorderPoints)
            {
                list.Reverse();
            }
            foreach (EllipseGeometry eg in list)
            {
                gg.Children.Add(eg);
            }
        }

        private void updateBorder(object newValue)
        {
            if (newValue is int)
            {
                int size = (int)newValue;
                LeftBorder.StrokeThickness = size;
                RightBorder.StrokeThickness = size;
                TopBorder.StrokeThickness = size;
                BotBorder.StrokeThickness = size;
            }
            else if (newValue is Brush)
            {
                Brush color = (Brush)newValue;
                LeftBorder.Stroke = color;
                RightBorder.Stroke = color;
                TopBorder.Stroke = color;
                BotBorder.Stroke = color;
            }
        }

        private GeometryGroup getGeometry(String name)
        {
            GeometryGroup result = null;
            Action a = new Action(() =>
            {
                foreach (FrameworkElement fe in grid.Children)
                {
                    if (fe is Path)
                    {
                        Path pa = (Path)fe;
                        GeometryGroup group = (GeometryGroup)pa.Clip;
                        if ((String)group.GetValue(NameProperty) == name)
                        {
                            result = group;
                        }
                    }
                }
            });
            if (!CheckAccess())
            {
                Dispatcher.Invoke(a);
            }
            else a.Invoke();
            return result;
        }

        public delegate void MouseEnterCallback(MouseEventArgs args);
        public event MouseEnterCallback MouseEnterCallbackEvent;

        public delegate void MouseLeaveCallback(MouseEventArgs args);
        public event MouseLeaveCallback MouseLeaveCallbackEvent;

        public delegate void MouseClickCallback(MouseButtonEventArgs args);
        public event MouseClickCallback MouseClickCallbackEvent;

        public void addMouseEnter(MouseEnterCallback function)
        {
            MouseEnterCallbackEvent += new MouseEnterCallback(function);
        }
        public void addMouseLeave(MouseLeaveCallback function)
        {
            MouseLeaveCallbackEvent += new MouseLeaveCallback(function);
        }
        public void addMouseClick(MouseClickCallback function)
        {
            MouseClickCallbackEvent += new MouseClickCallback(function);
        }
        private void EventMouseEnter(object sender, MouseEventArgs e)
        {
            if (MouseEnterCallbackEvent == null) return;
            MouseEnterCallbackEvent(e);
        }

        private void EventMouseLeave(object sender, MouseEventArgs e)
        {
            if (MouseLeaveCallbackEvent == null) return;
            MouseLeaveCallbackEvent(e);
        }

        private void EventMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (MouseClickCallbackEvent == null || !enabled) return;
            MouseClickCallbackEvent(e);
        }

        public class Bezier
        {
            Point[] Points;

            public Bezier(Point[] PointsPath)
            {
                if (PointsPath.Length < 2) return;
                Points = PointsPath;
            }

            public Path getBezierSegment()
            {
                Path p = new Path();
                p.Stroke = Brushes.Black;
                p.StrokeThickness = 1;

                PathGeometry Geometry = new PathGeometry();
                p.Data = Geometry;

                PolyBezierSegment BezierSegment = new PolyBezierSegment();
                PointCollection points = new PointCollection(Points.Length - 1);
                for (int i = 1; i < Points.Length; i++) points.Add(Points[i]);
                BezierSegment.Points = points;

                PathSegmentCollection Segments = new PathSegmentCollection();
                Segments.Add(BezierSegment);

                PathFigure Figure = new PathFigure();
                Figure.StartPoint = Points[0];
                Figure.Segments = Segments;

                PathFigureCollection FiguresCollection = new PathFigureCollection();
                FiguresCollection.Add(Figure);

                Geometry.Figures = FiguresCollection;
                return p;
            }

        }
    }
}
