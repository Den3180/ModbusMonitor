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
using ModbusMonitor.ViewModel;
namespace ModbusMonitor.Controls
{
    /// <summary>
    /// Логика взаимодействия для UserControlPackages.xaml
    /// </summary>
    public partial class UserControlPackages : UserControl
    {
        public UserControlPackages()
        {
            InitializeComponent();
            DataContext = new ViewingPackagesViewModel();
        }
    }
}
