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
        }

        private void initEvent()
        {
            // UI
            this.txtQRCodeContent.SizeChanged += txtQRCodeContent_SizeChanged;

            // BusinessLogic
            this.txtQRCodeContent.TextChanged += txtQRCodeContent_TextChanged;
            this.cbErrorCorrectionLevel.SelectionChanged += (o, e) => { optionChanged(); };
            this.cbCharacterSet.SelectionChanged += (o, e) => { optionChanged(); };
        }

        void txtQRCodeContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Height = 462 + e.NewSize.Height - 21.84;
        }

        private void optionChanged()
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

            // string characterSet = "UTF-8";
            string characterSet = "gb2312";
            if (cbCharacterSet.SelectedValue is ComboBoxItem)
            {
                ComboBoxItem cbi = cbCharacterSet.SelectedValue as ComboBoxItem;
                characterSet = cbi.Tag.ToString();
            }

            ZXing.QrCode.QrCodeEncodingOptions options = new ZXing.QrCode.QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = characterSet,
                Width = 870,
                Height = 870,
                ErrorCorrection = ecl
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
            catch (Exception ex)
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
                return BitmapToBitmapImage(writer.Write(content));
            }
        }

        private BitmapSource BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            IntPtr ip = bitmap.GetHbitmap();//从GDI+ Bitmap创建GDI位图对象

            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            return bitmapSource;
        }
    }
}
