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
    public class ViewModelMain : INotifyPropertyChanged
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
        private UserControlDevices userControl;
        private DeviceClass device;
        private readonly ModbusRTUASCII modbusRTU;
        private readonly DispatcherTimer timerPoll;
        private GroupsTreeNode treeNode;//Дерево устройств.
        public ObservableCollection<GroupsTreeNode> treeNodes;//Источник данных для дерева.
        public static List<DeviceClass> listDevices;

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

        #region[Обработчики комманд]
        /// <summary>
        /// Обработчик команнды "Отключить опрос".
        /// </summary>
        private void DisablePoll()
        {
            if (timerPoll.IsEnabled)
            {
                timerPoll.Stop();
            }
            CanRequest = true;
            CanDisablePoll = false;            
        }

        /// <summary>
        /// Поиск адреса устройства.
        /// </summary>
        private void SearchAddress()
        {
            SearchAddressWindow searchAddress = new SearchAddressWindow(modbusRTU);
            searchAddress.ShowDialog();            
        }

        /// <summary>
        /// Отключение карты.
        /// </summary>
        private void Disconnection()
        {
            if (timerPoll.IsEnabled)
            {
                timerPoll.Stop();
            }
            modbusRTU.PortClose();
            CanDisconnection = false;
            CanDisablePoll = false;
            CanRequest = false;
            CanConnection = true;
            CanWriteRegister = false;
            TreeViewEnabled = true;
            countReqgood = 0;
            countReqTot = 0;
            NumberRequest = "0";
            CorrectRequest = "0";
            device.LinkDevice = EnumLink.LinkNo;
            if(treeNode is not null)
            treeNode.SubGroups[3].ContentClass = "Отключено";
        }

        /// <summary>
        /// Подключение карты.
        /// </summary>
        private void Connection()
        {           
            modbusRTU.PortOpen(ModbusRTUASCII.SettingPortStart);  //Подключение порта.            
            if (treeNode is not null && ModbusRTUASCII.Mode==eMode.PortOpen)
            {
                CanConnection = false;
                CanRequest = true;
                CanWriteRegister = true;
                CanDisconnection = true;
               // DeviceClass.Link = EnumLink.LinkYes;        //Статус подключения.
                device.LinkDevice=EnumLink.LinkYes;                
                treeNode.SubGroups[3].ContentClass = "Подключено";
                TreeViewEnabled = false;
            }
            else
            {
                treeNode.SubGroups[3].ContentClass = "Отключено";
                CanConnection = true;
                CanDisconnection = false;
            }
        }

        /// <summary>
        /// Создание подключения.
        /// </summary>
        private void CreateConnect()
        {
            ConnectSettingWindow connectSetting = new ConnectSettingWindow(device, modbusRTU);//Создание объекта соединения.
            connectSetting.ShowDialog();//Открытие окна создания соединения.
            //Если не нажата кнопка отмены.
            if ((CommandTypeConnection)connectSetting.Content != CommandTypeConnection.None)
            {                
                //treeNodes?.Clear();//Очистка дерева перед новым заполнением. Нужно ли?               
                    foreach (var ports in ModbusRTUASCII.PortsEnabled)
                    {
                        ModbusRTUASCII.SettingPortStart.PortType = ports;
                        FillNodesTree(device);                   
                    }               
            }
            if ((CommandTypeConnection)connectSetting.Content == CommandTypeConnection.Add)
            {
                CanConnection = true;
            }
            if ((CommandTypeConnection)connectSetting.Content == CommandTypeConnection.AddConnection)
            {
                Connection();
                if (modbusRTU.MasterRTU != null)
                {
                    CanDisconnection = true;                    
                }
            }
        }
        /// <summary>
        /// Заполнение узлов дерева.
        /// </summary>
        /// <param name="device"></param>
        private void FillNodesTree(DeviceClass device)
        {
            treeNode = new GroupsTreeNode();//Архитектура дерева.                    
            treeNode.NameCOM = $"{treeNodes.Count + 1}";
            treeNode.SubGroups.Add(new SubGroupsTree("Порт: " ,ModbusRTUASCII.SettingPortStart.PortType));
            treeNode.SubGroups.Add(new SubGroupsTree("Адрес устройства: ",device.DeviceAdress_DC.ToString()));
            treeNode.SubGroups.Add(new SubGroupsTree("Имя устройства: ",device.DeviceName_DC));
            treeNode.SubGroups.Add(new SubGroupsTree());
            treeNodes.Add(treeNode);//Добавление в коллекцию источника данных дерева.
            CanConnection = true;
        }

        /// <summary>
        /// Отображение в виде таблицы.
        /// </summary>
        private void MakeTable()
        {

        }

        /// <summary>
        /// Отображение в виде текста.
        /// </summary>
        private void MakeText()
        {
        }

        /// <summary>
        /// Просмотр пакетов.
        /// </summary>
        private void ViewingPackages()
        {

        }

        /// <summary>
        /// Слушать порт
        /// </summary>
        private void ListenPort()
        {
            answerRequest = false;
            CanRequest = false;
            CanDisablePoll = true;
            TreeViewEnabled = false;
            timerPoll.Start();
        }

        /// <summary>
        /// Коэффициенты.
        /// </summary>
        private void EditRatio()
        {
        }

        /// <summary>
        /// Формат
        /// </summary>
        private void EditFormat()
        {
        }

        /// <summary>
        /// Цвета для типов.
        /// </summary>
        private void EditColorType()
        {

        }

        /// <summary>
        /// Сброс ширины столбцов.
        /// </summary>
        private void ResetColumn()
        {

        }

        /// <summary>
        /// Записать регистр.
        /// </summary>
        private void WriteRegister()
        {
            var cellData = ((ControlDeviceView)Usercontrol.DataContext).SelectedCell;
            if (string.IsNullOrEmpty(cellData.NameDevice))
            {
                MessageBox.Show("Не выбран регистр!");
                return;
            }
            WriteRegisterWindow writeRegister = new WriteRegisterWindow(modbusRTU,cellData);
            writeRegister.ShowDialog();
        }

        /// <summary>
        /// отправка запроса.
        /// </summary>
        private void SendRequest()
        {           
            NumberRequest = (++countReqTot).ToString();
            byte adressDev = Convert.ToByte(device.DeviceAdress_DC);
            short startAdressDI = -1;
            short startAdressDO = -1;
            short startAdressAO = -1;
            short startAdressAI = -1;
            ushort numOfDI = device.NumOfDI;
            ushort numOfDO = device.NumOfDO;
            ushort numOfAO = device.NumOfAO;
            ushort numOfAI = device.NumOfAI;
            string[] tempDI = null;
            string[] tempDO = null; 
            ushort[] tempAO = null;
            ushort[] tempAI = null;
            foreach (var item in device.CellsArray)
            {
                if (item.Type == "DI" && startAdressDI == -1)
                {
                    startAdressDI = (short)item.Adress;
                    tempDI = modbusRTU.ReadInputs(adressDev, (ushort)startAdressDI, numOfDI);
                    if (tempDI == null)
                    {
                        answerRequest = false;
                        return;
                    }                    
                    Array.Reverse(tempDI);
                }
                else if (item.Type == "DO" && startAdressDO == -1)
                {
                    startAdressDO = (short)item.Adress;
                    tempDO = modbusRTU.ReadCoilRegs(adressDev, (ushort)startAdressDO, numOfDO);
                    if (tempDO == null)
                    {
                        answerRequest = false;
                        return;
                    }
                    Array.Reverse(tempDO);
                }
                else if(item.Type == "AI" && startAdressAI == -1)
                {
                    startAdressAI = (short)item.Adress;
                    tempAI = modbusRTU.ReadInputRegs(adressDev, (ushort)startAdressAI, numOfAI);
                    if (tempAI == null)
                    {
                        answerRequest = false;
                        return;
                    }
                    Array.Reverse(tempAI);
                }
                else if (item.Type == "AO" && startAdressAO == -1)
                {
                    startAdressAO = (short)item.Adress;
                    tempAO = modbusRTU.ReadHoldingRegs(adressDev, (ushort)startAdressAO, numOfAO);
                    if (tempAO == null)
                    {
                        answerRequest = false;
                        return;
                    }
                    Array.Reverse(tempAO);
                }
                if (item == Cells?[^1])//Считывание закончено.
                {                    
                    FillCells(tempDI, tempDO, tempAO,tempAI, numOfDI, numOfDO, numOfAO, numOfAI);                  
                    startAdressDI = -1;
                    startAdressDO = -1;
                    startAdressAO = -1;
                    answerRequest = false;
                    CorrectRequest = (++countReqgood).ToString();
                    return;
                }
            }
        }

        /// <summary>
        /// Заполнить таблицу.
        /// </summary>
        /// <param name="tempDI"></param>
        /// <param name="tempDO"></param>
        /// <param name="tempAO"></param>
        /// <param name="numOfDI"></param>
        /// <param name="numOfDO"></param>
        /// <param name="numOfAO"></param>
        private void FillCells(string[] tempDI, string[] tempDO, ushort[] tempAO, ushort[] tempAI, 
            ushort numOfDI,ushort numOfDO, ushort numOfAO, ushort numOfAI)
        {
            //Заполнение ячеек привязанных к DataGrid. 
            foreach (var item in device.CellsArray)
            {
                if (item.Type == "DI")
                {
                    item.Value = tempDI[--numOfDI];
                }
                else if (item.Type == "DO")
                {
                    item.Value = tempDO[--numOfDO];
                }
                else if (item.Type == "AO")
                {
                    if (item.Format == "Bin")//Перевод в бинарный формат.
                    {
                        item.Value = Convert.ToString(tempAO[--numOfAO], 2);
                        continue;
                    }
                    item.Value = tempAO[--numOfAO].ToString();
                    //Если отрицательное значение.
                    if (ushort.Parse(item.Value) > 32767)
                    {
                        item.Value = (Convert.ToInt32(item.Value) - 65535 - 1).ToString();
                    }
                }
                else if (item.Type == "AI")
                {
                    if (item.Format == "Bin")//Перевод в бинарный формат.
                    {
                        item.Value = Convert.ToString(tempAI[--numOfAI], 2);
                        continue;
                    }
                    item.Value = tempAI[--numOfAI].ToString();
                }
            }
        }

        /// <summary>
        /// Открыть лог ошибок.
        /// </summary>
        private void OpenLogError()
        {

        }

        /// <summary>
        /// Очистить лог ошибок.
        /// </summary>
        private void ClearLogError()
        {

        }

        /// <summary>
        /// Параметры.
        /// </summary>
        private void ShowParam()
        {

        }

        /// <summary>
        /// Показать помощ.
        /// </summary>
        private void ShowHelp()
        {

        }

        /// <summary>
        ///О программе.
        /// </summary>
        private void ShowAbout()
        {

        }

        /// <summary>
        /// Активация комманд.
        /// </summary>
        private void CheckStartParam()
        {
            if (File.Exists("logfile.dat"))
            {
                CanOpenLog = true;
            }
            if (!Directory.Exists("./Maps"))
            {
                Directory.CreateDirectory("./Maps");
            }
        }

        /// <summary>
        /// Сохранить карту регистров.
        /// </summary>
        private void SaveMap()
        {
            DefaultDialogService dialogService = new DefaultDialogService();
            dialogService.SaveFileDialog(device);
        }

        /// <summary>
        /// Загрузить карту регистров.
        /// </summary>
        private void LoadMap()
        {
            DefaultDialogService dialogService = new DefaultDialogService();
            device = dialogService.OpenFileDialog();
            if (device == null)
            {
                return;
            }
            listDevices.Add(device);//После загрузки карты заносим устройство
                                    //в список устройств на этой линии.
            Cells = device.CellsArray; //Коллекция, которая заполняет DataGrid.           
            //Привязано к свойству Content основного окна.
            Usercontrol = new UserControlDevices(Cells, modbusRTU,device);            
            //Если устройство подключено.
            if (device.LinkDevice == EnumLink.LinkYes && DeviceAddress == device.DeviceAdress_DC)
            {
                Disconnection();  //Отключение подключения.          
            }
            DeviceName = device.DeviceName_DC;       //В группбокс "Добавление регистров".
            DeviceAddress = device.DeviceAdress_DC;  //В группбокс "Добавление регистров".
            CanCreateConnect = true; //Кнопку "Создать" включить.
            //Установка параметров порта из данных карты устройства.
            if (ModbusRTUASCII.PortsEnabled.Count > 0)
            {
                ModbusRTUASCII.SettingPortStart.PortType = ModbusRTUASCII.PortsEnabled.FirstOrDefault();
            }
            else
            {
                ModbusRTUASCII.SettingPortStart.PortType = "не обнаружено";
            }
            ModbusRTUASCII.SettingPortStart.BaudRate = device.ConnectFromMap.SpeedPort;
            ModbusRTUASCII.SettingPortStart.DataBit = device.ConnectFromMap.LenghtWord;
            ModbusRTUASCII.SettingPortStart.ParitySet = (Parity)device.ConnectFromMap.Parity;
            ModbusRTUASCII.SettingPortStart.StopBit = device.ConnectFromMap.Stop_Bit;
            FillNodesTree(device);//Заполнение дерева без подключения.           
            if (ModbusRTUASCII.PortsEnabled.Count == 0)
            {
                Task.Run(() => modbusRTU.SendResponsePort(ModbusRTUASCII.SettingPortStart, device.DeviceAdress_DC));            
            }
        }

        /// <summary>
        /// Открыть файл логов.
        /// </summary>
        private void OpenLog()
        {

        }

        /// <summary>
        /// Закрыть приложение.
        /// </summary>
        private void ExitApp()
        {
            if (timerPoll.IsEnabled)
            {
                timerPoll.Stop();
            }
            App.Current.MainWindow.Close();            
        }       

        /// <summary>
        /// Обработчик таймера.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TimerSec_Tick(object sender, EventArgs e)
        {            
            if (ModbusRTUASCII.Mode == eMode.None)
            {
                CanConnection = false;
                CanDisconnection = false;
                CanCreateConnect = false;
                timerPoll.Stop();
                modbusRTU.PortClose();
                CanRequest = true;
                MessageBox.Show("Потеря соединения!");
                return;
            }
            if (answerRequest == false)
            {
                answerRequest = true;
                await Task.Run(() => SendRequest());
            }
        }
        #endregion

        #region [Флаги доступности]

        /// <summary>
        /// Доступность записи в регистр.
        /// </summary>
        public bool CanWriteRegister
        {
            get => canWriteRegister;
            set
            {                
                SetOptions(nameof(CanWriteRegister), ref canWriteRegister, value);                
            }
        }
        /// <summary>
        /// Доступность опроса порта.
        /// </summary>
        public bool CanDisablePoll
        {
            get => canDisablePoll;
            set => SetOptions(nameof(CanDisablePoll), ref canDisablePoll, value);
        }
        /// <summary>
        /// Доступность лога. 
        /// </summary>
        public bool CanOpenLog
        {
            get => canOpenLog;
            set => SetOptions(nameof(CanOpenLog), ref canOpenLog, value);
        }
        /// <summary>
        /// Доступность кнопки "Создать". 
        /// </summary>
        public bool CanCreateConnect
        {
            get => canCreateConnect;
            set => SetOptions(nameof(CanCreateConnect), ref canCreateConnect, value);
        }
        /// <summary>
        /// Доступность кнопки "Подключение". 
        /// </summary>
        public bool CanConnection
        {
            get => canConnection;
            set =>SetOptions(nameof(CanConnection), ref canConnection, value);
        }
        /// <summary>
        /// Доступность кнопки "Отключение". 
        /// </summary>
        public bool CanDisconnection
        {
            get => canConnection;
            set => SetOptions(nameof(CanDisconnection), ref canDisconnection, value);
        }
        /// <summary>
        /// Доступность "Запрос данных","Слушать порт".
        /// </summary>
        public bool CanRequest
        {
            get => canRequest;
            set => SetOptions(nameof(CanRequest), ref canRequest, value);
        }
        #endregion

        #region[Свойства-привязки]

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
            set => SetOptions(nameof(CorrectRequest),ref correctRequest,value);
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


        private object obj;
        public object Obj
        {
            get => obj;
            set {
                var val = value;
                  SetOptions(nameof(Obj), ref obj, value);
            
            }
        }
    }
}
