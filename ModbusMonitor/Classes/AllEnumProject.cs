using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusMonitor.Classes
{

    /// <summary>
    /// Порт открыт/закрыт.
    /// </summary>
    public enum eMode
    {
        None = 0,
        PortOpen
    }
    /// <summary>
    /// Перечисление типов устройств.
    /// </summary>
    public enum EnumModel
    {
        None = 0,
        BKM_3 = 1,
        BKM_4 = 2,
        SKZ = 3,
        SKZ_IP = 4,
        BSZ = 5,
        USIKP = 6
    }

    /// <summary>
    /// Перечисление статусов подключения.
    /// </summary>
    public enum EnumLink
    {
        Unknown = 0,
        LinkNo = 1,
        LinkYes = 2,
        LinkConnect = 3
    }

    /// <summary>
    /// Перечисление типа регистров.
    /// </summary>
    public enum RegisterType
    {
        None,
        DI,
        Coil,
        AI,
        AO
    }

    /// <summary>
    /// Перечисление протокола передачи данных.
    /// </summary>
    public enum ConnectionType
    {
        None,
        RTU,
        IP,
        GSM
    }

    /// <summary>
    /// Класс хранение свойств при конвертации карт регистров.
    /// </summary>
    public class SettingConnectFromMap
    {
        public int DeviceAdress { get; set; }
        public string PortType { get; set; } = string.Empty;
        public int SpeedPort { get; set; }
        public int Parity { get; set; }
        public int Stop_Bit { get; set; }
        public int LenghtWord { get; set; }
        public int TimeOutRead { get; set; }
        public int TimeOutWrite { get; set; }
    }
    
    public class SettingPortStart
    {
        public string PortType { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 9600;
        public int DataBit { get; set; } = 8;
        public Parity ParitySet { get; set; } = Parity.None;
        public int StopBit { get; set; } = 1;
        public int TimeOutWrite { get; set; } = 1000;
        public int TimeOutRead { get; set; } = 1000;
    }
}
