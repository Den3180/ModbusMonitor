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
            if (treeNode is not null)
                treeNode.State = "Отключено";
        }

        /// <summary>
        /// Подключение карты.
        /// </summary>
        private void Connection()
        {
            modbusRTU.PortOpen(ModbusRTUASCII.SettingPortStart);  //Подключение порта.            
            if (treeNode is not null && ModbusRTUASCII.Mode == eMode.PortOpen)
            {
                CanConnection = false;
                CanRequest = true;
                CanWriteRegister = true;
                CanDisconnection = true;
                // DeviceClass.Link = EnumLink.LinkYes;        //Статус подключения.
                device.LinkDevice = EnumLink.LinkYes;
                treeNode.State = "Подключено";
                TreeViewEnabled = false;
            }
            else
            {
                treeNode.State = "Отключено";
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
            treeNode.SubGroups.Add(new SubGroupsTree(ModbusRTUASCII.SettingPortStart.PortType,
                device.DeviceAdress_DC.ToString(), device.DeviceName_DC));
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
            WriteRegisterWindow writeRegister = new WriteRegisterWindow(modbusRTU, cellData);
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
                else if (item.Type == "AI" && startAdressAI == -1)
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
                    FillCells(tempDI, tempDO, tempAO, tempAI, numOfDI, numOfDO, numOfAO, numOfAI);
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
            ushort numOfDI, ushort numOfDO, ushort numOfAO, ushort numOfAI)
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
            listMaps.Add((dialogService.FilePath, device.DeviceName_DC));
            listDevices.Add(device);//После загрузки карты заносим устройство
                                    //в список устройств на этой линии.
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
        /// Выбор карты для выбранного устройства дерева.
        /// </summary>
        /// <param name="item"></param>
        private void SelectMapsForDevice(object item)
        {
            SubGroupsTree itemSelected;
            string filePath = string.Empty;
            if (item is GroupsTreeNode)
            {
                itemSelected = (item as GroupsTreeNode).SubGroups.First();
            }
            else
            {
                itemSelected = item as SubGroupsTree;
            }
            foreach (var listItem in listMaps)//Ищем по имени нужный адрес карты.
            {
                if (listItem.Item2 == itemSelected.ContentName)
                {
                    filePath = listItem.Item1;
                }
            }
            if (!string.IsNullOrEmpty(filePath))
            {
                device = DeviceClass.LoadMapReg(filePath);
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



    }
}