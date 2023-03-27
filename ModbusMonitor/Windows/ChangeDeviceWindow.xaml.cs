using ModbusMonitor.Classes;
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
using System.Windows.Shapes;
using ModbusMonitor.ViewModel;

namespace ModbusMonitor.Windows
{
    /// <summary>
    /// Логика взаимодействия для ChangeDeviceWindow.xaml
    /// </summary>
    public partial class ChangeDeviceWindow : Window
    {
        public ChangeDeviceWindow(DeviceClass device)
        {
            InitializeComponent();
            DataContext = new ChangeDeviceViewMod(device,this);
        }
    }
}
