using ModbusMonitor.Classes;
using ModbusMonitor.Controls;
using ModbusMonitor.Windows;
using ModbusMonitor.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Threading;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ModbusMonitor
{
    public partial class ViewModelMain : INotifyPropertyChanged
    {
        #region[Свойства-привязки]        
        /// <summary>
        /// Привязка к выбранным элементам дерева.
        /// </summary>
        public object SelectedItemTree
        {
            get => selectedItemTree;
            set
            {
                SetOptions(nameof(SelectedItemTree), ref selectedItemTree, value);
                SelectMapsForDevice(value);
            }
        }
        /// <summary>
        /// Доступность дерева.
        /// </summary>
        public bool TreeViewEnabled
        {
            get => treeViewEnabled;
            set => SetOptions(nameof(TreeViewEnabled), ref treeViewEnabled, value);
        }
        /// <summary>
        /// Количество запросов.
        /// </summary>
        public string NumberRequest
        {
            get => numberRequest;
            set => SetOptions(nameof(NumberRequest), ref numberRequest, value);
        }
        /// <summary>
        /// Корректные запросы.
        /// </summary>
        public string CorrectRequest
        {
            get => correctRequest;
            set => SetOptions(nameof(CorrectRequest), ref correctRequest, value);
        }
        /// <summary>
        /// Свойство-привязка отображения таблицы.
        /// </summary>
        public UserControlDevices Usercontrol
        {
            get => userControl;
            set
            {
                SetOptions(nameof(Usercontrol), ref userControl, value);
            }
        }
        /// <summary>
        /// Номер регистра по порядку.
        /// </summary>
        public int NumInOrder
        {
            get => numInOrder;
            set
            {
                SetOptions(nameof(NumInOrder), ref numInOrder, value);
            }
        }
        /// <summary>
        /// Адрес регистра.
        /// </summary>
        public int RegAddress
        {
            get => regAddress;
            set
            {
                SetOptions(nameof(RegAddress), ref regAddress, value);
            }
        }
        /// <summary>
        /// Адрес устройства.
        /// </summary>
        public int DeviceAddress
        {
            get => deviceAddress;
            set
            {
                SetOptions(nameof(DeviceAddress), ref deviceAddress, value);
            }
        }
        /// <summary>
        /// Название устройства.
        /// </summary>
        public string DeviceName
        {
            get => deviceName;
            set
            {
                SetOptions(nameof(DeviceName), ref deviceName, value);
            }
        }
        /// <summary>
        /// Формат данных.
        /// </summary>
        public string DataFormat
        {
            get => dataFormat;
            set
            {
                SetOptions(nameof(DataFormat), ref dataFormat, value);
            }
        }
        /// <summary>
        /// Название регистра.
        /// </summary>
        public string RegName
        {
            get => regName;
            set
            {
                SetOptions(nameof(RegName), ref regName, value);
            }
        }
        /// <summary>
        /// Значение регистра.
        /// </summary>
        public string RegValue
        {
            get => regValue;
            set
            {
                SetOptions(nameof(RegValue), ref regValue, value);
            }
        }
        #endregion
    }
}

