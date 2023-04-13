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
    /// Логика взаимодействия для PropertyRegisterWindow.xaml
    /// </summary>
    public partial class PropertyRegisterWindow : Window
    {
        public PropertyRegisterWindow(params object[] objects)
        {
            InitializeComponent();
            object[] outgoingParam = new object[objects.Length + 1];
            objects.CopyTo(outgoingParam, 0);
            outgoingParam[^1] = this;
            DataContext = new WriteRegisterViewMod(outgoingParam);
        }
    }
}
