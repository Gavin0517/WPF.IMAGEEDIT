using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFPhotoEditorTool.ViewModels;

namespace WPFPhotoEditorTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                if (DataContext == null)
                {
                 
                    DataContext = _viewModel;
                }
            };
            Unloaded += (s, e) =>
            {
                this.DataContext = null;
            };
            _viewModel = new MainWindowViewModel();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (this.WindowState == WindowState.Maximized)
            {
                var screenWidth = System.Windows.SystemParameters.WorkArea.Width;
                var screenHeight = System.Windows.SystemParameters.WorkArea.Height;

                ScaleTransform st = new ScaleTransform(screenWidth / this.Width, screenHeight / this.Height, 0, 0);
                inkCanvasMeasure.RenderTransform = st;

            }
            else if (this.WindowState == WindowState.Normal)
            {
                ScaleTransform st = new ScaleTransform(1, 1, 0, 0);
                inkCanvasMeasure.RenderTransform = st;
            }


        }
    }
}
