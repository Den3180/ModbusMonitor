using ModbusMonitor.Classes;
using ModbusMonitor.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ModbusMonitor.ViewModel
{
    public class WriteRegisterViewMod : INotifyPropertyChanged
    {
        private string addressDevice=string.Empty;
        private string valueRegister=string.Empty;
        private int selectedTypeRegister;
        private int addressregister;
        private bool canWriteRegistry;
        private readonly Window window;
        private readonly Command writeRegistryCommand;
        private readonly CellData cellData;
        private readonly ICollection<string> connectionPortDevice = new ObservableCollection<string>();        
        private readonly ICollection<string> typeRegister = new ObservableCollection<string>()
        {
            "None",
            "Discrete Inputs",
            "Coil",
            "Input Registers",
            "Holding Registers"
        };
        
        public WriteRegisterViewMod(params object[] objects )
        {
            foreach(var item in objects)
            {
                if(item is CellData)   cellData = item as CellData;
                if (item is ModbusRTUASCII) ModbusRTU = item as ModbusRTUASCII;
                if (item is Window) window = item as Window;
            }
            if(cellData!=null) SelectItemTypeRegister();
            writeRegistryCommand = new Command(WriteRegistry, () => CanWriteRegistry);
            PropertyChanged += WriteRegisterViewMod_PropertyChanged;
        }

        public ModbusRTUASCII ModbusRTU { get; set; }
        public List<ButtonProp> CheckRadioButtons { get; set; } = new List<ButtonProp>(8); 
        public IEnumerable<string> ConnectionPortDevice => connectionPortDevice;
        public IEnumerable<string> TypeRegister => typeRegister;
        public ICommand WriteRegistryCommand => writeRegistryCommand;

        /// <summary>
        /// Запись в одиночный регистр.
        /// </summary>
        private void WriteRegistry()
        {
            int slaveID = Convert.ToInt32(AddressDevice);
            int regAddress = Convert.ToInt32(AddressRegister);
            if (cellData.Type == "DO")
            {
                bool value = Convert.ToBoolean(Int32.Parse(ValueRegister));
                ModbusRTU.WriteCoilRegister(slaveID, regAddress, value);
            }
            else if (cellData.Type == "AO")
            {               
                int value = ConvertFormat(ValueRegister);
                ModbusRTU.WriteHoldingRegister(slaveID, regAddress, value);
            }
            window.Close();
        }
        /// <summary>
        /// Преобразование формата данных.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private int ConvertFormat(string data)
        {           
            string tempFormat = string.Empty;
            string patternBin = @"^[01]{4,}$";
            string patternHex = @"^[0-9ABCDEF]{1,}$";
            if (new Regex(patternBin).IsMatch(data))
            {
                tempFormat = "Bin";
            }
            else if (new Regex(patternHex).IsMatch(data)&& Convert.ToInt32(data, 16)<=UInt16.MaxValue)
            {
                tempFormat = "Hex";
            }
            foreach (var item in CheckRadioButtons)
            {
                if (item.CheckButton == true)
                {
                    cellData.Format = item.NameButton;                   
                }
            }
            return tempFormat switch
            {
                "Bin" => Convert.ToInt32(data, 2),
                "Hex" => Convert.ToInt32(data, 16),
                _ => Convert.ToInt32(data)
            } ;
        }

        /// <summary>
        /// Заполнение формы в окна записи регистра.
        /// </summary>
        private void SelectItemTypeRegister()
        {
            if (ModbusRTUASCII.Mode==eMode.PortOpen &&(cellData.Type=="AO"||cellData.Type=="DO"))
            {
                CanWriteRegistry = true;
            }
            connectionPortDevice.Add(ModbusRTUASCII.SettingPortStart.PortType + " --- " + cellData.NameDevice);
            AddressDevice = cellData.DeviceAdress;
            AddressRegister = cellData.Adress;
            ValueRegister = cellData.Value;
            SelectedTypeRegister = cellData.Type switch
            {
                "DI" => 1,
                "DO" => 2,
                "AI" => 3,
                "AO" => 4,
                 _ => 0
            };
            string[] NameButton = new string[] 
            { "Decimal","Int","Bin","Hex","Float","swFloat",
               "Double","swDouble"
            };          
            for(int i = 0; i < CheckRadioButtons.Capacity; i++)
            {
                if (cellData.Format == NameButton[i])
                {
                    CheckRadioButtons.Add(new ButtonProp(NameButton[i],true));                    
                }
                else
                {
                    CheckRadioButtons.Add(new ButtonProp(NameButton[i]));
                }
            }
        }       

        /// <summary>
        /// Привязка выбранного элемента поля "Значение".
        /// </summary>
        public string ValueRegister
        {
            get => valueRegister;
            set => SetOptions(nameof(ValueRegister), ref valueRegister, value);
        }
        /// <summary>
        /// Привязка выбранного элемента поля "Адрес регистра".
        /// </summary>
        public int AddressRegister
        {
            get => addressregister;
            set => SetOptions(nameof(AddressRegister), ref addressregister, value);
        }
        /// <summary>
        /// Привязка поля "Адрес устройства".
        /// </summary>
        public string AddressDevice
        {
            get => addressDevice;
            set => SetOptions(nameof(AddressDevice), ref addressDevice, value);
        }
        /// <summary>
        /// Привязка выбранного элемента поля "Тип регистра".
        /// </summary>
        public int SelectedTypeRegister
        {
            get => selectedTypeRegister;
            set => SetOptions(nameof(SelectedTypeRegister), ref selectedTypeRegister, value);
        }
        /// <summary>
        /// Свойство доступности команды.
        /// </summary>
        public bool CanWriteRegistry
        {
            get => canWriteRegistry;
            set => SetOptions(nameof(CanWriteRegistry), ref canWriteRegistry, value);
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
        private void WriteRegisterViewMod_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(CanWriteRegistry)))
            {
                writeRegistryCommand.RaiseCanExecuteChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
    /// <summary>
    /// Класс свойств IsChecked radiobutton.
    /// </summary>
    public class ButtonProp : INotifyPropertyChanged
    {
        private bool checkButton;
        public ButtonProp(string name,bool val=false)
        {
            NameButton = name;
            checkButton = val;
        }
        public string NameButton { get; set; }
        public bool CheckButton
        {
            get => checkButton;
            set
            {
                SetOptions(nameof(CheckButton), ref checkButton, value);
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
