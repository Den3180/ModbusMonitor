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
        private int numInOrder;//Номера регистров по порядку не зависимо от типа.
        private int regAddress;
        private int deviceAddress;//Адрес устройства в области "Добавления регистров".
        private string deviceName = "нет данных";
        private string dataFormat = "нет данных";
        private string regName = "нет данных";
        private string regValue = "нет данных";
        private string correctRequest = "0"; //Корректные запросы.
        private string numberRequest = "0";  //Общее количество запросов.
        private object selectedItemTree;
        private UserControlDevices userControl;

        public ObservableCollection<GroupsTreeNode> treeNodes;//Источник данных для дерева.
        public IEnumerable<GroupsTreeNode> TreeNodes => treeNodes;//Свойство данных дерева. 

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

        /// <summary>
        /// Изменение доступности команд.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewModelMain_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == null)
            {
                return;
            }
            if (e.PropertyName.Equals(nameof(CanDisablePoll)))
            {
                disablePollCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName.Equals(nameof(CanOpenLog)))
            {
                openLogCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName.Equals(nameof(CanCreateConnect)))
            {
                createConnectCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName.Equals(nameof(CanConnection)))
            {
                connectionCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName.Equals(nameof(CanDisconnection)))
            {
                disconnectionCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName.Equals(nameof(CanRequest)))
            {
                listenPortCommand.RaiseCanExecuteChanged();
                sendRequestCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName.Equals(nameof(CanWriteRegister)))
            {
                writeRegisterCommand.RaiseCanExecuteChanged();
            }
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

