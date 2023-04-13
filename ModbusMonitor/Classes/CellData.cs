using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusMonitor.Classes
{
    #region[Классы обработки данных подключения карт регистров]
    public class Device
    {
        public string Adress { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class Devices
    {
        public Device Device { get; set; }
        public Devices()
        {
            Device = new Device();
        }
    }

    public class AdapterData
    {
        public string FullAdapterInf { get; set; } = string.Empty;
        public Devices Devices { get; set; } = new Devices();
        public string DefaultDeviceAdress { get; set; } = string.Empty;
        public AdapterData()
        {
        }
    }

    public class AdaptersArray
    {
        public AdapterData AdapterData { get; set; }
        public AdaptersArray()
        {
            AdapterData = new AdapterData();
        }
    }
    #endregion

    public class CellTypeComparer : IComparer<CellData>
    {
        public int Compare(CellData x, CellData y)
        {
            if (x is CellData && y is CellData)
            {               
                return x.Adress.CompareTo(y.Adress);                
            }
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Класс ячеек для регистров из карты TikModbus.
    /// </summary>
    public class CellData : ChangePropertyClass,IComparable<CellData>
    {
        private int adress;//Адрес регистра.
        private string numberReg = string.Empty;//Номер по порядку.
        private string nameDevice = string.Empty;//Имя устройства.
        private string name = "None";//Имя-описание регистра.
        private string _value = string.Empty;//Значение регистра.
        private string format = string.Empty;
        public string Type { get; set; } = string.Empty;//Тип регистра.
        public string Represent { get; set; } = string.Empty;//Первичный тип регистра(не обработанный)
        public string AdapterId { get; set; } = string.Empty;//Данные в карте.
        public string DeviceAdress { get; set; } = string.Empty;//Адрес устройства.
        public string isHaveData { get; set; } = string.Empty;//Если есть данные в карте.
        
        public int CompareTo(CellData cell)//Метод сравнения при сортировке.
        {           
           return Adress.CompareTo(cell.Adress);            
        } 
        /// <summary>
        /// Формат регистра.
        /// </summary>
        public string Format 
        { 
            get=> format;
            set=> SetOptions(nameof(Format), ref format, value);
        }

        /// <summary>
        /// Номер регистра по порядку списка.
        /// </summary>
        public string NumberReg
        {
            get => numberReg;
            set => SetOptions(nameof(NumberReg), ref numberReg, value);            
        }

        /// <summary>
        /// Имя устройства.
        /// </summary>
        public string NameDevice
        {
            get => nameDevice;
            set => SetOptions(nameof(NameDevice), ref nameDevice, value);            
        }

        /// <summary>
        /// Имя регистра.
        /// </summary>
        public string Name
        {
            get => name;
            set => SetOptions(nameof(Name), ref name, value);
        }

        /// <summary>
        /// Значение регистра.
        /// </summary>
        public string Value
        {
            get => _value;
            set => SetOptions(nameof(Value), ref _value, value);            
        }

        /// <summary>
        /// Адрес регистра.
        /// </summary>
        public int Adress
        {
            get => adress;
            set => SetOptions(nameof(Adress), ref adress, value);            
        }       
    }
}
