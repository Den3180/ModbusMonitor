using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Modbus.Device;
using Modbus.Utility;
using Modbus.Data;
using ModbusMonitor.Controls;
using System.Windows;
using System.Net;
using System.Net.NetworkInformation;
using System.Data;
using System.Threading;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Media;
using ModbusMonitor.ViewModel;
using Modbus.Message;

namespace ModbusMonitor.Classes
{
    public class ModbusRTU
    {
        public delegate void PortErrorEventHandler(Exception ex);//Делегат метода ошибки.
        public event PortErrorEventHandler PortErrorEvent; //Событие ошибки.
        private SerialPort serialPort;//Создание порта.       
        private ModbusSerialMaster masterRTU;
       
        //TODO: Счетчики запроса сделать для класса Modbus.
        public ModbusRTU()
        {
            serialPort = new SerialPort();
            PortErrorEvent += MessageError;
            PortsEnabled = new List<string>();
            AdressSearch = new List<int>();
            SettingPortStart = new SettingPortStart();
        }

        public string TextMessage { get; set; } = string.Empty;
        public List<int> AdressSearch { get; set; } //Найденый адрес устройства.
        public static eMode Mode { get; set; }
        public static List<string> PortsEnabled { get; set; } //Список портов доступных
                                                              //для передачи данных.
        public static SettingPortStart SettingPortStart { get; set; }//Первичные настройки порта.
        public ModbusSerialMaster MasterRTU => masterRTU;

        /// <summary>
        /// Открытие порта Modbus.
        /// </summary>
        /// <param name="port">Порт</param>
        /// <param name="baudRate">Скорость</param>
        /// <param name="dataBit"></param>
        /// <param name="parity"></param>
        /// <param name="stopBit">Стоп-бит</param>
        public void PortOpen(SettingPortStart portStart)
        {
            serialPort ??= new SerialPort();//Создание пустого объекта порта, если его нет.
            try
            {
                if (serialPort.IsOpen && Mode == eMode.PortOpen)
                {
                    serialPort.Close();
                    Mode = eMode.None;

                }
                serialPort.PortName = portStart.PortType;
                serialPort.BaudRate = portStart.BaudRate;
                serialPort.DataBits = portStart.DataBit;
                serialPort.Parity = portStart.ParitySet;
                serialPort.StopBits = (StopBits)portStart.StopBit;
                serialPort.ReadTimeout = portStart.TimeOutRead;
                serialPort.WriteTimeout = portStart.TimeOutWrite;
                if (!serialPort.IsOpen && Mode == eMode.None)
                {
                    serialPort.Open();
                    Mode = eMode.PortOpen;
                    masterRTU = ModbusSerialMaster.CreateRtu(serialPort);
                }
            }
            catch (Exception ex)
            {
                PortErrorEvent?.Invoke(ex);
                return;
            }
        }

        /// <summary>
        /// Закрытие порта.
        /// </summary>
        public void PortClose()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort?.Close();
                serialPort = null;
                Mode = eMode.None;
            }
        }

        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        /// <param name="ex"></param>
        public void MessageError(Exception ex)
        {
            MessageBox.Show("Ошибка порта!\n" + ex.Message);
        }

        /// <summary>
        /// Чтение группы регистров типа АО.
        /// </summary>
        /// <param name="adresDevice"></param>
        /// <param name="startAdress"></param>
        /// <param name="numOfPoint"></param>
        public ushort[] ReadHoldingRegs(byte adresDevice, ushort startAdress, ushort numOfPoint)
        {
            try
            {
                ushort[] tempData = MasterRTU.ReadHoldingRegisters(adresDevice, startAdress, numOfPoint);                
                return tempData;
            }
            catch (Exception ex)
            {
                //Mode = eMode.None;
            }
            return null;
        }

        /// <summary>
        /// Чтение группы регистров типа АI.
        /// </summary>
        /// <param name="adresDevice"></param>
        /// <param name="startAdress"></param>
        /// <param name="numOfPoint"></param>
        public ushort[] ReadInputRegs(byte adresDevice, ushort startAdress, ushort numOfPoint)
        {
            try
            {
                ushort[] tempData = MasterRTU.ReadInputRegisters(adresDevice, startAdress, numOfPoint);
                return tempData;
            }
            catch (Exception ex)
            {
                //Mode = eMode.None;
            }
            return null;
        }

        /// <summary>
        /// Чтение группы регистров типа DO.
        /// </summary>
        /// <param name="adresDevice"></param>
        /// <param name="startAdress"></param>
        /// <param name="numOfPoint"></param>
        /// <returns></returns>
        public string[] ReadCoilRegs(byte adresDevice, ushort startAdress, ushort numOfPoint)
        {
            try
            {
                bool[] tempcoil = MasterRTU.ReadCoils(adresDevice, startAdress, numOfPoint);
                string[] dataCoils = new string[tempcoil.Length];
                for (int i = 0; i < tempcoil.Length; i++)
                {
                    dataCoils[i] = Convert.ToUInt16(tempcoil[i]).ToString();
                }
                return dataCoils;
            }
            catch (Exception ex)
            {
                //Mode = eMode.None;               
            }
            return null;
        }

        /// <summary>
        /// Чтение группы регистров типа DI.
        /// </summary>
        /// <param name="adresDevice"></param>
        /// <param name="startAdress"></param>
        /// <param name="numOfPoint"></param>
        /// <returns></returns>
        public string[] ReadInputs(byte adresDevice, ushort startAdress, ushort numOfPoint)
        {
            try
            {
                bool[] tempcoil = MasterRTU.ReadInputs(adresDevice, startAdress, numOfPoint);

                string[] dataCoils = new string[tempcoil.Length];
                for (int i = 0; i < tempcoil.Length; i++)
                {
                    dataCoils[i] = Convert.ToUInt16(tempcoil[i]).ToString();
                }
                return dataCoils;
            }
            catch (Exception ex)
            {
                //Mode = eMode.None;               
            }
            return null;
        }

        /// <summary>
        /// Запись в регистр типа Coil.
        /// </summary>
        /// <param name="slaveID"></param>
        /// <param name="coilAddress"></param>
        /// <param name="value"></param>
        public void WriteCoilRegister(int slaveID, int coilAddress,bool value)
        {
            byte ID = Convert.ToByte(slaveID);
            ushort coilAddr = Convert.ToUInt16(coilAddress);
            try
            {
                MasterRTU.WriteSingleCoil(ID, coilAddr, value);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Прослушивание ответов.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int n = serialPort.BytesToRead;
            byte[] mess = new byte[n];
            serialPort.Read(mess, 0, mess.Length);
            //Если хоть что-то пришло в ответ - заносим порт в список.
            if (mess.Length > 0)
            {
                //Запись порта, по которому идет передача.
                if (!PortsEnabled.Contains(serialPort.PortName))
                {
                    PortsEnabled.Add(serialPort.PortName);                    
                }
                    //Контроль совпадения адреса устройства в пакете и отсутствия его в списке.
                if (mess[0] == tempAdr && !AdressSearch.Contains(mess[0]))
                {
                    AdressSearch.Add(mess[0]);
                }                
            }      
        }        

        /// <summary>
        /// Отправка запроса на устройство для поиска рабочего порта.
        /// </summary>
        /// <param name="adress"></param>
        public void SendResponsePort(int adress, SettingPortStart settingPort)
        {
            serialPort ??= new SerialPort();
            if (PortsEnabled.Count != 0)//Очистка списка доступных портов.
            {
                PortsEnabled.Clear();
            }
            serialPort.DataReceived += Port_DataReceived;//Включение прослушки.
            string[] ports = GetListPorts();             //Получить доступные порты компьютера.
            foreach (var port in ports)                  //Пробуем подключиться на каждом порту.
            {
                settingPort.PortType = port;
                PortOpen(settingPort);
                byte[] b = new byte[6];
                b[0] = (byte)adress;//Адрес устройства.
                b[1] = 0x3;        //Команда 60.
                b[2] = 0x0;        //Адрес регистра.
                b[3] = 0x88;        //Адрес регистра.
                b[4] = 0;           //Количество регистров.
                b[5] = 0x1;         //Количество регистров.
                byte[] crc = ModbusUtility.CalculateCrc(b); //0-low, 1-high
                byte[] mes = new byte[b.Length + crc.Length];
                b.CopyTo(mes, 0);
                crc.CopyTo(mes, mes.Length - crc.Length);
                try
                {
                    serialPort.Write(mes, 0, mes.Length);
                }
                catch (Exception ex)
                {
                    serialPort.Close();
                    Mode = eMode.None;
                }
            }
            Thread.Sleep(800);
            serialPort.DataReceived -= Port_DataReceived;
        }  
        
        int tempAdr;
        /// <summary>
        /// Поиск адреса устройства.
        /// </summary>
        /// <param name="baudRate"></param>
        /// <param name="dataBit"></param>
        /// <param name="parity"></param>
        /// <param name="stopBit"></param>
        public void SearchAddress(int addressStart, int addressEnd, SettingPortStart settingPortStart, 
            SearchAddrViewMod windowSearch=null)
        {   
            TimeOnly timeOnly = new TimeOnly(0,0,0);
            serialPort ??= new SerialPort();
            if (AdressSearch.Count > 0)
            {
                AdressSearch.Clear();
            }
            if (PortsEnabled.Count != 0)//Очистка списка доступных портов.
            {
                PortsEnabled.Clear();
            }
                PortOpen(settingPortStart);            
                serialPort.DataReceived += Port_DataReceived;            
            for (int i = addressStart; i <=addressEnd; i++)
            {
                if (windowSearch.CanSearch)
                {
                    break;
                }
                windowSearch.Address = i;
                windowSearch.TimeCount = timeOnly.Add(TimeSpan.FromSeconds(addressEnd-i)).ToLongTimeString();
                tempAdr = i;//Временно
                windowSearch.ProgBarValue++;
                byte[] b = new byte[6];
                b[0] = (byte)i;     //Адрес устройства.
                b[1] = 0x3;         //Команда 3.
                b[2] = 0x0;         //Адрес регистра.
                b[3] = 0x0;         //Адрес регистра.
                b[4] = 0;           //Количество регистров.
                b[5] = 0x1;         //Количество регистров.
                byte[] crc = ModbusUtility.CalculateCrc(b); //0-low, 1-high
                byte[] mes = new byte[b.Length + crc.Length];
                b.CopyTo(mes, 0);
                crc.CopyTo(mes, mes.Length - crc.Length);
                try
                {
                    serialPort.Write(mes, 0, mes.Length);                    
                }
                catch (Exception ex)
                {
                    serialPort.Close();
                    Mode = eMode.None;
                }               
                Thread.Sleep(settingPortStart.TimeOutWrite);
            }           
                serialPort.DataReceived -= Port_DataReceived;//Отключить прослушку порта.
        }
    
        /// <summary>
        /// Поиск портов на компе.
        /// </summary>
        /// <returns></returns>
        public static string[] GetListPorts()
        {
            return SerialPort.GetPortNames();
        }
    
    }
}
