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
using ModbusMonitor.Classes;
using ModbusMonitor.ViewModel;

namespace ModbusMonitor.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddСellsWindow.xaml
    /// </summary>
    public partial class AddСellsWindow : Window
    {
        public AddСellsWindow(CellData cellData)
        {
            InitializeComponent();
            DataContext = new AddCellViewModel(cellData, this);
        }
    }
}
