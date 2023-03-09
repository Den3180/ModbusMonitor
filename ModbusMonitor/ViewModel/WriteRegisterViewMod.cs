using ModbusMonitor.Classes;
using ModbusMonitor.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ModbusMonitor.ViewModel
{
    class WriteRegisterViewMod : INotifyPropertyChanged
    {
        private string addressDevice=string.Empty;
        private string valueRegister=string.Empty;
        private int selectedTypeRegister;
        private int addressregister;
        private readonly CellData cellData;
        private readonly ICollection<string> connectionPortDevice = new ObservableCollection<string>();
        private writeRegisterWindow window;
        private readonly ICollection<string> typeRegister = new ObservableCollection<string>()
        {
            "None",
            "Discrete Inputs",
            "Coil",
            "Input Registers",
            "Holding Registers"
        };

        public WriteRegisterViewMod(object obj, writeRegisterWindow window )
        {
            this.window = window;
            cellData = obj as CellData;           
            SelectItemTypeRegister();
            PropertyChanged += WriteRegisterViewMod_PropertyChanged;
        }

        public IEnumerable<string> ConnectionPortDevice => connectionPortDevice;
        public IEnumerable<string> TypeRegister => typeRegister;


        /// <summary>
        /// Выбор типа регистра в комбобокс "Тип регистра".
        /// </summary>
        private void SelectItemTypeRegister()
        {
            connectionPortDevice.Add(ModbusRTU.SettingPortStart.PortType + " --- " + cellData.NameDevice);
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
           
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
