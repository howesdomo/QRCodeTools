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

namespace QRToolsClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Group> groupList;
        ZXing.BarcodeWriter writer;

        public MainWindow()
        {
            InitializeComponent();

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
            initData();

            string a = "我在重庆称重量".ToPinYin();
            a = "单志强".ToPinYin();
        }

        private void initData()
        {
            
            // Load Data

            if (groupList == null)
            {
                groupList = new List<Group>();
            }

            Group rootGroup = new Group()
            {
                ID = Guid.Empty,
                Code = "默认分组",
                EntryTime = DateTime.Now,
                Remark = "默认分组",
                LogList = new List<Log>() 
                {
                    new Log() 
                    {
                        ID = new Guid(),
                        Code = "1238192319028391230123", 
                        Remark = "",
                        EntryTime = DateTime.Now,
                    },
                    new Log() 
                    {
                        ID = new Guid(),
                        Code = "3322443", 
                        Remark = "",
                        EntryTime = DateTime.Now,
                    }
                }
            };

            groupList.Add(rootGroup);

            this.tv.DataContext = groupList;
        }

        private void initEvent()
        {
            this.txtQRCodeContent.TextChanged += txtQRCodeContent_TextChanged;
            this.cbErrorCorrectionLevel.SelectionChanged += cbErrorCorrectionLevel_SelectionChanged;
            this.txtQRCodeContent.SizeChanged += txtQRCodeContent_SizeChanged;
        }

        void txtQRCodeContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Height = 462 + e.NewSize.Height - 21.84;
        }

        void cbErrorCorrectionLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ZXing.QrCode.Internal.ErrorCorrectionLevel ecl = ZXing.QrCode.Internal.ErrorCorrectionLevel.L;

            if (cbErrorCorrectionLevel.SelectedValue is ComboBoxItem)
            {
                ComboBoxItem cbi = cbErrorCorrectionLevel.SelectedValue as ComboBoxItem;
                switch (cbi.Content.ToString())
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

            ZXing.QrCode.QrCodeEncodingOptions options = new ZXing.QrCode.QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
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
            return textRange.Text;
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
