using ModbusMonitor.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace ModbusMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModelMain();
        }
        
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var dContext = DataContext as ViewModelMain;
            if (dContext.treeNodes.Count == 0) return;
            if(MessageBox.Show("Сохранить карту?", string.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)          
            {
                dContext.SaveMapcomman.Execute(null);
            }
        }       
    }
}
