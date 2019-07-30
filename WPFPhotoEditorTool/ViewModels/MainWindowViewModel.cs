using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPFPhotoEditorTool.Commands;
using WPFPhotoEditorTool.Models;

namespace WPFPhotoEditorTool.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private Logger logger = LogManager.GetCurrentClassLogger();
        public MainWindowViewModel()
        {
            InkStrokes = new StrokeCollection();
            markColor = Brushes.Red;
            InitEditMenus();
        }

        private void InitEditMenus()
        {
            EditMenus = Common.GetEditMenus();
            curEditMenu = EditMenus.ElementAt(0);
            curDrawEnum = curEditMenu.DrawEnum;
        }
        /// <summary>
        /// 当前画笔
        /// </summary>
        private DrawEnum curDrawEnum { get; set; }
        /// <summary>
        /// 历史画笔记录
        /// </summary>
        private List<DrawEnum> drawEnums { get; set; }
        /// <summary>
        /// 当前菜单
        /// </summary>
        private EditMenu curEditMenu;

        /// <summary>
        /// 标记颜色
        /// </summary>
        SolidColorBrush markColor { get; set; }

        public SolidColorBrush MarkColor
        {
            get
            {
                return markColor;
            }
            set
            {
                markColor = value;
                OnPropertyChanged("MarkColor");
            }
        }

        bool allowOpera { get; set; } = true;

        Point startPoint { get; set; }
        /// <summary>
        /// 画笔属性
        /// </summary>
        DrawingAttributes drawingAttributes { get; set; }

        InkCanvas inkCanvasMeasure { get; set; }
        /// <summary>
        /// 笔刷
        /// </summary>
        StylusPointCollection penCollection { get; set; }

        /// <summary>
        /// 最后一个stroke
        /// </summary>
        Stroke stroke { get; set; }
        #region 画布事件
        public ICommand InkCanvasMouseDown
        {
            get
            {
                return new DelegateCommand<EventArgsCommandParameter>((param) =>
                {
                    if (!allowOpera)
                    {
                        allowOpera = true;
                        return;
                    }
                    inkCanvasMeasure = param.Sender as InkCanvas;
                    var e = param.EventArgs as MouseButtonEventArgs;
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        startPoint = e.GetPosition(inkCanvasMeasure);
                        if (curDrawEnum == DrawEnum.Square || curDrawEnum == DrawEnum.Eclipse)
                        {
                            SetDrawingAttributes();
                        }
                        else if (curDrawEnum == DrawEnum.Arrow)
                        {
                            Polygon myPolygon = new Polygon
                            {
                                Stroke = markColor,
                                StrokeThickness = 2,
                                StrokeLineJoin = PenLineJoin.Round,
                                Fill = markColor,
                                Points = new PointCollection { startPoint, startPoint, startPoint, startPoint, startPoint, },
                            };
                            if (inkCanvasMeasure.Children.Count == 0)
                            {
                                var curCanvas = new Canvas();
                                inkCanvasMeasure.Children.Add(curCanvas);
                            }
                                 (inkCanvasMeasure.Children[0] as Canvas).Children.Add(myPolygon);
                        }
                        else if (curDrawEnum == DrawEnum.Pen)
                        {
                            SetDrawingAttributes();
                            penCollection = new StylusPointCollection();
                        }
                    }
                });
            }
        }

        public ICommand InkCanvasMouseMove
        {
            get
            {
                return new DelegateCommand<EventArgsCommandParameter>((param) =>
                {
                    if (!allowOpera)
                        return;
                    inkCanvasMeasure = param.Sender as InkCanvas;
                    var e = param.EventArgs as MouseEventArgs;
                    if (e == null)
                        return;
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        Point endP = e.GetPosition(inkCanvasMeasure);

                        if (curDrawEnum == DrawEnum.Square)
                        { // Draw square
                            DrawSquare(endP);
                        }
                        else if (curDrawEnum == DrawEnum.Eclipse)
                        {  // Draw Eclipse
                            DrawEllipse(endP);
                        }
                        else if (curDrawEnum == DrawEnum.Arrow)
                        {  // Draw Arrow
                            DrawArrow(endP);
                        }
                        else if (curDrawEnum == DrawEnum.Pen)
                        {
                            DrawPen(endP);
                        }
                    }

                });
            }
        }
        public ICommand InkCanvasMouseUp
        {
            get
            {
                return new DelegateCommand<EventArgsCommandParameter>((p) =>
                {
                    if (!allowOpera)
                    {
                        allowOpera = true;
                        return;
                    }
                    stroke = null;
                    //记录上次是什么操作
                    if (drawEnums == null)
                        drawEnums = new List<DrawEnum>();
                    drawEnums.Add(curDrawEnum);
                });
            }
        }
        #endregion

        #region 笔刷
        private void DrawPen(Point endP)
        {
            try
            {
                penCollection.Add(new StylusPoint(endP.X, endP.Y));
                if (stroke != null)
                    InkStrokes.Remove(stroke);
                stroke = new Stroke(penCollection)
                {
                    DrawingAttributes = inkCanvasMeasure.DefaultDrawingAttributes.Clone()
                };
                InkStrokes.Add(stroke);

            }
            catch (Exception ex)
            {
                logger.Error($"DrawEllipse:{ex.Message}");
            }
        }
        #endregion

        #region 底部菜单


        /// <summary>
        /// 底部菜单
        /// </summary>
        private ICollection<EditMenu> editMenus;

        public ICollection<EditMenu> EditMenus
        {
            get { return editMenus; }
            set
            {
                editMenus = value;
                OnPropertyChanged("EditMenus");
            }
        }
        #endregion

        #region 画笔属性
        /// <summary>
        /// 设置画笔属性
        /// </summary>
        public void SetDrawingAttributes()
        {

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                drawingAttributes = new DrawingAttributes
                {
                    Color = markColor.Color,
                    Width = 2,
                    Height = 2,
                    StylusTip = StylusTip.Rectangle,
                    IsHighlighter = false,
                    IgnorePressure = true
                };
                inkCanvasMeasure.DefaultDrawingAttributes = drawingAttributes;
            }));
        }
        #endregion

        #region 画箭头
        /// <summary>
        /// 画箭头
        /// </summary>
        /// <param name="endP"></param>
        private void DrawArrow(Point endP, double arrowAngle = Math.PI / 6, double arrowLength = 20)
        {
            try
            {
                if (inkCanvasMeasure.Children.Count == 0)
                    return;
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
            catch (Exception ex)
            {

                logger.Error($"DrawArrow:{ex.Message}");
            }

        }
        #endregion

        #region 画矩形
        /// <summary>
        /// 画矩形
        /// </summary>
        private void DrawSquare(Point endP)
        {
            try
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
                    InkStrokes.Remove(stroke);
                stroke = new Stroke(point)
                {
                    DrawingAttributes = inkCanvasMeasure.DefaultDrawingAttributes.Clone()
                };
                InkStrokes.Add(stroke);
                point = null;
            }
            catch (Exception ex)
            {

                logger.Error($"DrawSquare:{ex.Message}");
            }
        }
        #endregion

        #region 画椭圆
        /// <summary>
        /// 画椭圆
        /// </summary>
        /// <param name="endP"></param>
        private void DrawEllipse(Point endP)
        {
            try
            {
                List<Point> pointList = GenerateEclipseGeometry(startPoint, endP);
                StylusPointCollection point = new StylusPointCollection(pointList);
                if (stroke != null)
                    InkStrokes.Remove(stroke);
                stroke = new Stroke(point)
                {
                    DrawingAttributes = inkCanvasMeasure.DefaultDrawingAttributes.Clone()
                };
                InkStrokes.Add(stroke);
                point = null;
            }
            catch (Exception ex)
            {
                logger.Error($"DrawEllipse:{ex.Message}");
            }
        }
        /// <summary>
        /// 椭圆形坐标
        /// </summary>
        /// <param name="st"></param>
        /// <param name="ed"></param>
        /// <returns></returns>
        private List<Point> GenerateEclipseGeometry(Point st, Point ed)
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

        #region 画笔轨迹
        private StrokeCollection inkStrokes;
        public StrokeCollection InkStrokes
        {
            get { return inkStrokes; }
            set
            {
                inkStrokes = value;
                OnPropertyChanged("InkStrokes");
            }
        }
        #endregion

        #region 画笔切换
        public ICommand BrushChangeCommand
        {
            get
            {
                return new DelegateCommand<EventArgsCommandParameter>(BrushChange, (p) => { return true; });
            }
        }
        /// <summary>
        /// 画笔切换
        /// </summary>
        /// <param name="obj"></param>
        private void BrushChange(EventArgsCommandParameter obj)
        {

            var name = (obj.Sender as Image).Name;
            var item = EditMenus.Where(t => t.Name == name).FirstOrDefault();
            if (!string.IsNullOrEmpty(curEditMenu.SourceUrl))
                curEditMenu.Source = curEditMenu.SourceUrl;
            if (item != null)
            {
                item.Source = item.CheckedSourceUrl;
                curEditMenu = item;
                curDrawEnum = curEditMenu.DrawEnum;
            }
        }
        #endregion

        #region SaveImageCommand保存图片
        public ICommand SaveImageCommand
        {
            get
            {
                return new DelegateCommand<EventArgsCommandParameter>((p) =>
                {

                    var imageName = Guid.NewGuid().ToString("N") + ".jpeg";
                    var path = Environment.CurrentDirectory + @"\" + imageName;
                    var ctrl = p.Parameter as FrameworkElement;
                    Save(path, ctrl);
                });
            }
        }
        private bool Save(string fileName, FrameworkElement uIElement)
        {
            try
            {
                RenderTargetBitmap rtp = new RenderTargetBitmap((int)uIElement.ActualWidth, (int)uIElement.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                rtp.Render(uIElement);
                JpegBitmapEncoder jpeg = new JpegBitmapEncoder();
                jpeg.Frames.Add(BitmapFrame.Create(rtp));
                using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
                {
                    jpeg.Save(fs);
                    fs.Close();
                    fs.Dispose();
                    jpeg = null;
                    rtp = null;
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"Save:{ex.Message}");
                return false;
            }

        }
        #endregion

        #region CancelCommand撤销操作
        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (drawEnums.LastOrDefault() == DrawEnum.Eclipse || drawEnums.LastOrDefault() == DrawEnum.Square || drawEnums.LastOrDefault() == DrawEnum.Pen)
                    {
                        if (InkStrokes.Count > 0)
                            InkStrokes.RemoveAt(InkStrokes.Count - 1);
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
                });
            }
        }
        #endregion

        #region SendToComparedCommand发送对比屏幕
        public ICommand SendToComparedCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    MessageBox.Show("发送对比屏幕操作");
                });
            }
        }
        #endregion

        #region SendToIMCommand发送至聊天框
        public ICommand SendToIMCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    MessageBox.Show("发送至聊天框操作");
                });
            }
        }
        #endregion

        #region CloseCommand退出操作
        public ICommand CloseCommand
        {
            get
            {
                return new DelegateCommand<EventArgsCommandParameter>((p) =>
                {
                    Dispose();
                    Window.GetWindow(p.Sender)?.Close();
                });
            }
        }
        #endregion

        #region 标记颜色选择
        public ICommand MarkColorSelectCommand
        {
            get
            {
                return new DelegateCommand<EventArgsCommandParameter>((p) =>
                {
                    var TxtMarkColor = p.Sender as TextBlock;
                    using (var colorDlg = new System.Windows.Forms.ColorDialog())
                    {
                        colorDlg.AnyColor = true;
                        colorDlg.FullOpen = true;
                        if (markColor == null)
                        {
                            markColor = Brushes.Red;
                        }
                        colorDlg.Color = System.Drawing.ColorTranslator.FromHtml(markColor.Color.ToString());


                        var ok = colorDlg.ShowDialog();
                        if (ok == System.Windows.Forms.DialogResult.OK)
                        {
                            markColor = new SolidColorBrush(Color.FromRgb(colorDlg.Color.R, colorDlg.Color.G, colorDlg.Color.B));
                            TxtMarkColor.Background = markColor;
                        }
                    }

                });
            }
        }


        #endregion

        #region Dispose


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    InkStrokes = null;
                    EditMenus = null;
                    stroke = null;
                    drawEnums = null;
                    curEditMenu = null;
                    MarkColor = null;
                    drawingAttributes = null;
                    inkCanvasMeasure = null;
                    penCollection = null;
                }

                m_disposed = true;
            }
        }
        // //Finalize用于释放非托管的资源
        //Dispose用于释放所有资源，包括托管和非托管
        ~MainWindowViewModel()
        {
            Dispose(false);
        }

        private bool m_disposed;
        #endregion
    }
}
