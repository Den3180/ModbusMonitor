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
        private readonly Command aboutCommand;//О программе.
        private readonly Command clearLogErrCommand;//Очистить лог ошибок.
        private readonly Command colorTypeCommand;//Цвета для типов.
        private readonly Command connectionCommand;//Подключение устройства.
        private readonly Command createConnectCommand;//Настройка подключения.
        private readonly Command disconnectionCommand;//Отлючение.
        private readonly Command exitCommand;//Выход.
        private readonly Command formatCommand;//Формат.
        private readonly Command helpCommand;//Справка.
        private readonly Command listenPortCommand;//Слушать порт.
        private readonly Command loadMapCommand;//Загрузить карту.
        private readonly Command openLogCommand;//Открыть лог.
        private readonly Command openLogErrCommand;//Открыть лог ошибок.
        private readonly Command paramCommand;//Параметры.
        private readonly Command ratioCommand;//Коэффициенты.
        private readonly Command resetColCommand;//Сброс ширины столбцов.
        private readonly Command saveMapCommand;//Сохранить карту регистров. 
        private readonly Command sendRequestCommand;//Запрос данных.
        private readonly Command tableCommand;//Показать таблицу.
        private readonly Command textCommand;//Показать как текст.
        private readonly Command viewingPackagesCommand;//Паказать пакеты.
        private readonly Command writeRegisterCommand;//Записать регистр.
        private readonly Command searchAddressCommand;
        private readonly Command disablePollCommand;
        private bool canWriteRegister;
        private bool canConnection;
        private bool canCreateConnect;
        private bool canDisconnection;
        private bool canOpenLog;
        private bool canRequest;
        private bool canDisablePoll;
        private bool treeViewEnabled;
        private bool answerRequest = false;//Флаг завершения опроса.        
        private int numInOrder;//Номера регистров по порядку не зависимо от типа.
        private int regAddress;
        private int deviceAddress;//Адрес устройства в области "Добавления регистров".
        private int countReqTot = 0; //Общее количество запросов.
        private int countReqgood=0;//Количество корректных запросов.
        private string deviceName = "нет данных";
        private string dataFormat = "нет данных";
        private string regName = "нет данных";
        private string regValue = "нет данных";
        private string correctRequest = "0"; //Корректные запросы.
        private string numberRequest = "0";  //Общее количество запросов.
        private object selectedItemTree;
        private UserControlDevices userControl;
        private DeviceClass device;
        private readonly ModbusRTUASCII modbusRTU;
        private readonly DispatcherTimer timerPoll;
        private GroupsTreeNode treeNode;//Дерево устройств.
        public ObservableCollection<GroupsTreeNode> treeNodes;//Источник данных для дерева.
        public static List<DeviceClass> listDevices;
        private List<(string, string)> listMaps;//Хранение загруженных карт.

        public ViewModelMain()
        {
            exitCommand = new Command(ExitApp);
            openLogCommand = new Command(OpenLog, () => CanOpenLog);
            loadMapCommand = new Command(LoadMap);
            saveMapCommand = new Command(SaveMap);
            tableCommand = new Command(MakeTable);
            textCommand = new Command(MakeText);
            viewingPackagesCommand = new Command(ViewingPackages);
            listenPortCommand = new Command(ListenPort, () => CanRequest);
            ratioCommand = new Command(EditRatio);
            formatCommand = new Command(EditFormat);
            colorTypeCommand = new Command(EditColorType);
            resetColCommand = new Command(ResetColumn);
            writeRegisterCommand = new Command(WriteRegister, () => CanWriteRegister);
            sendRequestCommand = new Command(SendRequest, () => CanRequest);
            openLogErrCommand = new Command(OpenLogError);
            clearLogErrCommand = new Command(ClearLogError);
            paramCommand = new Command(ShowParam);
            helpCommand = new Command(ShowHelp);
            aboutCommand = new Command(ShowAbout);
            createConnectCommand = new Command(CreateConnect, () => CanCreateConnect);
            connectionCommand = new Command(Connection, () => canConnection);
            disconnectionCommand = new Command(Disconnection, () => canDisconnection);
            searchAddressCommand = new Command(SearchAddress);
            disablePollCommand = new Command(DisablePoll, () => CanDisablePoll);

            TreeViewEnabled = true;
            modbusRTU = new ModbusRTUASCII();
            listMaps = new List<(string, string)>();
            userControl = new UserControlDevices(new List<CellData>(), modbusRTU);
            device = new DeviceClass();
            listDevices = new List<DeviceClass>();//Список устройств на линии.
            treeNodes = new ObservableCollection<GroupsTreeNode>();//Источник данных дерева.            
            timerPoll = new DispatcherTimer();
            timerPoll.Interval = TimeSpan.FromMilliseconds(1000);
            
            PropertyChanged += ViewModelMain_PropertyChanged;
            timerPoll.Tick += TimerSec_Tick;
            CheckStartParam();
            Task.Run(() => modbusRTU.SendResponsePort(ModbusRTUASCII.SettingPortStart));
        }

        public List<CellData> Cells { get; set; }//Свойство привязки к DataGrid.        
        public IEnumerable<GroupsTreeNode> TreeNodes => treeNodes;//Свойство данных дерева.
        //Комманды.Вкладка "Файл".
        public ICommand LoadMapCommand => loadMapCommand;
        public ICommand SaveMapcomman => saveMapCommand;
        public ICommand OpenLogCommand => openLogCommand;
        public ICommand ExitCommand => exitCommand;
        //Комманды.Вкладка "Вид".
        public ICommand TableCommand => tableCommand;
        public ICommand TextCommand => textCommand;
        public ICommand ViewingPackagesCommand => viewingPackagesCommand;
        public ICommand RatioCommand => ratioCommand;
        public ICommand FormatCommand => formatCommand;
        public ICommand ColorTypeCommand => colorTypeCommand;
        public ICommand ResetColCommand => resetColCommand;
        //Команды.Вкладка "Инструменты".
        public ICommand WriteRegisterCommand => writeRegisterCommand;
        public ICommand SendRequestCommand => sendRequestCommand;
        public ICommand OpenLogErrCommand => openLogErrCommand;
        public ICommand ClearLogErrCommand => clearLogErrCommand;
        public ICommand ListenPortCommand => listenPortCommand;
        public ICommand SearchAddressCommand => searchAddressCommand;
        public ICommand DisablePollCommand => disablePollCommand;
        //Команды.Вкладка "Сервис".
        public ICommand ParamCommand => paramCommand;
        //Команды.Вкладка "Справка".
        public ICommand HelpCommand => helpCommand;
        public ICommand AboutCommand => aboutCommand;
        //Кнопки области "Подключения".
        public ICommand CreateConnectCommand => createConnectCommand;
        public ICommand ConnectionCommand => connectionCommand;
        public ICommand DisconnectionCommand => disconnectionCommand;

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
