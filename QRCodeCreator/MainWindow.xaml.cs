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
using System.Runtime.InteropServices;

namespace QRCodeCreator
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static object _Input_Lock_ = new object();

        System.Timers.Timer mTimer { get; set; } = new System.Timers.Timer();

        ZXing.BarcodeWriter mBarcodeWriter { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            this.Title = $"二维码生成器 - V {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}";

            mBarcodeWriter = new ZXing.BarcodeWriter();

            ZXing.QrCode.QrCodeEncodingOptions options = new ZXing.QrCode.QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = 870,
                Height = 870,
                ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.L,
            };

            mBarcodeWriter.Format = ZXing.BarcodeFormat.QR_CODE;
            mBarcodeWriter.Options = options;

            initEvent();

            this.txtQRCodeContent.AppendText("测试");                        
        }

        private void initEvent()
        {
            // UI
            this.txtQRCodeContent.SizeChanged += txtQRCodeContent_SizeChanged;
            this.Closed += new EventHandler(MainWindow_Closed);
            this.img.MouseDown += new MouseButtonEventHandler(img_MouseDown);

            this.Activated += MainWindow_Activated;

            mTimer.Interval = 1000;
            mTimer.Elapsed += mTimer_Elapsed_txtQRCodeContent;
            mTimer.Start();

            // BusinessLogic
            this.txtQRCodeContent.TextChanged += txtQRCodeContent_TextChanged;
            this.cbErrorCorrectionLevel.SelectionChanged += (o, e) => { optionChanged(); };
            this.cbCharacterSet.SelectionChanged += (o, e) => { optionChanged(); };

            this.btnImportExcel.Click += BtnImportExcel_Click;
        }

        #region UI 事件

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            this.txtQRCodeContent.Focus();
            this.txtQRCodeContent.SelectAll();
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            this.img.Source = null;
            closeAllFrmViewImage();

            mTimer.Elapsed -= mTimer_Elapsed_txtQRCodeContent;
            mTimer.Stop();
        }

        void txtQRCodeContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Height = 462 + e.NewSize.Height - 21.84;
        }

        void txtQRCodeContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            lastInputDateTime = DateTime.Now;
        }

        #endregion

        /// <summary>
        /// 获取 RichTextbox 信息
        /// </summary>
        /// <param name="rtb"></param>
        /// <returns></returns>
        private string getDocumentStringFormRichTextBox(RichTextBox rtb)
        {
            TextRange textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            string r = textRange.Text;

            // 减少系统抛错
            if (r.Length > 2) // 由于 RichTextBox 自带\r\n 故去掉2位
            {
                r = r.Substring(0, r.Length - 2);
            }

            return r;
        }

        #region 下拉框选项更改后, 获取最新 Writer 配置

        private void optionChanged()
        {
            Tuple<ZXing.QrCode.Internal.ErrorCorrectionLevel, string> item = getSelectedOptions();

            ZXing.QrCode.QrCodeEncodingOptions options = new ZXing.QrCode.QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = item.Item2,
                Width = 870,
                Height = 870,
                ErrorCorrection = item.Item1
            };

            mBarcodeWriter.Options = options;

            drawQRCode();
        }

        private Tuple<ZXing.QrCode.Internal.ErrorCorrectionLevel, string> getSelectedOptions()
        {
            ZXing.QrCode.Internal.ErrorCorrectionLevel ecl = ZXing.QrCode.Internal.ErrorCorrectionLevel.L;

            if (cbErrorCorrectionLevel.SelectedValue is ComboBoxItem)
            {
                ComboBoxItem cbi = cbErrorCorrectionLevel.SelectedValue as ComboBoxItem;
                switch (cbi.Tag.ToString())
                {
                    case "L":
                        ecl = ZXing.QrCode.Internal.ErrorCorrectionLevel.L; break;
                    case "M":
                        ecl = ZXing.QrCode.Internal.ErrorCorrectionLevel.M; break;
                    case "Q":
                        ecl = ZXing.QrCode.Internal.ErrorCorrectionLevel.Q; break;
                    case "H":
                        ecl = ZXing.QrCode.Internal.ErrorCorrectionLevel.H; break;
                }
            }

            string characterSet = "UTF-8";
            if (cbCharacterSet.SelectedValue is ComboBoxItem)
            {
                ComboBoxItem cbi = cbCharacterSet.SelectedValue as ComboBoxItem;
                characterSet = cbi.Tag.ToString();
            }

            return new Tuple<ZXing.QrCode.Internal.ErrorCorrectionLevel, string>(ecl, characterSet);
        }

        #endregion

        #region 绘制二维码

        private void drawQRCode()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                string content = this.getDocumentStringFormRichTextBox(this.txtQRCodeContent);
                this.img.Source = zxingDrawQRCode(content);
            }));
        }

        private BitmapSource zxingDrawQRCode(string content)
        {
            if (string.IsNullOrWhiteSpace(content) == true)
            {
                return null;
            }
            else
            {
                System.Drawing.Bitmap bitmap = mBarcodeWriter.Write(content);
                BitmapSource r = bitmap2BitmapImage(bitmap);
                bitmap.Dispose();
                return r;
            }
        }

        private BitmapSource bitmap2BitmapImage(System.Drawing.Bitmap bitmap)
        {
            BitmapImage r = new BitmapImage();
            r.BeginInit();
            r.CacheOption = BitmapCacheOption.OnLoad;
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            r.StreamSource = ms;
            r.EndInit();

            return r;
        }

        #endregion

        #region 监控输入时间 减少生成二维码的次数

        DateTime? lastInputDateTime = null;
        DateTime? secondLastInputDateTime = null;

        List<DateTime> inputDateTimeList = new List<DateTime>();

        private void mTimer_Elapsed_txtQRCodeContent(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (_Input_Lock_)
            {
                DateTime intoLockMethodDateTime = DateTime.Now;

                string msg = $"lock (_Input_Lock_)";
                System.Diagnostics.Debug.WriteLine(msg);

                if (lastInputDateTime.HasValue == false)
                {
                    msg = $"lastInputDateTime == null";
                    System.Diagnostics.Debug.WriteLine(msg);

                    return;
                }

                if (secondLastInputDateTime.HasValue == false)
                {
                    secondLastInputDateTime = lastInputDateTime;

                    msg = $"=== 通过 === 校验, secondLastInputDateTime == null";
                    System.Diagnostics.Debug.WriteLine(msg);

                    lastInputDateTime = null;
                    drawQRCode();
                    return;
                }

                // 1 两次输入的时间在 1 秒之内
                if (new TimeSpan(lastInputDateTime.Value.Ticks - secondLastInputDateTime.Value.Ticks).TotalMilliseconds < TimeSpan.FromMilliseconds(1000).TotalMilliseconds)
                {
                    secondLastInputDateTime = lastInputDateTime;

                    msg = $"时间间隔校验1 *** 失败 ***";
                    System.Diagnostics.Debug.WriteLine(msg);
                    return;
                }

                // 2 最后一次输入时间 和 intoLockMethod 隔现在也在一秒之内
                if (new TimeSpan(intoLockMethodDateTime.Ticks - lastInputDateTime.Value.Ticks).TotalMilliseconds < TimeSpan.FromMilliseconds(1000).TotalMilliseconds)
                {
                    msg = $"时间间隔校验2 *** 失败 ***";
                    System.Diagnostics.Debug.WriteLine(msg);
                    return;
                }

                msg = $"=== 通过 === 校验";
                System.Diagnostics.Debug.WriteLine(msg);

                // 通过验证执行方法
                lastInputDateTime = null;
                secondLastInputDateTime = null;
                drawQRCode();
            }
        }

        #endregion

        #region 生成窗口 ( 1秒内对准图片控件点击左键2次, 生成窗口 )

        private List<FrmViewImage> mFrmViewImageList = new List<FrmViewImage>();

        private DateTime? mMouseDownDateTime = null;

        void img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (mMouseDownDateTime.HasValue == true)
                {
                    DateTime now = DateTime.Now;
                    double value = TimeSpan.FromTicks(now.Ticks).TotalMilliseconds - TimeSpan.FromTicks(mMouseDownDateTime.Value.Ticks).TotalMilliseconds;
                    if (value.CompareTo(1000) < 0)
                    {
                        mMouseDownDateTime = null;

                        // 获取当前鼠标位置
                        Point mousePoint = Mouse.GetPosition(e.Source as FrameworkElement);

                        FrmViewImage frm = new FrmViewImage(mousePoint.X + this.Top, mousePoint.Y + this.Left, this.getDocumentStringFormRichTextBox(this.txtQRCodeContent), ((BitmapSource)img.Source).CloneCurrentValue());
                        mFrmViewImageList.Add(frm);
                        frm.Show();

                        return;
                    }
                }

                mMouseDownDateTime = DateTime.Now;
            }
        }

        private void closeAllFrmViewImage()
        {
            foreach (var item in mFrmViewImageList)
            {
                item.Close();
            }
        }

        #endregion

        #region 待处理 导入Excel文件

        private void BtnImportExcel_Click(object sender, RoutedEventArgs e)
        {
            ImportExcel frm = new ImportExcel(this);
            frm.Show();
        }

        #endregion
    }

}
