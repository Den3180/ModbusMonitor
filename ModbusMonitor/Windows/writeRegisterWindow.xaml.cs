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
using System.Windows.Threading;
using ModbusMonitor.ViewModel;

namespace ModbusMonitor.Windows
{
    /// <summary>
    /// Логика взаимодействия для writeRegisterWindow.xaml
    /// </summary>
    public partial class WriteRegisterWindow : Window
    {
        public WriteRegisterWindow(params object[] objects)
        {
            InitializeComponent();
            object[] outgoingParam = new object[objects.Length+1];
            objects.CopyTo(outgoingParam,0);
            outgoingParam[^1] = this;
            DataContext = new WriteRegisterViewMod(outgoingParam);
        }
    }
}
