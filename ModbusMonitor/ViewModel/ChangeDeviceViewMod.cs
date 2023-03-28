using ModbusMonitor.Classes;
using ModbusMonitor.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModbusMonitor.ViewModel
{
    class ChangeDeviceViewMod : INotifyPropertyChanged
    {
        private string address_ChD;
        private string name_ChD;
        private bool canCloseChange;
        private readonly Command closeChangeCommand;
        private readonly DeviceClass device;
        private readonly ChangeDeviceWindow window;
        string patternName = @"^\S+[a-zA-Z0-9-+_]$";
        string patternAddress = @"^\d{0,3}$";

        public ChangeDeviceViewMod(DeviceClass device, ChangeDeviceWindow window)
        {
            this.device = device;
            this.window = window;
            address_ChD = device.DeviceAdress_DC.ToString();
            name_ChD = device.DeviceName_DC;
            closeChangeCommand = new Command(CloseChange);
        }

        public ICommand CloseChangeCommand => closeChangeCommand;

        /// <summary>
        /// Закрыть окно изменения устройства.
        /// </summary>
        private void CloseChange()
        {
            if (Address_ChD != device.DeviceAdress_DC.ToString() || Name_ChD != device.DeviceName_DC)
            {
                device.DeviceAdress_DC =!string.IsNullOrEmpty(Address_ChD)?Int32.Parse(Address_ChD):1;
                device.DeviceName_DC = !string.IsNullOrEmpty(Name_ChD)? Name_ChD : device.DeviceName_DC;
                window.DialogResult = true;
                window.Close();
            }
        }

        /// <summary>
        /// Привязка к полю с адресом.
        /// </summary>
        public string Address_ChD
        {
            get => address_ChD;
            set
            {                
                if (!new Regex(patternAddress).IsMatch(value)|| 
                    Int32.Parse(value)<=0 || Int32.Parse(value) > Byte.MaxValue)
                {
                    return;
                }
                SetOptions(nameof(Address_ChD), ref address_ChD, value);
            }
        }
       
        /// <summary>
        /// Привяка к полю с именем.
        /// </summary>
        public string Name_ChD
        {
            get => name_ChD;
            set
            {
                if (!new Regex(patternName).IsMatch(value))
                {                    
                    return;
                }
                SetOptions(nameof(Name_ChD), ref name_ChD, value);
            }
        }

        public bool CanCloseChange
        {
            get => canCloseChange;
            set => SetOptions(nameof(CanCloseChange), ref canCloseChange, value);
        }
        /// <summary>
        /// Настройка изменяющихся свойств.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Property"></param>
        /// <param name="variable"></param>
        /// <param name="value"></param>
        private void SetOptions<T>(string Property, ref T variable, T value)
        {
            if (variable != null && !variable.Equals(value))
            {
                variable = value;
                OnPropertyChanged(new PropertyChangedEventArgs(Property));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

    }
}
