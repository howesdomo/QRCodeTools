using System;
using System.Collections.Generic;
using System.Data;
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
        public EventHandler<SelectedCellEventArgs> SelectedCell;

        private void OnSelectedCell(string value)
        {
            if (SelectedCell != null)
            {
                SelectedCell.Invoke(this, new SelectedCellEventArgs(value));
            }
        }

        public ImportExcel()
        {
            InitializeComponent();
            initEvent();
        }

        private void initEvent()
        {
            ucsfExcelFile.SuccessEventHandler += new EventHandler<EventArgs>(excelFile_ImportSuccess);
        }

        private void excelFile_ImportSuccess(object sender, EventArgs e)
        {
            var ds = Util.Excel.ExcelUtils_Aspose.Excel2DataSetAsString
            (
                path: ucsfExcelFile.FileName,
                exportColumnName: true
            );

            tabCtrl.Items.Clear();

            for (int i = 0; i < ds.Tables.Count; i++)
            {
                TabItem toAdd = new TabItem();
                toAdd.Header = ds.Tables[i].TableName;

                Grid grid = new Grid();

                DataGrid dg = new DataGrid();
                dg.AutoGenerateColumns = true;
                dg.IsReadOnly = true;
                dg.CanUserSortColumns = true;

                dg.CanUserAddRows = false;
                dg.CanUserDeleteRows = false;
                dg.SelectionMode = DataGridSelectionMode.Extended;
                dg.SelectionUnit = DataGridSelectionUnit.CellOrRowHeader;

                dg.MouseUp += (senderObj, args) =>
                {                   
                    if (args.ChangedButton == MouseButton.Left)
                    {
                        string s = @"|";
                        StringBuilder sb = new StringBuilder();

                        DataGrid target = senderObj as DataGrid;
                        System.Diagnostics.Debug.WriteLine("*************************");
                        for (int index = 0; index < target.SelectedCells.Count; index++)
                        {
                            var item = target.SelectedCells[index];
                            string msg = GetCellValue(item).ToString();
                            System.Diagnostics.Debug.WriteLine(msg);
                            sb.Append(msg);
                            if (target.SelectedCells.Count > 1)
                            {
                                sb.Append(s);
                            }                           
                        }
                        System.Diagnostics.Debug.WriteLine("==========================");

                        this.OnSelectedCell(sb.ToString());
                    }
                };

                dg.ItemsSource = ds.Tables[i].DefaultView; // 简化以上获取 DataView 的方式

                grid.Children.Add(dg);

                toAdd.Content = grid;

                tabCtrl.Items.Add(toAdd);
            }

            tabCtrl.SelectedIndex = -1;

            if (tabCtrl.Items.Count > 0)
            {
                tabCtrl.SelectedIndex = 0;
            }
        }


        public object GetCellValue(DataGridCellInfo dataGridCellInfo)
        {
            object r = null;

            if (dataGridCellInfo != null)
            {
                DataGridBoundColumn column = dataGridCellInfo.Column as DataGridBoundColumn;
                if (column != null)
                {
                    FrameworkElement element = new FrameworkElement()
                    {
                        DataContext = dataGridCellInfo.Item
                    };

                    BindingOperations.SetBinding
                    (
                        target: element,
                        dp: FrameworkElement.TagProperty,
                        binding: column.Binding
                    );

                    r = element.Tag;
                }
            }

            return r;
        }
    }
}
