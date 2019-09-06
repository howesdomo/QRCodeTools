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
using System.Windows.Shapes;
using System.Threading.Tasks;


namespace QRCodeCreator
{
    /// <summary>
    /// FrmViewImage.xaml 的交互逻辑
    /// </summary>
    public partial class FrmViewImage : Window
    {
        private string mQRCodeTextContent;
        private BitmapSource mOrginalImage;

        public FrmViewImage(double _Top, double _Left, string _QRCodeTextContent, BitmapSource bitmapSource)
        {
            InitializeComponent();
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.Topmost = true;
            this.ShowInTaskbar = false;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;

            this.Top = _Top;
            this.Left = _Left;

            mQRCodeTextContent = _QRCodeTextContent;
            mOrginalImage = bitmapSource;

            this.img.Source = mOrginalImage;
            this.img.ToolTip = mQRCodeTextContent;

            this.calcCurrentScale();
            this.initEvent();
        }

        private void initEvent()
        {
            // 长按鼠标左键移动窗口
            this.MouseMove += (o, e) => { MouseMoveAndLeftButtonPressed(e); };

            // 右键菜单
            this.img.PreviewMouseRightButtonDown += new MouseButtonEventHandler(img_PreviewMouseRightButtonDown);

            // 菜单
            // 复制图片到粘贴板
            // 图片另存为
            // 添加到本次程序的历史记录中
            // 图片尺寸信息 | DPI 等等
            // 关闭当前窗口

            // 滚轮缩放
            this.img.MouseWheel += new MouseWheelEventHandler(img_MouseWheel);
        }

        #region 长按鼠标左键移动窗口

        private void MouseMoveAndLeftButtonPressed(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        #endregion

        #region 滚轮缩放

        /// <summary>
        /// 初始缩放比例
        /// </summary>
        double initScale = 0;

        /// <summary>
        /// 当前缩放比例
        /// </summary>
        double currentScale = 0;

        /// <summary>
        /// 计算当前缩放比例
        /// </summary>
        private void calcCurrentScale()
        {
            var t1 = this.img.Width / mOrginalImage.Width * 100;
            // var t2 = this.img.Height / mOrginalImage.Height * 100;

            initScale = Math.Floor(t1);
            currentScale = Math.Floor(t1);
        }

        double wheelPlusMinusValue = 10;
        void img_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (currentScale + wheelPlusMinusValue <= 500)
                {
                    currentScale += wheelPlusMinusValue;
                }
            }
            else
            {
                if (currentScale - wheelPlusMinusValue > 0)
                {
                    currentScale -= wheelPlusMinusValue;
                }
            }

            Console.WriteLine($"当前缩放比例 {currentScale}");

            img.Width = mOrginalImage.Width * currentScale / 100;
            img.Height = mOrginalImage.Height * currentScale / 100;

            this.Width = img.Width;
            this.Height = img.Height;

        }

        #endregion

        #region 右键菜单

        void img_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (img.ContextMenu == null)
            {
                ContextMenu cm = new ContextMenu();

                MenuItem mi_copyQRCodeText = new MenuItem();
                mi_copyQRCodeText.Header = "复制QRCode文本";
                mi_copyQRCodeText.Click += (a, b) => { Clipboard.SetText(mQRCodeTextContent); };

                MenuItem mi_imgInfo = new MenuItem();
                mi_imgInfo.Header = "{0:N0} x {1:N0}".Format(Math.Floor(this.Width), Math.Floor(this.Height));
                mi_imgInfo.IsEnabled = false;

                MenuItem mi_copyImage = new MenuItem();
                mi_copyImage.Header = "复制图像";
                mi_copyImage.Click += (a, b) => { Clipboard.SetImage((BitmapSource)img.Source); };

                MenuItem mi_SaveAs = new MenuItem();
                mi_SaveAs.Header = "图像另存为";
                mi_SaveAs.Click += (a, b) =>
                {
                    Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
                    dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    dialog.Filter = "*.jpg|*.jpg|*.bmp|*.bmp|*.png|*.png";
                    var r = dialog.ShowDialog();
                    if (r.HasValue && r.Value == true)
                    {
                        this.SaveImage(dialog.FileName, mOrginalImage);
                    }
                };

                MenuItem mi_close = new MenuItem();
                mi_close.Header = "关闭";
                mi_close.Click += (a, b) => { this.Close(); };

                //cm.Items.Add(mi_QRCodeText);
                cm.Items.Add(mi_copyQRCodeText);
                cm.Items.Add(mi_imgInfo);
                cm.Items.Add(new Separator());
                cm.Items.Add(mi_copyImage);
                cm.Items.Add(mi_SaveAs);
                cm.Items.Add(new Separator());
                cm.Items.Add(mi_close);

                img.ContextMenu = cm;
            }
            else
            {
                // 更新图片大小信息
                (img.ContextMenu.Items[1] as MenuItem).Header = "{0:N0} x {1:N0}".Format(Math.Floor(this.Width), Math.Floor(this.Height));
            }
        }

        #endregion

        public void SaveImage(string filePath, BitmapSource imageSource)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageSource));
            encoder.Save(ms);

            System.Drawing.Bitmap bp = new System.Drawing.Bitmap(ms);
            bp.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            ms.Close();

            sw.Stop();
            Console.WriteLine($"用时 {sw.Elapsed.Milliseconds / 1000} 秒");
        }

    }
}
