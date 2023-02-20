using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModbusMonitor.Windows;
using ModbusMonitor.Classes;
using System.Windows.Input;

namespace ModbusMonitor.ViewModel
{
    public class SearchAddrViewMod:INotifyPropertyChanged
    {
        private string speed = string.Empty;
        private string dataBits = string.Empty;
        private string parity = string.Empty;
        private string stopBits = string.Empty;
        private string currentPort = string.Empty;
        private int address;
        private int addressStart;
        private int addressEnd;
        private int timeOutWrite;
        private int timeOutRead;
        private readonly Command searchCommand;
        private readonly Command radioSpeedCommand;
        private bool canSearch;
        public ICollection<string> sourceNamePort = new ObservableCollection<string>();
        private readonly SearchAddressWindow window;
        private readonly ModbusRTU modbusRTU;

        public SearchAddrViewMod(ModbusRTU modbusRTU ,SearchAddressWindow window)
        {
            this.window = window;
            this.modbusRTU = modbusRTU;
            timeOutWrite = 1000;
            timeOutRead = 1000;
            addressStart = 0;
            addressEnd = 247;
            address = 0;
            searchCommand = new Command(SearchAddress,()=>CanSearch);
            radioSpeedCommand = new Command(RadioSpeed);
            sourceNamePort = ModbusRTU.GetListPorts();
            if (sourceNamePort.Count > 0)
            {
                canSearch = true;
                currentPort = ModbusRTU.GetListPorts()[0];
            }            
            PropertyChanged += SearchAddrViewMod_PropertyChanged;
        }

        public IEnumerable<string> SourceNamePort => sourceNamePort;
        public ICommand SearchCommand => searchCommand;
        public ICommand RadioSpeedCommand => radioSpeedCommand;

        #region[Обработчики команд]
        /// <summary>
        /// Обработчик команды "Поиск".
        /// </summary>
        private void SearchAddress()
        {
            modbusRTU.SearchAddress(AddressStart,AddressEnd);
        }
        
        private void RadioSpeed()
        {

        }
        #endregion

        #region[Свойства-привязки]

        public string CurrentPort
        {
            get => currentPort;
            set
            {
                SetOptions(nameof(CurrentPort), ref currentPort, value);
                TimeOutWrite = Convert.ToInt32(CurrentPort);
            }
        }
        /// <summary>
        /// Скорость передачи данных.
        /// </summary>
        public string Speed
        {
            get => speed;
            set => SetOptions(nameof(Speed), ref speed, value);
        }
        /// <summary>
        /// Размер передачи данных.
        /// </summary>
        public string DataBits
        {
            get => dataBits;
            set => SetOptions(nameof(DataBits), ref dataBits, value);
        }
        /// <summary>
        /// Четность.
        /// </summary>
        public string Parity
        {
            get => parity;
            set => SetOptions(nameof(Parity), ref parity, value);
        }
        /// <summary>
        /// Стопбит.
        /// </summary>
        public string StopBits
        {
            get => stopBits;
            set => SetOptions(nameof(StopBits), ref stopBits, value);
        }
        /// <summary>
        /// Адрес.
        /// </summary>
        public int Address
        {
            get => address;
            set => SetOptions(nameof(Address), ref address, value);
        }
        /// <summary>
        /// Начальный адрес сканирования.
        /// </summary>
        public int AddressStart
        {
            get => addressStart;
            set 
            {
                if (value < 0 || value > 247) return;
                SetOptions(nameof(AddressStart), ref addressStart, value);
                Address = value;
            }
        }
        /// <summary>
        /// Конечный адрес сканирования.
        /// </summary>
        public int AddressEnd
        {
            get => addressEnd;
            set
            {
                if (value < 0 || value > 247) return;
                SetOptions(nameof(AddressEnd), ref addressEnd, value);

            }
        }
        /// <summary>
        /// Таймаут записи.
        /// </summary>
        public int TimeOutWrite
        {
            get => timeOutWrite;
            set => SetOptions(nameof(TimeOutWrite), ref timeOutWrite, value);
        }
        /// <summary>
        /// Таймаут чтения.
        /// </summary>
        public int TimeOutRead
        {
            get => timeOutRead;
            set => SetOptions(nameof(TimeOutRead), ref timeOutRead, value);
        }
        #endregion

        #region[Флаги доступности]
        /// <summary>
        /// Флаг доступности команды "Поиск".
        /// </summary>
        public bool CanSearch
        {
            get => canSearch;
            set => SetOptions(nameof(CanSearch), ref canSearch, value);
        }
        #endregion

        private void SearchAddrViewMod_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(CanSearch)))
            {
                searchCommand.RaiseCanExecuteChanged();
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
