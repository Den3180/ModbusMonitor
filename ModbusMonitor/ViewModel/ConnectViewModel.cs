using ModbusMonitor.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using ModbusMonitor.Classes;
using System.Threading;

namespace ModbusMonitor.ViewModel
{
    public enum CommandTypeConnection
    {
        None,
        Add,
        AddConnection
    }
    public class ConnectViewModel : INotifyPropertyChanged
    {
        private string deviceName_VM = string.Empty;
        private string adressIP_VM = string.Empty;
        private string port_VM = "нет доступных портов";
        private string selectedItem = string.Empty;
        private string headerCombobox = "--выберите подключение--";
        private string headerAddress=string.Empty;
        private int speedPort_VM;
        private int wordLength_VM;
        private int timeoutRead_VM;
        private int timeoutWrite_VM;
        private bool canAddConnect;
        private bool canAdd;
        private bool canTextIP;
        private Parity parity_VM;
        private StopBits stopBits_VM;
        private DeviceClass device;
        public ICollection<string> sourceTypeConnect = new ObservableCollection<string>();
        private ConnectSettingWindow window;
        private readonly Command addConnectCommand;
        private readonly Command addCommand;
        private readonly Command canselCommand;
        private readonly Command refreshPortCommand;
        private readonly ModbusRTUASCII modbusMaster;
        private readonly List<string> typeOfAddress = new List<string>() 
        {
            "Адрес",
            "Адрес(Modbus RTU/ASCII)",
            "IP-адрес/Хост(Modbus IP)"
        };
        


        public ConnectViewModel(DeviceClass device,ModbusRTUASCII modbus, ConnectSettingWindow window)
        {
            this.device = device;
            this.window = window;
            modbusMaster = modbus;
            DeviceName_VM = device.DeviceName_DC;
            HeaderAddress = typeOfAddress[0];
            SetSourceConnect();
            addConnectCommand = new Command(AddConnect, () => CanAddConnect);
            addCommand = new Command(Add, () => CanAdd);
            canselCommand = new Command(Cansel);
            refreshPortCommand = new Command(RefreshPort);
            PropertyChanged += ConnectViewModel_PropertyChanged;            
        }

        public IEnumerable<string> SourceTypeConnect => sourceTypeConnect;
        public ICommand AddConnectCommand => addConnectCommand;
        public ICommand AddCommand => addCommand;
        public ICommand CanselCommand => canselCommand;
        public ICommand RefreshPortCommand => refreshPortCommand;

        /// <summary>
        /// Обновление доступных портов.
        /// </summary>
        private void RefreshPort()
        {           
            modbusMaster.SendResponsePort(ModbusRTUASCII.SettingPortStart, device.DeviceAdress_DC);
            if (ModbusRTUASCII.PortsEnabled.Count > 0)
            {
                sourceTypeConnect.Remove(port_VM);
                foreach(var item in ModbusRTUASCII.PortsEnabled)
                {
                    if (!sourceTypeConnect.Contains(item))
                    {
                        sourceTypeConnect.Add(item);
                    }
                }                              
            }
        }
        /// <summary>
        /// Метод кнопки "Добавить и подключить".
        /// </summary>
        private void AddConnect()
        {
            SetConnectionDevice();
            window.Content = CommandTypeConnection.AddConnection;           
            window.Close();
        }
        /// <summary>
        /// Метод кнопки "Добавить".
        /// </summary>
        private void Add()
        {
            SetConnectionDevice();
            window.Content = CommandTypeConnection.Add;           
            window.Close();
        }
        /// <summary>
        /// Метод кнопки "Отмена".
        /// </summary>
        private void Cansel()
        {
            window.Content = CommandTypeConnection.None;
            window.Close();
        }
        /// <summary>
        /// Определения типа подключения.
        /// </summary>
        /// <param name="conn"></param>
        private void SelectConnections(string conn)
        {
            if (conn == null)
            {
                return;
            }
            string connectPatternCOM = @"^(COM?)[1-9][0-9]?$";//Шаблон для последовательного порта.
            string connectPatternIP = @"\d{0,3}.\d{0,3}.\d{0,3}";//Шаблон для IP подключения.
                                                                 //Если есть совпадение по шаблону.                                                   
            if (new Regex(connectPatternCOM).IsMatch(conn))
            {
                SpeedPort_VM = device.ConnectFromMap.SpeedPort;
                Parity_VM = (Parity)device.ConnectFromMap.Parity;
                StopBits_VM = (StopBits)device.ConnectFromMap.Stop_Bit;
                WordLength_VM = device.ConnectFromMap.LenghtWord;
                TimeoutRead_VM = device.ConnectFromMap.TimeOutRead;
                TimeoutWrite_VM = device.ConnectFromMap.TimeOutWrite;
                device.ConnectionType = ConnectionType.RTU;
                CanTextIP = false;
                CanAddConnect = true;
                CanAdd = true;
                CanTextIP = true;
                HeaderAddress = typeOfAddress[1];
                Port_VM = conn;
                AdressIP_VM = device.DeviceAdress_DC.ToString();
                device.ConnectFromMap.PortType = Port_VM;
                sourceTypeConnect.Remove(HeaderCombobox);
                if (!ModbusRTUASCII.PortsEnabled.Contains(SelectedItem))
                {
                    ModbusRTUASCII.PortsEnabled.Add(SelectedItem);
                }
            }
            else if (new Regex(connectPatternIP).IsMatch(conn))
            {
                HeaderAddress = typeOfAddress[2];
                CanTextIP = true;//Включение строки с IP адресом на форме.
            }
        }

        /// <summary>
        /// Установка выбраных настроек в текущее устройство. 
        /// </summary>
        private void SetConnectionDevice()
        {
            device.ConnectFromMap.SpeedPort = SpeedPort_VM;
            device.ConnectFromMap.Parity = (int)Parity_VM;
            device.ConnectFromMap.Stop_Bit = (int)StopBits_VM;
            device.ConnectFromMap.LenghtWord = WordLength_VM;
            device.ConnectFromMap.TimeOutRead = TimeoutRead_VM;
            device.ConnectFromMap.TimeOutWrite = TimeoutWrite_VM;

            device.DeviceName_DC = DeviceName_VM;
            device.DeviceAdress_DC = Int32.Parse(AdressIP_VM);
        }

        /// <summary>
        /// Поиск и добавление всех портов на устройстве.
        /// </summary>
        private void SetSourceConnect()
        {
            sourceTypeConnect.Add(HeaderCombobox); //Добавление заголовка в Combobox.
            string[] temp = ModbusRTUASCII.PortsEnabled.ToArray();
            //string[] temp = ModbusRTUASCII.GetListPorts();
            if (temp.Length == 0)
            {
                sourceTypeConnect.Add(Port_VM);
            }
            foreach (var item in temp)
            {
                sourceTypeConnect.Add(item);                
            }
        }

        /// <summary>
        /// Заголовок combobox.
        /// </summary>
        public string HeaderCombobox
        {
            get => headerCombobox;
            set
            {
                SetOptions(nameof(HeaderCombobox), ref headerCombobox, value);
            }
        }

        /// <summary>
        /// Метка адреса.
        /// </summary>
        public string HeaderAddress
        {
            get => headerAddress;
            set
            {
                SetOptions(nameof(HeaderAddress), ref headerAddress, value);
            }
        }

        /// <summary>
        /// Выбранный элемент combobox.
        /// </summary>
        public string SelectedItem
        {
            get => selectedItem;
            set
            {
                SetOptions(nameof(SelectedItem), ref selectedItem, value);
                if (!string.IsNullOrEmpty(SelectedItem))
                {
                    SelectConnections(value);                    
                }
            }
        }
        /// <summary>
        /// Привязка имя устройства.
        /// </summary>
        public string DeviceName_VM
        {
            get => deviceName_VM;
            set
            {
                SetOptions(nameof(DeviceName_VM), ref deviceName_VM, value);
            }
        }
        /// <summary>
        /// Привязка IP-адрес.
        /// </summary>
        public string AdressIP_VM
        {
            get => adressIP_VM;
            set
            {
                SetOptions(nameof(AdressIP_VM), ref adressIP_VM, value);
            }
        }
        /// <summary>
        /// Привязка COM-порт.
        /// </summary>
        public string Port_VM
        {
            get => port_VM;
            set
            {
                SetOptions(nameof(Port_VM), ref port_VM, value);
            }
        }
        /// <summary>
        /// Привязка скорость порта.
        /// </summary>
        public int SpeedPort_VM
        {
            get => speedPort_VM;
            set
            {
                SetOptions(nameof(SpeedPort_VM), ref speedPort_VM, value);
            }
        }
        /// <summary>
        /// Привязка четность.
        /// </summary>
        public Parity Parity_VM
        {
            get => parity_VM;
            set
            {
                SetOptions(nameof(Parity_VM), ref parity_VM, value);
            }
        }
        /// <summary>
        /// Привязка стоп-бит.
        /// </summary>
        public StopBits StopBits_VM
        {
            get => stopBits_VM;
            set
            {
                SetOptions(nameof(StopBits_VM), ref stopBits_VM, value);
            }
        }
        /// <summary>
        /// Привязка длины слова.
        /// </summary>
        public int WordLength_VM
        {
            get => wordLength_VM;
            set
            {
                SetOptions(nameof(WordLength_VM), ref wordLength_VM, value);
            }
        }
        /// <summary>
        /// Привязка таймаута чтения.
        /// </summary>
        public int TimeoutRead_VM
        {
            get => timeoutRead_VM;
            set
            {
                SetOptions(nameof(TimeoutRead_VM), ref timeoutRead_VM, value);
            }
        }
        /// <summary>
        /// Привязка таймаута записи.
        /// </summary>
        public int TimeoutWrite_VM
        {
            get => timeoutWrite_VM;
            set
            {
                SetOptions(nameof(TimeoutWrite_VM), ref timeoutWrite_VM, value);
            }
        }
        /// <summary>
        /// Доступность кнопки "Добавить и подключить".
        /// </summary>
        public bool CanAddConnect
        {
            get => canAddConnect;
            set
            {
                SetOptions(nameof(CanAddConnect), ref canAddConnect, value);
            }
        }
        /// <summary>
        /// Доступность кнопки "Добавить".
        /// </summary>
        public bool CanAdd
        {
            get => canAdd;
            set
            {
                SetOptions<bool>(nameof(CanAdd), ref canAdd, value);
            }
        }
        /// <summary>
        /// Доступность поля IP адпеса
        /// </summary>
        public bool CanTextIP
        {
            get => canTextIP;
            set
            {
                SetOptions(nameof(canTextIP), ref canTextIP, value);
            }
        }


        private void ConnectViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != null && e.PropertyName.Equals(nameof(CanAddConnect)))
            {
                addConnectCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName != null && e.PropertyName.Equals(nameof(CanAdd)))
            {
                addCommand.RaiseCanExecuteChanged();
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
            if (variable!=null && !variable.Equals(value))
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
