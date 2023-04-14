using ModbusMonitor.Classes;
using ModbusMonitor.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ModbusMonitor.ViewModel;
using System.Windows.Controls;

namespace ModbusMonitor
{
    public partial class ViewModelMain : ChangePropertyClass
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
        private readonly Command clearTreeSingleCommand;//Удалить один элемент из дерева.
        private readonly Command clearTreeAllCommand;//Удалить все дерево.
        private readonly Command refreshTreeCommand; //Обновить все дерево.
        private readonly Command changeDeviceCommand;//Изменить устройство в дереве.
        private readonly Command addCellsCommand;//Добавить ячейки.        
        private readonly DispatcherTimer timerPoll;
        
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
            colorTypeCommand = new Command(EditColorType,()=>CanColorType);
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
            clearTreeSingleCommand = new Command(ClearTreeSingle,()=>CanClearTreeSingle);
            clearTreeAllCommand = new Command(ClearTreeAll,()=>CanClearTreeAll);
            refreshTreeCommand = new Command(RefreshTree,()=>CanRefreshTree);
            changeDeviceCommand = new Command(ChangeDevice, () => CanChangeDevice);
            addCellsCommand = new Command(AddCells,()=>CanAddCells);
            
            TreeViewEnabled = true;
            modbusRTU = new ModbusRTUASCII();
            listMaps = new List<(string, string)>();//Список кортежей(путь к карте, имя устройства).
            userControl = new UserControlDevices(new List<CellData>(), modbusRTU);            
            device = new DeviceClass();
            listDevices = new List<DeviceClass>();//Список устройств на линии.
            treeNodes = new ObservableCollection<GroupsTreeNode>();//Источник данных дерева.
            logItemSource = new ObservableCollection<string>();//Источник лога листбокса.                                                       
            timerPoll = new DispatcherTimer();
            timerPoll.Interval = TimeSpan.FromMilliseconds(1000);
            
            PropertyChanged += ViewModelMain_PropertyChanged;
            timerPoll.Tick += TimerSec_Tick;
            CheckStartParam();
            Task.Run(() => modbusRTU.SendResponsePort(ModbusRTUASCII.SettingPortStart));            
        }
      
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

        public ICommand ClearTreeSingleCommand => clearTreeSingleCommand;
        public ICommand ClearTreeAllCommand => clearTreeAllCommand;
        public ICommand RefreshTreeCommand => refreshTreeCommand;
        public ICommand ChangeDeviceCommand => changeDeviceCommand;
        public ICommand AddCellsCommand => addCellsCommand;
    }
}
