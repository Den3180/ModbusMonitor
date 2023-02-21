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
    /// Логика взаимодействия для SearchAddressWindow.xaml
    /// </summary>
    public partial class SearchAddressWindow : Window
    {
        private string _speed;
        private string _dataBits;
        private string _parity;
        private string _stopBits;

        public SearchAddressWindow(ModbusRTU modbusRTU)
        {
            InitializeComponent();
            DataContext = new SearchAddrViewMod(modbusRTU, this)
            {
                Speed = _speed,
                DataBits=_dataBits,
                ParityS=_parity,
                StopBits=_stopBits
            };            
        }
        /// <summary>
        /// Кнопки выбора скорости.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButtonSpeed_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (DataContext != null)
            {
                ((SearchAddrViewMod)DataContext).Speed = radioButton.Content.ToString();
            }
            else 
            {
                _speed = radioButton.Content.ToString();
            }
        }
        /// <summary>
        /// Кнопки выбора DataBits.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButtonDataBits_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (DataContext != null)
            {
                ((SearchAddrViewMod)DataContext).DataBits = radioButton.Content.ToString();
            }
            else 
            {
                _dataBits = radioButton.Content.ToString();
            }
        }
        /// <summary>
        /// Кнопки выбора четности.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButtonParity_Checked(object sender,RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (DataContext != null)
            {
                ((SearchAddrViewMod)DataContext).ParityS = radioButton.Content.ToString();
            }
            else
            {
                _parity = radioButton.Content.ToString();
            }
        }
        /// <summary>
        /// Кнопки выбора стопбита.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButtonStopBits_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (DataContext != null)
            {
                ((SearchAddrViewMod)DataContext).StopBits = radioButton.Content.ToString();
            }
            else
            {
                _stopBits = radioButton.Content.ToString();
            }
        }

    }
}
