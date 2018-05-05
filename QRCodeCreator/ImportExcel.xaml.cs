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

namespace QRCodeCreator
{
    /// <summary>
    /// ImportExcel.xaml 的交互逻辑
    /// </summary>
    public partial class ImportExcel : Window
    {
        private MainWindow frmMain { get; set; }

        public ImportExcel(MainWindow _mainWindow)
        {
            InitializeComponent();
            this.frmMain = _mainWindow;
            initEvent();
        }

        private void initEvent()
        {
            btnImport.Click += BtnImport_Click;
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            string path = @"D:\HoweDesktop\水路发运单.xlsx";
            System.Data.DataTable dt = new System.Data.DataTable();
            // System.Data.DataTable dt = Util.Excel.ExcelUtils_Aspose.Excel2DataTable(path);
            this.dg.DataContext = dt;
        }
    }
}
