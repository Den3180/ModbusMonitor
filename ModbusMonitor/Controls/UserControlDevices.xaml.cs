using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ModbusMonitor;
using ModbusMonitor.ViewModel;
using ModbusMonitor.Classes;
using Microsoft.Win32;
using System.IO;
using Microsoft.Office.Interop.Excel;
using System.Windows.Automation;

namespace ModbusMonitor.Controls
{
    /// <summary>
    /// Логика взаимодействия для UserControlDevices.xaml
    /// </summary>
    public partial class UserControlDevices : UserControl
    {
        public UserControlDevices(params object[] objects)
        {
            InitializeComponent();           
            DataContext = new ControlDeviceView(objects);
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var sen = (DataGrid)sender;
            var sourceCommand = DataContext as ControlDeviceView;
            if (sen.CurrentCell.Column.Header.ToString() == "Значение" && sourceCommand.CanWriteRegister==true )
            {
                sourceCommand.WriteRegisterCommand.Execute(new object());
            }
            else if(sen.CurrentCell.Column.Header.ToString() != "Значение")
            {
                sourceCommand.ShowPropertiesCommand.Execute(new object());
            }
        }      
    }
}
