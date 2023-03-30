using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ModbusMonitor.Classes
{
    public class DeviceClass
    {
        public DeviceClass()
        {
            AdaptersArray = new AdaptersArray();
            CellsArray = new List<CellData>();
            ConnectFromMap = new SettingConnectFromMap();
        }

        public ConnectionType ConnectionType { get; set; } //Содержит инвормацию о подключении.
        public SettingConnectFromMap ConnectFromMap { get; set; } //Содержит информацию о подключении.
        public AdaptersArray AdaptersArray { get; set; } //Содержит инвормацию о подключении.
        public List<CellData> CellsArray { get; set; }//Список регистров в устройстве.
        public string DeviceName_DC { get; set; } = string.Empty;//Имя устройства.
        public int DeviceAdress_DC { get; set; }//Адрес устройства.
        public ushort NumOfAO { get; set; } = 0;//Количество регистров АО в устройстве.
        public ushort NumOfAI { get; set; } = 0;//Количество регистров АI в устройстве.
        public ushort NumOfDO { get; set; } = 0;//Количество регистров DO в устройстве.
        public ushort NumOfDI { get; set; } = 0;//Количество регистров DI в устройстве.
        public static EnumLink Link { get; set; } //Устройство подключено/отключено.

        public EnumLink LinkDevice { get; set; } = EnumLink.Unknown;

        /// <summary>
        /// Загрузка карты и обработка данных.
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static DeviceClass LoadMapReg(string FileName)
        {
            DeviceClass device = null; //Объект устройства.
            TextReader reader = new StreamReader(FileName); //Открытие потока чтения данных.
            XmlSerializer serializer = new XmlSerializer(typeof(DeviceClass)); //Объект сериализации.
            try
            {
                device = (DeviceClass)serializer.Deserialize(reader);//Считывание данных с объекта сериализации.
                if (device != null)//Если считывание успешно.
                {
                    return ChangeTIKFormat(device);//Преобразуем данные из карты регистров.
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
            return device;
        }

        /// <summary>
        /// Сохранение карты.
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public bool SaveMapReg(string FileName)
        {
            TextWriter writer = new StreamWriter(FileName);
            XmlSerializer serializer = new XmlSerializer(typeof(DeviceClass));
            serializer.Serialize(writer, this);
            writer.Close();
            return true;
        }

        /// <summary>
        /// Изменение формата данных из TikModbus в ModbusMonitor.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private static DeviceClass ChangeTIKFormat(DeviceClass device)
        {
            if (string.IsNullOrEmpty(device.AdaptersArray.AdapterData.Devices.Device.Adress) ||
                Convert.ToInt32(device.AdaptersArray.AdapterData.Devices.Device.Adress) == 0)
            {
                MessageBox.Show("Адрес устройства - 0!");
                return null;
            }
            int count = 1;//Стартовое значение номеров по порядку.
                          //В "Добавление регистров".
            device.DeviceName_DC = device.AdaptersArray.AdapterData.Devices.Device.Name;
            //В "Добавление регистров".
            device.DeviceAdress_DC = Convert.ToInt32(device.AdaptersArray.AdapterData.Devices.Device.Adress);

            foreach (var item in device.CellsArray)
            {                
                if (ushort.TryParse(item.Value,out ushort val) && ushort.Parse(item.Value) > 32767)//Приведение данных из карты.
                {
                    item.Value = (Convert.ToInt32(item.Value) - ushort.MaxValue - 1).ToString();
                }
                
                item.NumberReg = count++.ToString();
                //Преобразование в бинарный формат.
                if (item.Represent == "Bin" && ushort.TryParse(item.Value, out val))
                {
                    item.Format = item.Represent;
                    item.Value = Convert.ToString(Convert.ToUInt16(item.Value), 2);
                }
                if (item.Type.Contains("Coil"))
                {
                    item.Type = "DO";
                    device.NumOfDO++;
                }
                if (item.Type.Contains("HoldingRegister"))
                {
                    item.Type = "AO";
                    device.NumOfAO++;
                }
                if (item.Type.Contains("InputRegister"))
                {
                    item.Type = "AI";
                    device.NumOfAI++;
                }
                if (item.Type.Contains("DiscreteInput"))
                {
                    item.Type = "DI";
                    device.NumOfDI++;
                }
                item.NameDevice = device.DeviceName_DC;
            }
            device.ConvertConnectData();//Выборка объедененной информации карты.
            return device;
        }

        /// <summary>
        /// Перевод данных подключения из карты регистров TicModscan.
        /// </summary>
        private void ConvertConnectData()
        {
            string pattern = @"\d*";
            Regex regex = new Regex(pattern);
            string[] tempDataArray = AdaptersArray.AdapterData.FullAdapterInf.Split(';');
            List<string> strings = new List<string>();
            for (int i = 0; i < tempDataArray.Length; i++)
            {
                if (!String.IsNullOrEmpty(tempDataArray[i]) && regex.IsMatch(tempDataArray[i]))
                {
                    strings.Add(tempDataArray[i]);
                }
            }
            ConnectFromMap.DeviceAdress = Convert.ToInt32(strings[0]);
            ConnectFromMap.PortType = strings[1];
            ConnectFromMap.SpeedPort = Convert.ToInt32(strings[2]);
            ConnectFromMap.Parity = Convert.ToInt32(strings[3]);
            ConnectFromMap.Stop_Bit = Convert.ToInt32(strings[4]);
            ConnectFromMap.LenghtWord = Convert.ToInt32(strings[5]);
            ConnectFromMap.TimeOutRead = Convert.ToInt32(strings[6]);
            ConnectFromMap.TimeOutWrite = Convert.ToInt32(strings[7]);
        }
    }
}
