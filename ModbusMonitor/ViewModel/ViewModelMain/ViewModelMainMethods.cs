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
using System.Runtime.CompilerServices;

namespace ModbusMonitor
{
    public partial class ViewModelMain 
    {
        private readonly object locker = new();// Заглушка локера.
        private bool answerRequest = false;//Флаг завершения опроса.        
        private int countReqTot = 0; //Общее количество запросов.
        private int countReqgood = 0;//Количество корректных запросов.       
        private DeviceClass device;
        private readonly ModbusRTUASCII modbusRTU;
        private GroupsTreeNode treeNode;//Дерево устройств.
        public static List<DeviceClass> listDevices;
        private List<(string, string)> listMaps;//Хранение загруженных карт.
        public List<CellData> Cells { get; set; }//Свойство привязки к DataGrid.
        public UserControl UserTemp { get; set; }

        #region[Обработчики комманд]

        /// <summary>
        /// Удаление всех ячеек карты.
        /// </summary>
        private void ClearAllCells()
        {
            (Usercontrol.DataContext as ControlDeviceView).Cells=null;
            device.CellsArray.Clear();
            ClearLogError();
            (Usercontrol.DataContext as ControlDeviceView).Cells = device.CellsArray;            
        }
        /// <summary>
        /// Добавить ячейки.
        /// </summary>
        private void AddCells()
        {             
            AddСellsWindow addСellsWindow = new AddСellsWindow(device??new DeviceClass());
            addСellsWindow.ShowDialog();
            if (addСellsWindow.Content is not List<CellData> addCells || addCells.Count == 0) return;//Выход, если список пуст.
            device.CellsArray.AddRange(addCells);
            device.CountingRegisters();           
            DeviceClass.NumberSortList(device);
            (Usercontrol.DataContext as ControlDeviceView).Cells = null;           
            (Usercontrol.DataContext as ControlDeviceView).Cells=device.CellsArray;
            CanColorType = true;
        }
        /// <summary>
        /// Изменить устройство.
        /// </summary>
        private void ChangeDevice()
        {
            ChangeDeviceWindow changeDevice = new ChangeDeviceWindow(device);
            changeDevice.ShowDialog(); 
            if(changeDevice.DialogResult == true)//Если были изменения.
            {
                if(SelectedItemTree is GroupsTreeNode node)//Если выбран верхний узел.
                {
                    node.SubGroups[0].ContentAddress = device.DeviceAdress_DC.ToString();
                    node.SubGroups[0].ContentName = device.DeviceName_DC;
                    node.SubGroups[0].ContentPort = device.ConnectFromMap.PortType;
                }
                else if(SelectedItemTree is SubGroupsTree subnode)//Если выбран подузел.
                {
                    subnode.ContentAddress = device.DeviceAdress_DC.ToString();
                    subnode.ContentName = device.DeviceName_DC;
                    subnode.ContentPort = device.ConnectFromMap.PortType;
                }
                foreach (var cell in Cells)//Перебор коллекции регистров.
                {
                    cell.DeviceAdress = device.DeviceAdress_DC.ToString();
                    cell.NameDevice = device.DeviceName_DC;
                }
                device.AdaptersArray.AdapterData.Devices.Device.Adress = device.DeviceAdress_DC.ToString();//Сохранение в поля адаптера.
                device.AdaptersArray.AdapterData.Devices.Device.Name = device.DeviceName_DC;//Сохранение в поля адаптера.
                device.ConnectFromMap.DeviceAdress = device.DeviceAdress_DC;//Сохранение в поле карты.
                ModbusRTUASCII.SettingPortStart.PortType = device.ConnectFromMap.PortType;
                SaveMapTemp();//Сохранение карт во временный файл.
            }
        }
        /// <summary>
        /// Сохранение карт во временный файл.
        /// </summary>
        private void SaveMapTemp()
        {
            ushort[] regTypeNum = new ushort[] {device.NumOfDI,device.NumOfDO,device.NumOfAI,device.NumOfAO };
            FileInfo file = new FileInfo("ModbusMonitor.exe");
            string dir = file.DirectoryName + FileNameMap.MapsTemp;
            if (!Directory.Exists(dir))//Если каталога с картами по этому пути нет,то создаем его.
            {
                Directory.CreateDirectory(dir);
            }
            string filePath = $"{dir}\\{device.DeviceName_DC}.xml";
            device.SaveMapReg(filePath);            
            listMaps.Add((filePath, device.DeviceName_DC));
            device.NumOfDI = regTypeNum[0];
            device.NumOfDO = regTypeNum[1];
            device.NumOfAI = regTypeNum[2];
            device.NumOfAO = regTypeNum[3];
        }
        /// <summary>
        /// Обновить дерево.
        /// </summary>
        private void RefreshTree()
        {
            if (treeNodes.Count > 0)//Если в дереве есть элементы.
            {
                var temp = new ObservableCollection<GroupsTreeNode>(treeNodes);                
                treeNodes.Clear();
                //Занаво отображаем элементы дерева.
                for(int i = 0; i < temp.Count; i++)
                {
                    treeNodes.Add(temp[i]);
                }
                SelectMapsForDevice(treeNodes[0]);//Обновление карты после обновления дерева.
                                                   //Загружается карта для последнего узла дерева.
            }
            else //Если узлов в дереве нет - очищаем.
            {
                treeNodes.Clear();
            }
        }
        /// <summary>
        /// Очистка всего дерева.
        /// </summary>
        private void ClearTreeAll()
        {            
            if (treeNodes.Count > 0)
            {
                treeNodes.Clear();
                Disconnection();
                //device = new DeviceClass();
                //Usercontrol = new UserControlDevices(new List<CellData>(), modbusRTU, device);
                Usercontrol = null;
                listDevices.Clear();//Очистка списка устройств.
            }
            CanClearTreeAll = false;//Отключение команды "Обновить".
            CanClearTreeSingle = false;//Отключение команды "Удалить".
            CanRefreshTree = false;// отключение команды "Удалить все".
            CanChangeDevice = false;//Отключение команды изменить устройство.
            CanAddCells = false;
            CanColorType = false;
            SaveLoadService.CheckAndSaveUnsavedMaps(listMaps);
        }
        /// <summary>
        /// Удаление одного элемента дерева.
        /// </summary>
        private void ClearTreeSingle()
        {
            string nameDev=string.Empty;//Локальная переменная для хранения имени устройства.
            GroupsTreeNode treeNode=null;//Локальная переменная для хранения текущего узла.
            if (SelectedItemTree == null)//Если устройство не выбрано.
            {
                MessageBox.Show("Выберите устройство!");
                return;
            }
            if (SelectedItemTree is GroupsTreeNode node)//Если выбран верхний узел.
            {                
                if (node.State == "Подключено")//Если состояние устройства - Подключено.
                {
                    Disconnection(); //Отключение
                }
                nameDev = node.SubGroups[0].ContentName;
                treeNode = node;
            }
            else if(SelectedItemTree is SubGroupsTree subNode)//Если выбран вторичный узел.
            {
                foreach(var item in treeNodes)//Проходим по списку элементов.
                {
                    if (item.SubGroups[0] == subNode)
                    {
                        if (item.State == "Подключено")
                        {
                            Disconnection();
                        }
                        nameDev = subNode.ContentName;
                        treeNode = item;                       
                        break;
                    }
                }
            }       
            //Если есть имя устройства.
            if (!string.IsNullOrEmpty(nameDev))
            {
                (string, string) mapTemp;//Локальная переменна списка карт.                
                foreach(var map in listMaps)//Проходим по списку карт.
                {
                    if(map.Item2==nameDev && map.Item1.Contains(FileNameMap.MapsTemp)) //Находим в списке карт карту с нужным именем.
                    {
                        SaveLoadService.CheckAndSaveUnsavedMaps(map.Item1);//Сохраняем или удаляем карту.
                        mapTemp = map;
                        listMaps.Remove(mapTemp);//Удаляем карту из списка.
                        break;
                    }                     
                }
            }
            //Поиск и удаление устройства по ID.
            for (int i=0;i<listDevices.Count;i++)
            {
                if (listDevices[i].ID == treeNode.SubGroups.First().DeviceID)
                {
                    listDevices.RemoveAt(i);
                    break;
                }
            }
            treeNodes.Remove(treeNode);//Удаляем элемент из дерева. 
            if (treeNodes.Count == 0) //Если дерево пустое.
            {
                //device = new DeviceClass();
                //Usercontrol = new UserControlDevices(new List<CellData>(), modbusRTU, device);
                Usercontrol = null;
                Disconnection();
                CanClearTreeAll = false;
                CanClearTreeSingle = false;
                CanRefreshTree = false;
                CanChangeDevice = false;
                CanAddCells = false;
                CanColorType = false;
                listDevices.Clear();
                listMaps.Clear();
            }                
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
            CanViewingPackages = false;
            CanDisconnection = false;
            CanDisablePoll = false;
            CanRequest = false;
            CanWriteRegister = false;
            TreeViewEnabled = true;
            CanCreateConnect = true;
            countReqgood = 0;
            countReqTot = 0;
            NumberRequest = "0";
            CorrectRequest = "0";
            device.LinkDevice = EnumLink.LinkNo;
            if (treeNodes.Count>0 && treeNode is not null)
            {
                CanConnection = true;
                treeNode.State = "Отключено";
                treeNode.ColorTextTreeConnect = Brushes.Red;                
            }
            else
            {
                CanDisconnection = false;
                CanConnection = false;
            }
        }
        /// <summary>
        /// Подключение карты.
        /// </summary>
        private void Connection()
        {
            modbusRTU.PortOpen(ModbusRTUASCII.SettingPortStart);  //Подключение порта.
            var itemSelected = DefineNodeInTree(SelectedItemTree);
            foreach(var node in treeNodes)
            {
                if (itemSelected.NameComNode == node.NameCOM)//При совпадении узла и подузла.
                {
                    treeNode = node;//Присваиваем текущее значение.
                }
            }
            if (treeNode is not null && ModbusRTUASCII.Mode == eMode.PortOpen)
            {
                CanViewingPackages = true;
                CanConnection = false;
                CanRequest = true;
                CanWriteRegister = true;
                CanDisconnection = true;
                device.LinkDevice = EnumLink.LinkYes;
                treeNode.State = "Подключено";
                treeNode.ColorTextTreeConnect = Brushes.Green;
                TreeViewEnabled = false;
                CanCreateConnect = false;                
            }
            else
            {
                treeNode.State = "Отключено";
                treeNode.ColorTextTreeConnect = Brushes.Red;
                CanConnection = true;
                CanDisconnection = false;
            }
        }
        /// <summary>
        /// Создание подключения.
        /// </summary>
        private void CreateConnect()
        {           
            if (device != null)
            {
                device = null;
                device = new DeviceClass();
            }
            ConnectSettingWindow connectSetting = new ConnectSettingWindow(device, modbusRTU);//Создание объекта соединения.
            connectSetting.ShowDialog();//Открытие окна создания соединения.
            //Поиск совпадений уже существующих стройств с вновь создаваемыми.
            if (CheckForRepeatabilityOfNodes() == true || 
                (CommandTypeConnection)connectSetting.Content == CommandTypeConnection.None)
            {
                return;
            }           
            ModbusRTUASCII.SettingPortStart.PortType = 
                ModbusRTUASCII.PortsEnabled.FirstOrDefault(port=>port== device.ConnectFromMap.PortType);          
                    FillNodesTree(device);//Заполняем дерево новым устройством.
                    if (!SearchForMatchesNameDevice(listMaps, device.DeviceName_DC))//Если нет совпадения в картах.
                    {
                        //Создаем пустую карту.
                        Usercontrol = new UserControlDevices(new List<CellData>(), modbusRTU,device);
                        CanConnection = false;//Отключаем возможность подключения.
                        listDevices.Add(device);
                        return;
                    }                   
            if ((CommandTypeConnection)connectSetting.Content == CommandTypeConnection.Add)
            {
                CanConnection = true;
            }
            else if ((CommandTypeConnection)connectSetting.Content == CommandTypeConnection.AddConnection)
            {
                Connection();
                if (modbusRTU.MasterRTU != null)
                {
                    CanDisconnection = true;
                }
            }
                listDevices.Add(device);
        }
        /// <summary>
        /// Поиск совпадений имени создаваемого устройства и имени в реестре карт. 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="elem"></param>
        /// <returns></returns>
        private bool SearchForMatchesNameDevice(List<(string,string)>list,string elem)
        {
            foreach (var item in list)
            {
                if (item.Item2 == elem)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Заполнение узлов дерева.
        /// </summary>
        /// <param name="device"></param>
        private void FillNodesTree(DeviceClass device)
        {
            treeNode = new GroupsTreeNode();//Архитектура дерева.Корневой узел.                    
            treeNode.NameCOM = $"{treeNodes.Count + 1}";//Номер устройства.
            treeNode.SubGroups.Add(new SubGroupsTree() 
            { 
                ContentPort= device.ConnectFromMap.PortType,
                ContentAddress= device.DeviceAdress_DC.ToString(),
                ContentName= device.DeviceName_DC,
                NameComNode=treeNode.NameCOM,
                DeviceID=device.ID
            });//Добавление подузлов.
            CanConnection = true;//Включить кнопку "Подключение".
            CanAddCells = true;
            treeNodes.Add(treeNode);//Добавление в коллекцию источника данных дерева.
        }
        /// <summary>
        /// Отображение в виде таблицы.
        /// </summary>
        private void MakeTable()
        {
            Usercontrol = UserTemp;
            if(UserTemp!=null) UserTemp = null;
            viewMode = EnumView.Table;
            CanViewingPackages = true;
            CanMakeTable = false;
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
            if (Usercontrol != null && UserTemp == null) UserTemp = Usercontrol;
            Usercontrol = new UserControlPackages();
            viewMode = EnumView.Packages;
            CanViewingPackages = false;
            CanMakeTable = true;
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
            CanViewingPackages = true;
            timerPoll.Start();
        }
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
            CanViewingPackages = false;
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
            var cellData = ((ControlDeviceView)Usercontrol.DataContext).SelectedCell;
            ValueConverter.ChangeFormatData(cellData); 
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
            WriteRegisterWindow writeRegister = new WriteRegisterWindow(modbusRTU, cellData);
            writeRegister.ShowDialog();
        }
        /// <summary>
        /// отправка запроса.
        /// </summary>
        private void SendRequest()
        {
            lock (locker) 
            { 
                NumberRequest = (++countReqTot).ToString();
                byte adressDev = Convert.ToByte(device.DeviceAdress_DC);
                short startAdressDI = -1;
                short startAdressDO = -1;
                short startAdressAO = -1;
                short startAdressAI = -1;
                ushort numOfDI = 0; 
                ushort numOfDO = 0;
                ushort numOfAO = 0;
                ushort numOfAI = 0;
                string[] tempDI = null;
                string[] tempDO = null;
                ushort[] tempAO = null;
                ushort[] tempAI = null;
                //Ищем первый адрес каждого типа регистров.
                foreach(var item in device.CellsArray)
                {
                    if (item.Type == "DI")
                    {
                        startAdressDI = startAdressDI < 0 ? (short)item.Adress : startAdressDI;
                        numOfDI = item.Adress >= numOfDI ? (ushort)(item.Adress + 1) : numOfDI;
                    }
                    if (item.Type == "DO")
                    {                       
                       startAdressDO = startAdressDO < 0?(short)item.Adress:startAdressDO;                       
                       numOfDO = item.Adress >= numOfDO ? (ushort)(item.Adress+1) : numOfDO;
                    }
                    if (item.Type == "AI")
                    {
                        startAdressAI = startAdressAI < 0 ? (short)item.Adress : startAdressAI;
                        numOfAI = item.Adress >= numOfAI ? (ushort)(item.Adress + 1) : numOfAI;
                    }
                    if (item.Type == "AO")
                    {
                        startAdressAO = startAdressAO < 0 ? (short)item.Adress : startAdressAO;
                        numOfAO = item.Adress >= numOfAO ? (ushort)(item.Adress + 1) : numOfAO;
                    }                      
                }
                    if (numOfDI>0)
                    {                    
                        tempDI = modbusRTU.ReadInputs(adressDev, (ushort)startAdressDI, numOfDI);
                        if (tempDI == null)
                        {
                            answerRequest = false;
                            dispatcher.Invoke(() => logItemSource.Add(modbusRTU.RequestStatusMessage));
                        }
                    }
                    if (numOfDO>0)
                    {                    
                        tempDO = modbusRTU.ReadCoilRegs(adressDev, (ushort)startAdressDO, numOfDO);
                        if (tempDO == null)
                        {
                            answerRequest = false;
                            dispatcher.Invoke(() => logItemSource.Add(modbusRTU.RequestStatusMessage));
                        }
                    }
                    if (numOfAI>0)
                    {
                        tempAI = modbusRTU.ReadInputRegs(adressDev, (ushort)startAdressAI, numOfAI);
                        if (tempAI == null)
                        {
                            answerRequest = false;
                            dispatcher.Invoke(() => logItemSource.Add(modbusRTU.RequestStatusMessage));
                        }
                    }
                    if (numOfAO>0)
                    {
                        tempAO = modbusRTU.ReadHoldingRegs(adressDev, (ushort)startAdressAO, numOfAO);
                         if (tempAO == null)
                         {
                            answerRequest = false;
                            dispatcher.Invoke(() => logItemSource.Add(modbusRTU.RequestStatusMessage));
                         }
                    }
                    if (viewMode == EnumView.Table)//Передача пакетов в таблицу.
                    {
                        FillCells(tempDI, tempDO, tempAO, tempAI);               
                    }
                    else if (viewMode == EnumView.Packages)//Передача пакетов в окно просмотра пакетов.
                    {
                        FillPackage(tempDI, tempDO, tempAO, tempAI);                       
                    }                        
                        if((numOfAO>0 && tempAO!=null) || (numOfDO > 0 && tempDO != null) || (numOfAI > 0 && tempAI != null)
                            || (numOfDI > 0 && tempDI != null))
                        {
                            CorrectRequest = (++countReqgood).ToString();
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
            if (logItemSource.Count > 0)
            {
                logItemSource.Clear();
                CanClearLog = false;
            }
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
            Task.Run(() => modbusRTU.SendResponsePort(ModbusRTUASCII.SettingPortStart));
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
            SaveLoadService dialogService = new SaveLoadService();
            dialogService.SaveFileDialog(device);
        }
        /// <summary>
        /// Загрузить карту регистров.
        /// </summary>
        private void LoadMap()
        {             
            SaveLoadService dialogService = new SaveLoadService();
            device = dialogService.OpenFileDialog();           
            if(device==null)
            {
                device = new DeviceClass();
                return;
            }
            if (!CheckListDevice())
            {
                listDevices.Add(device);//После загрузки карты заносим устройство
                listMaps.Add((dialogService.FilePath, device.DeviceName_DC));//в список устройств на этой линии.
            }            
            Cells = device.CellsArray; //Коллекция, которая заполняет DataGrid.           
            //Привязано к свойству Content основного окна.
            Usercontrol = new UserControlDevices(Cells, modbusRTU, device);
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
                ModbusRTUASCII.SettingPortStart.PortType =
                    ModbusRTUASCII.PortsEnabled.FirstOrDefault(port=>port==device.ConnectFromMap.PortType);
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
            CanRefreshTree = true;
            CanColorType = true;
            CanClearAllCells = true;
            SetSelectedDevice();//Синхронизация выбранного элемента в дереве.
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
            modbusRTU.PortClose();            
            SaveLoadService.CheckAndSaveUnsavedMaps(listMaps);
            App.Current.MainWindow.Close();
        }      
        /// <summary>
        /// Выбор карты для выбранного устройства дерева.
        /// </summary>
        /// <param name="item"></param>
        private void SelectMapsForDevice(object item)
        {
            bool selectNewMap=false; //Флаг выбора новой карты.
            SubGroupsTree itemSelected;  //Подузел для выбранного элемента          
            CanConnection = true;
            //Определяем какой вид узла дерева выбран.
            itemSelected = DefineNodeInTree(item);            
            //Если нужная карта уже загружена, то ничего не меняем.
            if (itemSelected?.ContentName == (Usercontrol.DataContext as ControlDeviceView)?.CurrentDevice.DeviceName_DC)
            {
                Cells = device.CellsArray;               
                return;
            }           
            foreach(var dev in listDevices)
            {
                if(itemSelected?.ContentName==dev.DeviceName_DC && itemSelected?.ContentAddress == dev.DeviceAdress_DC.ToString())
                {
                    device = dev;
                    selectNewMap = true;
                }
            }
            if (selectNewMap)//Если выбранв новая карта.
            {        
                Cells = device.CellsArray; //Коллекция, которая заполняет DataGrid.
                Usercontrol = new UserControlDevices(Cells, modbusRTU, device);                
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
                    //Ставим первый доступный порт, если никакого порта нет.
                    ModbusRTUASCII.SettingPortStart.PortType = string.IsNullOrEmpty(ModbusRTUASCII.SettingPortStart.PortType)?
                        ModbusRTUASCII.PortsEnabled.FirstOrDefault(): ModbusRTUASCII.SettingPortStart.PortType;                    
                }
                else
                {
                    ModbusRTUASCII.SettingPortStart.PortType = "не обнаружено";
                }
                ModbusRTUASCII.SettingPortStart.BaudRate = device.ConnectFromMap.SpeedPort;
                ModbusRTUASCII.SettingPortStart.DataBit = device.ConnectFromMap.LenghtWord;
                ModbusRTUASCII.SettingPortStart.ParitySet = (Parity)device.ConnectFromMap.Parity;
                ModbusRTUASCII.SettingPortStart.StopBit = device.ConnectFromMap.Stop_Bit;               
                if (ModbusRTUASCII.PortsEnabled.Count == 0)
                {
                    Task.Run(() => modbusRTU.SendResponsePort(ModbusRTUASCII.SettingPortStart, device.DeviceAdress_DC));
                }
            }
            else
            {
                Usercontrol = new UserControlDevices(new List<CellData>(), modbusRTU, device);
                CanConnection = false;//При пустой карте не подключить.
            }
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
            if (logItemSource.Count > 0)//Вклчаем кнопку Очистка лога ошибок.
            {
                CanClearLog = true;
            }
            if (answerRequest == false)
            {
                answerRequest = true;
                await Task.Run(() => SendRequest());
            }
        }
        #endregion
    }
}