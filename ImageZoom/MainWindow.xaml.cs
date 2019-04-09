using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageZoom
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
           //Cursors
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            imageAutomationPeer = new ImageAutomationPeer(img);
            InitPoints();
           

        }
        Rect initRect { get; set; }

        private void InitPoints()
        {
            initRect = rect = imageAutomationPeer.GetBoundingRectangle();
        }


        Rect rect { get; set; }
        ImageAutomationPeer imageAutomationPeer { get; set; }
        //用于计算放大缩小倍率
        private readonly int magnification = 10000;
        private void Img_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var imgItem = sender as Image;
            Point centerPoint = e.GetPosition(imgItem);
            str.CenterX = centerPoint.X;
            str.CenterY = centerPoint.Y;

            if (str.ScaleX <= 1 && str.ScaleY <= 1 && e.Delta < 0)
            {
                return;
            }

            var zoom = (double)e.Delta / magnification;
            str.ScaleX += zoom;
            str.ScaleY += zoom;
           
            ResetImage();
            rect = imageAutomationPeer.GetBoundingRectangle();

        }


        private bool isMouseLeftButtonDown = false;
        Point previousMousePoint = new Point(0, 0);
        private void Img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isMouseLeftButtonDown = true;
            previousMousePoint = e.GetPosition(img);
        }

        private void Img_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseLeftButtonDown == true)
            {
                Point position = e.GetPosition(img);
                double diffX = position.X - previousMousePoint.X;
                double diffY = position.Y - previousMousePoint.Y;
                //向右边
                if (diffX > 0 && (initRect.TopLeft.X > rect.TopLeft.X + 5)) { 
                    
                    tlt.X += position.X - previousMousePoint.X;
                }
                else 
                    tlt.X +=0;
               
                ///向左边
                if (diffX < 0 && (initRect.TopRight.X < rect.TopRight.X + 5))
                    tlt.X += position.X - previousMousePoint.X;
                else
                    tlt.X += 0;
                //向上
                if (diffY < 0 && (initRect.BottomLeft.Y < rect.BottomLeft.Y + 5))
                    tlt.Y += position.Y - previousMousePoint.Y;
                else
                    tlt.Y += 0;
                //向下
                if (diffY > 0 && (initRect.TopRight.Y > rect.TopRight.Y + 5))
                    tlt.Y += position.Y - previousMousePoint.Y;
                else
                    tlt.Y += 0;
                rect = imageAutomationPeer.GetBoundingRectangle();
            }
        }
        private void ResetImage()
        {
            if (initRect.Left > rect.Left)
                tlt.X += Math.Abs(initRect.Left - rect.Left);
             if (initRect.Right < rect.Right)
                tlt.X -= Math.Abs(rect.Right - initRect.Right);
             if(initRect.Bottom < rect.Bottom)
                tlt.Y -= Math.Abs(rect.Bottom - initRect.Bottom);
             if(initRect.Top > rect.Top)
                tlt.Y += Math.Abs(rect.Top - initRect.Top);
        }
        private void Img_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isMouseLeftButtonDown = false;
        }

        private void Img_MouseLeave(object sender, MouseEventArgs e)
        {
            isMouseLeftButtonDown = false;
        }
    }
}
