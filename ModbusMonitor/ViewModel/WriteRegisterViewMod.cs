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
    public class WriteRegisterViewMod : ChangePropertyClass
    {
        private string addressDevice=string.Empty;
        private string valueRegister=string.Empty;
        private int selectedTypeRegister;
        private int addressregister;
        private bool canWriteRegistry;
        private readonly Window window;
        private readonly Command writeRegistryCommand;
        private readonly Command checkTypeDataCommand;
        private readonly CellData cellData;
        private readonly ICollection<string> connectionPortDevice = new ObservableCollection<string>();
        
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
            checkTypeDataCommand = new Command(CheckTypeData);
            PropertyChanged += WriteRegisterViewMod_PropertyChanged;
        }

        public ModbusRTUASCII ModbusRTU { get; set; }
        public List<ButtonProp> CheckRadioButtons { get; set; } = new List<ButtonProp>(8); 
        public IEnumerable<string> ConnectionPortDevice => connectionPortDevice;       
        public ICommand WriteRegistryCommand => writeRegistryCommand;
        public ICommand CheckTypeDataCommand => checkTypeDataCommand;

        private void CheckTypeData()
        {
            string typeData=string.Empty;
            foreach(var rButton in CheckRadioButtons)//Ищем и запоминаем выбраный тип данных.
            {
                if (rButton.CheckButton == true)
                {
                    typeData = rButton.NameButtonProp;
                }
            }
            if(Regex.IsMatch(ValueRegister, patternHex, RegexOptions.IgnoreCase))
            {

            }
        }
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

            foreach (var item in CheckRadioButtons)//Определяем какой чек выбран.
            {
                if (item.CheckButton == true)
                {
                    cellData.Format = item.NameButtonProp;//Присваиваем этот чек элементу в таблице данных.
                    break;
                }
            }
            if (new Regex(patternBin).IsMatch(data))
            {
                tempFormat = "Bin";
            }
            else if (new Regex(patternHex).IsMatch(data)&& Convert.ToInt32(data, 16)<=UInt16.MaxValue)
            {
                tempFormat = "Hex";
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
            for(int i = 0; i < CheckRadioButtons.Capacity; i++)
            {
                if (cellData.Format == NameButton[i])//Если формат выбранного элемента совпал с именем кнопки.
                {
                    CheckRadioButtons.Add(new ButtonProp(NameButton[i],true));                    
                }
                else
                {
                    CheckRadioButtons.Add(new ButtonProp(NameButton[i]));
                }
            }
            if (cellData.Format == nameof(TypeData.Bin))
            {               
                ValueRegister = ValueConverter.RepresentBinFormat(cellData.Value);
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
        private void WriteRegisterViewMod_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(CanWriteRegistry)))
            {
                writeRegistryCommand.RaiseCanExecuteChanged();
            }
        }        
    }


    /// <summary>
    /// Класс свойств IsChecked radiobutton.
    /// </summary>
    public class ButtonProp : ChangePropertyClass
    {
        private bool checkButton;
        public ButtonProp(string name,bool val=false)
        {
            NameButtonProp = name;
            checkButton = val;
        }
        public string NameButtonProp { get; set; }
        public bool CheckButton
        {
            get => checkButton;
            set
            {
                SetOptions(nameof(CheckButton), ref checkButton, value);
            }
        }       
    }
}
