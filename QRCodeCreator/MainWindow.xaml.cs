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
        // TODO 一个超长的纯数字, 能生成二维码, 但无法正常解释
        // 例如 23921238123912837198273891273987182379812893718923789127389179832798123891232738917983279812389123

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
            this.txtDecodeContent.SizeChanged += txtDecodeContent_SizeChanged;
            this.Closed += new EventHandler(MainWindow_Closed);
            this.img.MouseDown += new MouseButtonEventHandler(img_MouseDown);

            this.Activated += MainWindow_Activated;

            // BusinessLogic
            this.txtQRCodeContent.TextChanged += txtQRCodeContent_TextChanged;
            this.cbErrorCorrectionLevel.SelectionChanged += (o, e) => { optionChanged(); };
            this.cbCharacterSet.SelectionChanged += (o, e) => { optionChanged(); };

            this.btnImportExcel.Click += BtnImportExcel_Click;

            this.btnDecode.Click += BtnDecode_Click;

            txtDecodeContent.MouseDoubleClick += TxtDecodeContent_MouseDoubleClick;
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
        }

        void txtQRCodeContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // this.Height = 462 + e.NewSize.Height - 21.84;
            this.Height = 500 + e.NewSize.Height + txtDecodeContent.ActualHeight;
        }

        void txtDecodeContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Height = 500 + txtQRCodeContent.ActualHeight + e.NewSize.Height;
        }

        Util.ActionUtils.DebounceAction mDebounceAction = new Util.ActionUtils.DebounceAction();

        void txtQRCodeContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            mDebounceAction.Debounce(1000, drawQRCode, this.Dispatcher);
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

            if (cbErrorCorrectionLevel.SelectedValue is ComboBoxItem cbi1)
            {
                switch (cbi1.Tag.ToString())
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
            if (cbCharacterSet.SelectedValue is ComboBoxItem cbi2)
            {
                string tempEncoding = cbi2.Content.ToString();
                try
                {
                    characterSet = ZXing.Common.CharacterSetECI.getCharacterSetECIByName(tempEncoding).EncodingName;

                    #region 知识点 ZXing.Common.CharacterSetECI

                    // ZXing.Common.CharacterSetECI.getCharacterSetECIByName()
                    // 从网页 https://zxing.github.io/zxing/apidocs/com/google/zxing/common/CharacterSetECI.html#UnicodeBigUnmarked
                    // 了解到 zxing 只支持

                    //ASCII
                    //Big5
                    //Cp1250
                    //Cp1251
                    //Cp1252
                    //Cp1256
                    //Cp437
                    //EUC_KR
                    //GB18030
                    //ISO8859_1
                    //ISO8859_10
                    //ISO8859_11
                    //ISO8859_13
                    //ISO8859_14
                    //ISO8859_15
                    //ISO8859_16
                    //ISO8859_2
                    //ISO8859_3
                    //ISO8859_4
                    //ISO8859_5
                    //ISO8859_6
                    //ISO8859_7
                    //ISO8859_8
                    //ISO8859_9
                    //SJIS
                    //UnicodeBigUnmarked
                    //UTF8

                    #endregion
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}\r\n编码{tempEncoding}不存在", "捕获异常");
                }
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
            ImportExcel frm = new ImportExcel();
            frm.SelectedCell += new EventHandler<SelectedCellEventArgs>(handle_SelectedCell);
            frm.Show();
        }

        private void handle_SelectedCell(object sender, SelectedCellEventArgs args)
        {
            var textRange = new TextRange(this.txtQRCodeContent.Document.ContentStart, this.txtQRCodeContent.Document.ContentEnd);
            textRange.Text = args.SelectedValue;
            drawQRCode();
        }

        #endregion

        #region 解析粘贴板的图像

        ZXing.BarcodeReader mBarcodeReader { get; set; } = new ZXing.BarcodeReader();

        void BtnDecode_Click(object sender, RoutedEventArgs e)
        {
            System.Drawing.Bitmap bitmap = null;
            try
            {
                bitmap = Util.Drawing.DrawingUtils.BitmapSource2Bitmap(Clipboard.GetImage());
            }
            catch
            {
                return;
            }

            var r0 = mBarcodeReader.Decode(bitmap);
            if (r0 == null)
            {
                txtDecodeContent.Text = string.Empty;
                txtDecodeFormat.Text = string.Empty;

                MessageBox.Show("图像不能被解析");
            }
            else
            {
                txtDecodeContent.Text = r0.Text;

                switch (r0.BarcodeFormat)
                {
                    case ZXing.BarcodeFormat.QR_CODE:
                        {
                            r0.ResultMetadata.TryGetValue(ZXing.ResultMetadataType.ERROR_CORRECTION_LEVEL, out object v);
                            txtDecodeFormat.Text = $"条码类型: {r0.BarcodeFormat.ToString()}; 纠错等级: {v.ToString()}";
                        }
                        break;
                    default:
                        {
                            txtDecodeFormat.Text = $"条码类型: {r0.BarcodeFormat.ToString()}";
                        }
                        break;
                }
            }
        }

        private void TxtDecodeContent_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        #endregion
    }

}
