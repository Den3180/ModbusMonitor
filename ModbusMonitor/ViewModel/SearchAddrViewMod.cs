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
using System.IO.Ports;
using System.Windows.Threading;
using System.Threading;

namespace ModbusMonitor.ViewModel
{
    public class SearchAddrViewMod:INotifyPropertyChanged
    {
        private string speed=string.Empty;
        private string dataBits = string.Empty;
        private string parity = string.Empty;
        private string stopBits = string.Empty;
        private string currentPort = string.Empty;
        private string timeCount=string.Empty;
        private int address;
        private int addressStart;
        private int addressEnd;
        private int timeOutWrite;
        private int timeOutRead;
        private double progBarValue;
        private TimeOnly timeCurrent;
        private readonly Command searchCommand;
        private readonly Command clearResaultCommand;
        private readonly Command closeCommand;
        private readonly Command breakCommand;
        private bool canSearch;
        private bool canClearResault;
        private bool canBreakCommand;
        private bool flagSearchMethod = false;
        public ICollection<string> sourceNamePort = new ObservableCollection<string>();
        private readonly SearchAddressWindow window;
        private readonly ModbusRTU modbusRTU;
        private SettingPortStart settingPort;
        private readonly DispatcherTimer timer;
        public ICollection<string> resault = new ObservableCollection<string>();
        public ICollection<int> addressList = new ObservableCollection<int>();

        public SearchAddrViewMod(ModbusRTU modbusRTU ,SearchAddressWindow window)
        {
            this.window = window;
            this.modbusRTU = modbusRTU;            
            progBarValue = 0;
            timeCount = new TimeOnly(0, 0, 0).ToLongTimeString();
            timeOutWrite = 1000;
            timeOutRead = 1000;
            addressStart = 0;
            addressEnd = 255;
            address = 0;
            searchCommand = new Command(SearchAddress,()=>CanSearch);
            clearResaultCommand = new Command(ClearResault,()=>CanClearResault);
            closeCommand = new Command(CloseSearch);
            breakCommand = new Command(BreakSearch,()=>CanBreakCommand);
            sourceNamePort = ModbusRTU.GetListPorts();//Список доступных портов.
            //Заполнение диапазона адресов устройств.
            for(int i = 1; i <= addressEnd; i++)
            {
                addressList.Add(i);
            }
            //Отображение порта в комбобоксе портов.
            if (sourceNamePort.Count > 0)
            {
                canSearch = true;
                currentPort = ModbusRTU.GetListPorts()[0];
            }
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000)
            };
            timer.Tick += Timer_Tick;
            PropertyChanged += SearchAddrViewMod_PropertyChanged;
        }
        
        public IEnumerable<string> SourceNamePort => sourceNamePort;
        public IEnumerable<string> Resault => resault;
        public IEnumerable<int> AddressList => addressList;
        public ICommand SearchCommand => searchCommand;
        public ICommand ClearResaultCommand => clearResaultCommand;
        public ICommand CloseCommand => closeCommand;
        public ICommand BreakCommand => breakCommand;

        #region[Обработчики команд]

        /// <summary>
        /// Прервать сканирование.
        /// </summary>
        private void BreakSearch() 
        {           
            if (resault.Count > 0)
            {
                CanClearResault = true;
            }
            modbusRTU.AdressSearch.Clear();
            CanSearch = true;
            flagSearchMethod = false;
            AddressStart = Address;
            CanBreakCommand = false;
            timer.Stop();
        }
        /// <summary>
        /// Закрыть окно поиска адресов.
        /// </summary>
        private void CloseSearch()
        {
            modbusRTU.PortClose();
            window.Close();
        }
        /// <summary>
        /// Очистка результатов, сброс прогрессбара, отключение кнопки очистить.
        /// </summary>
        private void ClearResault()
        {
            if (resault.Count > 0)
            {
                modbusRTU.PortClose();//Закрытие порта.
                resault.Clear();//Очистка списка окна результатов.
                ProgBarValue = 0;//Сброс прогрессбара.
                modbusRTU.AdressSearch.Clear();//Очитска списка адресов.
                Address = 0;
                TimeCount = new TimeOnly(0, 0, 0).ToLongTimeString();
                CanClearResault = false;
                CanSearch = true;
                CanBreakCommand = false;
                flagSearchMethod = false;
            }
        }

        /// <summary>
        /// Обработчик таймера и запуск поиска адресов.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {           
            if (!flagSearchMethod)
            {
                Task.Run(() => modbusRTU.SearchAddress(AddressStart, AddressEnd,
                    settingPort, this));
                flagSearchMethod = true;
            }
            if (modbusRTU.AdressSearch.Count > 0 && !modbusRTU.AdressSearch.Contains(-1))
            {
                foreach (var item in modbusRTU.AdressSearch)
                {                  
                        resault.Add($"Порт: {CurrentPort}\n" +
                                    $"Адрес: {item}\n" +
                                    $"Скорость: {Speed}\n" +
                                    $"DataBits: {DataBits}\n" +
                                    $"Четность: {ParityS}\n" +
                                    $"Стопбит: {StopBits}");                    
                }
                modbusRTU.AdressSearch.Clear();
            }
            else if (resault.Count == 0 && Address == AddressEnd)
            {
                resault.Add("Устройств не обнаружено");
            }
            if (Address == AddressEnd)
            {
                if (resault.Count > 0)
                {
                    CanClearResault = true;
                }
                modbusRTU.AdressSearch.Clear();
                flagSearchMethod = false;
                CanSearch = true;
                CanBreakCommand = false;
                timer.Stop();
            }             
        }

        /// <summary>
        /// Обработчик команды "Поиск".
        /// </summary>
        private void SearchAddress()
        {
            ClearResault();
            ProgBarValue = AddressStart;
            TimeCount = new TimeOnly(0, 0, 0).ToLongTimeString();
            int parity_s = ParityS switch
            {
                "None" => 0,
                "Odd" => 1,
                "Even" =>2,
                _ => 0
            };     
            settingPort = new SettingPortStart
            {
                PortType=CurrentPort,
                BaudRate= Int32.Parse(Speed),
                DataBit= Int32.Parse(DataBits),
                StopBit= Int32.Parse(StopBits),
                ParitySet=(Parity)parity_s,
                TimeOutWrite=TimeOutWrite,
                TimeOutRead=TimeOutRead
            };
            timer.Start();
            CanBreakCommand = true;
            CanSearch = false;
        }       
        #endregion

        #region[Свойства-привязки]      

        /// <summary>
        /// Изменение величины прогрессбара.
        /// </summary>
        public double ProgBarValue
        {
            get => progBarValue;
            set => SetOptions(nameof(ProgBarValue), ref progBarValue, value);
        }

        /// <summary>
        /// Свойство счетчика времени.
        /// </summary>
        public string TimeCount
        {
            get => timeCount;
            set => SetOptions(nameof(TimeCount), ref timeCount, value);
        }

        /// <summary>
        /// Выбранный порт.
        /// </summary>
        public string CurrentPort
        {
            get => currentPort;
            set
            {
                SetOptions(nameof(CurrentPort), ref currentPort, value);                
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
        public string ParityS
        {
            get => parity;
            set => SetOptions(nameof(ParityS), ref parity, value);
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
        /// Включение кнопки "Прервать".
        /// </summary>
        public bool CanBreakCommand
        {
            get => canBreakCommand;
            set => SetOptions(nameof(CanBreakCommand), ref canBreakCommand, value);
        }
        /// <summary>
        /// Флаг доступности команды "Очистить".
        /// </summary>
        public bool CanClearResault
        {
            get => canClearResault;
            set=> SetOptions(nameof(CanClearResault), ref canClearResault, value);
        }
        /// <summary>
        /// Флаг доступности команды "Поиск".
        /// </summary>
        public bool CanSearch
        {
            get => canSearch;
            set => SetOptions(nameof(CanSearch), ref canSearch, value);
        }
        #endregion

        /// <summary>
        /// Изменение доступности команд.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchAddrViewMod_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(CanSearch)))
            {
                searchCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName.Equals(nameof(CanClearResault)))
            {
                clearResaultCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName.Equals(nameof(CanBreakCommand)))
            {
                breakCommand.RaiseCanExecuteChanged();
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
