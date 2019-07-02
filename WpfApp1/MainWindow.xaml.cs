using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp1.Model;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point startPoint { get; set; }
        private ViewModel viewModel;
        private DrawEnum drawEnum { get; set; }
        DrawingAttributes drawingAttributes { get; set; }
        /// <summary>
        /// 最后一个stroke
        /// </summary>
        Stroke stroke { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            viewModel = new ViewModel
            {
                InkStrokes = new StrokeCollection()
            };
           
            DataContext = viewModel;
        }
        private Dictionary<Stroke, Timer> dicTimer = new Dictionary<Stroke, Timer>();
        private void InkCanvasMeasure_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
           Timer t1= new Timer(new TimerCallback(ChangeOpacity), e.Stroke, 100, 100);
            dicTimer.Add(e.Stroke, t1);
        }

        private void ChangeOpacity(object obj)
        {
            Stroke line = obj as Stroke;
            Color linecolor = line.DrawingAttributes.Color;
            if (linecolor.ScA > 0)
                linecolor.ScA -= 0.1f;
            this.Dispatcher.BeginInvoke(new Action(() => { line.DrawingAttributes.Color = linecolor; }));
            if (linecolor.A <= 0)
            {
                dicTimer[line].Dispose();
                dicTimer.Remove(line);
            }
        }

        private void DrawSquare_Click(object sender, RoutedEventArgs e)
        {
            if (btnSquare.IsChecked == true)
            {
                drawEnum = DrawEnum.Square;
                btnEllipse.IsChecked = false;
                btnArrow.IsChecked = false;
                btnPen.IsChecked = false;
            }
        }
        private void Arrow_Click(object sender, RoutedEventArgs e)
        {
            if (btnArrow.IsChecked == true)
            {
                drawEnum = DrawEnum.Arrow;
                btnSquare.IsChecked = false;
                btnEllipse.IsChecked = false;
                btnPen.IsChecked = false;
            }
        }
        private void DrawEllipse_Click(object sender, RoutedEventArgs e)
        {
            if (btnEllipse.IsChecked == true)
            {
                drawEnum = DrawEnum.Eclipse;
                btnSquare.IsChecked = false;
                btnArrow.IsChecked = false;
                btnPen.IsChecked = false;
            }
        }
        private void InkCanvasMeasure_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                startPoint = e.GetPosition(inkCanvasMeasure);
                if (drawEnum == DrawEnum.Square || drawEnum == DrawEnum.Eclipse)
                {
                    SetDrawingAttributes();
                }
                else if (drawEnum == DrawEnum.Arrow)
                {
                    Polygon myPolygon = new Polygon
                    {
                        Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x00, 0x00)),
                        StrokeThickness = 2,
                        StrokeLineJoin = PenLineJoin.Round,
                        Fill = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x00, 0x00)),
                        Points = new PointCollection { startPoint, startPoint, startPoint, startPoint, startPoint, },
                    };
                    if (inkCanvasMeasure.Children.Count == 0)
                    {
                        var curCanvas = new Canvas();
                        inkCanvasMeasure.Children.Add(curCanvas);
                    }

                    (inkCanvasMeasure.Children[0] as Canvas).Children.Add(myPolygon);

                }
                else if (drawEnum == DrawEnum.Pen)
                {
                    SetDrawingAttributes();
                    penCollection = new StylusPointCollection();
                }


            }
        }
        /// <summary>
        /// 设置画笔属性
        /// </summary>
        public void SetDrawingAttributes()
        {
            drawingAttributes = new DrawingAttributes
            {
                Color = Colors.Red,
                Width = 2,
                Height = 2,
                StylusTip = StylusTip.Ellipse,
                IsHighlighter = false,
                IgnorePressure = true
            };

            inkCanvasMeasure.DefaultDrawingAttributes = drawingAttributes;
        }
        private void InkCanvasMeasure_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point endP = e.GetPosition(inkCanvasMeasure);

                if (drawEnum == DrawEnum.Square)
                { // Draw square
                    DrawSquare(endP);
                }
                else if (drawEnum == DrawEnum.Eclipse)
                {  // Draw Eclipse
                    DrawEllipse(endP);
                }
                else if (drawEnum == DrawEnum.Arrow)
                {  // Draw Arrow
                    DrawArrow(endP);
                }
                else if (drawEnum == DrawEnum.Pen)
                {
                    DrawPen(endP);
                }
            }
        }
        /// <summary>
        /// 画箭头
        /// </summary>
        /// <param name="endP"></param>
        private void DrawArrow(Point endP, double arrowAngle = Math.PI / 6, double arrowLength = 20)
        {
            Canvas myCanvas = (Canvas)inkCanvasMeasure.Children[inkCanvasMeasure.Children.Count - 1];

            if (myCanvas == null)
            {
                return;
            }
            // 修改多边形
            Polygon myPolygon = (Polygon)myCanvas.Children[myCanvas.Children.Count - 1];
            double x1 = myPolygon.Points[0].X;
            double y1 = myPolygon.Points[0].Y;
            double x2 = endP.X;
            double y2 = endP.Y;
            Point point1 = new Point(x1, y1);     // 箭头起点
            Point point2 = new Point(x2, y2);     // 箭头终点            
            double angleOri = Math.Atan((y2 - y1) / (x2 - x1));      // 起始点线段夹角
            double angleDown = angleOri - arrowAngle;   // 箭头扩张角度
            double angleUp = angleOri + arrowAngle;     // 箭头扩张角度
            int directionFlag = (x2 > x1) ? -1 : 1;     // 方向标识
            double x3 = x2 + ((directionFlag * arrowLength) * Math.Cos(angleDown));   // 箭头第三个点的坐标
            double y3 = y2 + ((directionFlag * arrowLength) * Math.Sin(angleDown));
            double x4 = x2 + ((directionFlag * arrowLength) * Math.Cos(angleUp));     // 箭头第四个点的坐标
            double y4 = y2 + ((directionFlag * arrowLength) * Math.Sin(angleUp));
            Point point3 = new Point(x3, y3);   // 箭头第三个点
            Point point4 = new Point(x4, y4);   // 箭头第四个点
            myPolygon.Points[0] = new Point(x1, y1);
            myPolygon.Points[1] = new Point(x2, y2);
            myPolygon.Points[2] = new Point(x3, y3);
            myPolygon.Points[3] = new Point(x4, y4);
            myPolygon.Points[4] = new Point(x2, y2);


        }


        #region 画矩形
        /// <summary>
        /// 画矩形
        /// </summary>
        private void DrawSquare(Point endP)
        {
            List<Point> pointList = new List<Point>
                    {
                        new Point(startPoint.X, startPoint.Y),
                        new Point(startPoint.X, endP.Y),
                        new Point(endP.X, endP.Y),
                        new Point(endP.X, startPoint.Y),
                        new Point(startPoint.X, startPoint.Y),
                    };
            StylusPointCollection point = new StylusPointCollection(pointList);
            if (stroke != null)
                viewModel.InkStrokes.Remove(stroke);
            stroke = new Stroke(point)
            {
                DrawingAttributes = inkCanvasMeasure.DefaultDrawingAttributes.Clone()
            };
            viewModel.InkStrokes.Add(stroke);
            point = null;
        }
        #endregion
        #region 画椭圆
        /// <summary>
        /// 画椭圆
        /// </summary>
        /// <param name="endP"></param>
        private void DrawEllipse(Point endP)
        {
            List<Point> pointList = GenerateEclipseGeometry(startPoint, endP);
            StylusPointCollection point = new StylusPointCollection(pointList);
            if (stroke != null)
                viewModel.InkStrokes.Remove(stroke);
            stroke = new Stroke(point)
            {
                DrawingAttributes = inkCanvasMeasure.DefaultDrawingAttributes.Clone()
            };
            viewModel.InkStrokes.Add(stroke);
            point = null;
        }
        /// <summary>
        /// 椭圆形坐标
        /// </summary>
        /// <param name="st"></param>
        /// <param name="ed"></param>
        /// <returns></returns>
        private List<Point> GenerateEclipseGeometry(System.Windows.Point st, System.Windows.Point ed)
        {
            double a = 0.5 * (ed.X - st.X);
            double b = 0.5 * (ed.Y - st.Y);
            List<System.Windows.Point> pointList = new List<System.Windows.Point>();
            for (double r = 0; r <= 2 * Math.PI; r = r + 0.01)
            {
                pointList.Add(new System.Windows.Point(0.5 * (st.X + ed.X) + a * Math.Cos(r), 0.5 * (st.Y + ed.Y) + b * Math.Sin(r)));
            }
            return pointList;
        }
        #endregion

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {

            RenderTargetBitmap rtp = new RenderTargetBitmap((int)MeasureGrid.ActualWidth, (int)MeasureGrid.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtp.Render(MeasureGrid);
            JpegBitmapEncoder jpeg = new JpegBitmapEncoder();
            jpeg.Frames.Add(BitmapFrame.Create(rtp));
            using (FileStream fs = new FileStream(@"d:\temp\test.jpeg", FileMode.CreateNew))
            {
                jpeg.Save(fs);
                fs.Close();
                fs.Dispose();
                jpeg = null;
                rtp = null;
            }



        }

        List<DrawEnum> drawEnums { get; set; }
        private void InkCanvasMeasure_MouseUp(object sender, MouseButtonEventArgs e)
        {
            stroke = null;
            //记录上次是什么操作
            if (drawEnums==null)
                drawEnums = new List<DrawEnum>();
            drawEnums.Add(drawEnum);

        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (drawEnums.LastOrDefault() == DrawEnum.Eclipse || drawEnums.LastOrDefault() == DrawEnum.Square || drawEnums.LastOrDefault() == DrawEnum.Pen)
            {
                if (viewModel.InkStrokes.Count > 0)
                    viewModel.InkStrokes.RemoveAt(viewModel.InkStrokes.Count - 1);
            }
            else
            {
                if (inkCanvasMeasure.Children.Count > 0)
                {
                    var res = (inkCanvasMeasure.Children[0] as Canvas);
                    if (res.Children.Count > 0)
                        res.Children.RemoveAt(res.Children.Count - 1);
                }
            }
            if (drawEnums.Count > 0)
                drawEnums.RemoveAt(drawEnums.Count - 1);
        }

        private void BtnPen_Click(object sender, RoutedEventArgs e)
        {
            penCollection = null;
            if (btnPen.IsChecked == true)
            {
                drawEnum = DrawEnum.Pen;
                btnSquare.IsChecked = false;
                btnArrow.IsChecked = false;
                btnEllipse.IsChecked = false;
            }
           
           
            
        }

        StylusPointCollection penCollection { get; set; }
        /// <summary>
        /// 画笔
        /// </summary>
        /// <param name="endP"></param>
        private void DrawPen(Point endP)
        {
            
                penCollection.Add(new StylusPoint(endP.X, endP.Y));
                if (stroke != null)
                    viewModel.InkStrokes.Remove(stroke);
                stroke = new Stroke(penCollection)
                {
                    DrawingAttributes = inkCanvasMeasure.DefaultDrawingAttributes.Clone()
                };
            
                viewModel.InkStrokes.Add(stroke);

            
        }
    }
}
