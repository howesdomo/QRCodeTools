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
        ZXing.BarcodeWriter writer;

        public MainWindow()
        {
            InitializeComponent();

            this.Title = "二维码生成器 - V {0}".FormatWith(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());


            writer = new ZXing.BarcodeWriter();

            ZXing.QrCode.QrCodeEncodingOptions options = new ZXing.QrCode.QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = 870,
                Height = 870,
                ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.L,
            };

            writer.Format = ZXing.BarcodeFormat.QR_CODE;
            writer.Options = options;

            initEvent();

            this.txtQRCodeContent.AppendText("测试");
        }

        private void initEvent()
        {
            // UI
            this.txtQRCodeContent.SizeChanged += txtQRCodeContent_SizeChanged;
            this.Closed += new EventHandler(MainWindow_Closed);
            this.img.MouseDown += new MouseButtonEventHandler(img_MouseDown);

            // BusinessLogic
            this.txtQRCodeContent.TextChanged += txtQRCodeContent_TextChanged;
            this.cbErrorCorrectionLevel.SelectionChanged += (o, e) => { optionChanged(); };
            this.cbCharacterSet.SelectionChanged += (o, e) => { optionChanged(); };

            this.btnImportExcel.Click += BtnImportExcel_Click;
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            this.img.Source = null;
            closeAllFrmViewImage();
            deleteAllTempFile();
        }

        void txtQRCodeContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Height = 462 + e.NewSize.Height - 21.84;
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

            writer.Options = options;

            this.img.Source = drawQRCode(this.getDocumentStringFormRichTextBox(this.txtQRCodeContent));
        }

        private string getDocumentStringFormRichTextBox(RichTextBox rtb)
        {
            TextRange textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            string r = textRange.Text;
            try
            {
                // 由于 RichTextBox 自带\r\n 故去掉2位
                r = r.Substring(0, r.Length - 2);
            }
            catch (Exception)
            {
                // MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                r = textRange.Text;
            }
            return r;
        }

        void txtQRCodeContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            RichTextBox target = sender as RichTextBox;
            this.img.Source = drawQRCode(this.getDocumentStringFormRichTextBox(target));
        }

        private BitmapSource drawQRCode(string content)
        {
            if (string.IsNullOrWhiteSpace(content) == true)
            {
                return null;
            }
            else
            {
                QRCodeModel qrCodeModel = new QRCodeModel();
                qrCodeModel.ID = Guid.NewGuid();
                qrCodeModel.Content = content;
                qrCodeModel.UpdatedTime = DateTime.Now;
                qrCodeModel.ErrorCorrectionLevel = ZXing.QrCode.Internal.ErrorCorrectionLevel.L; // TODO :
                qrCodeModel.CharacterSet = "UTF8";

                System.Drawing.Bitmap bitmap = writer.Write(content);
                BitmapSource r = BitmapToBitmapImage(bitmap);
                bitmap.Dispose(); // TODO 进一步优化
                return r;
            }
        }

        /// <summary>
        /// 条码图片临时存放路径
        /// </summary>
        string mTempDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HoweSoftware", "QRCodeCreator", "temp");

        private BitmapSource BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            #region 使用内存不断累积, 并且无法回收
            
            //IntPtr ip = bitmap.GetHbitmap(); // 从GDI+ Bitmap创建GDI位图对象
            //BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
            //System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            #endregion

            if (System.IO.Directory.Exists(mTempDirectory) == false)
            {
                System.IO.Directory.CreateDirectory(mTempDirectory);
            }

            string path = System.IO.Path.Combine(mTempDirectory, "{0}.png".FormatWith(Guid.NewGuid().ToString()));
            bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            return new BitmapImage(new Uri(path, UriKind.Absolute)); // bitmapSource;
        }


        private List<FrmViewImage> mFrmViewImageList = new List<FrmViewImage>();

        private DateTime? mouseDownDateTime = null;

        void img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mouseDownDateTime.HasValue == true)
            {
                DateTime now = DateTime.Now;
                double value = TimeSpan.FromTicks(now.Ticks).TotalMilliseconds - TimeSpan.FromTicks(mouseDownDateTime.Value.Ticks).TotalMilliseconds;
                if (value.CompareTo(1000) < 0)
                {
                    mouseDownDateTime = null;

                    // 获取当前鼠标位置
                    Point mousePoint = Mouse.GetPosition(e.Source as FrameworkElement);

                    FrmViewImage frm = new FrmViewImage(mousePoint.X + this.Top, mousePoint.Y + this.Left, this.getDocumentStringFormRichTextBox(this.txtQRCodeContent), ((BitmapSource)img.Source).CloneCurrentValue());
                    mFrmViewImageList.Add(frm);
                    frm.Show();

                    return;
                }
            }

            mouseDownDateTime = DateTime.Now;
        }

        private void closeAllFrmViewImage()
        {
            foreach (var item in mFrmViewImageList)
            {
                item.Close();
            }
        }

        private void deleteAllTempFile()
        {
            
            System.IO.Directory.Delete(this.mTempDirectory, true);
        }



        private void BtnImportExcel_Click(object sender, RoutedEventArgs e)
        {
            ImportExcel frm = new ImportExcel(this);
            frm.Show();
        }
    }

}
