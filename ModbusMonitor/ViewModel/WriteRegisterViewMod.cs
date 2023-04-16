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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ModbusMonitor.ViewModel
{
    public class WriteRegisterViewMod : ChangePropertyClass
    {
        private string addressDevice=string.Empty;
        private string valueRegister=string.Empty;
        private string dataFormat=string.Empty;
        private string nameRegister = string.Empty;
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
            dataFormat = cellData.Format;            
            if(cellData!=null) SelectItemTypeRegister();
            writeRegistryCommand = new Command(WriteRegistry, () => CanWriteRegistry);
            checkTypeDataCommand = new Command(CheckTypeData);
            PropertyChanged += WriteRegisterViewMod_PropertyChanged;
        }

        public ModbusRTUASCII ModbusRTU { get; set; }
        public List<ButtonProp> CheckRadioButtons { get; set; } = new List<ButtonProp>(8); 
        public IEnumerable<string> ConnectionPortDevice => connectionPortDevice;
        public IEnumerable<string> TypeRegister => typeRegister;
        public ICommand WriteRegistryCommand => writeRegistryCommand;
        public ICommand CheckTypeDataCommand => checkTypeDataCommand;

        private void CheckTypeData()
        {            
            foreach(var rButton in CheckRadioButtons)//Ищем и запоминаем выбраный тип данных.
            {
                if (rButton.CheckButton == true)
                {
                    ValueRegister= ValueConverter.ChangeFormatData(ValueRegister, rButton.NameButtonProp);
                    dataFormat = rButton.NameButtonProp;
                }                
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
                    int value;
                    string valString;
                    if(dataFormat=="Bin" && Regex.IsMatch(ValueRegister, patternBin))
                    {
                        valString = ValueConverter.RepresentBinToString(ValueRegister);
                        value = Convert.ToInt32(valString, 2);
                    }
                    else if (dataFormat == "Hex" && Regex.IsMatch(ValueRegister,patternHex))
                    {
                        valString = ValueConverter.RepresentHexToString(ValueRegister);
                        value = Convert.ToInt32(valString, 16);
                    }
                    else if (dataFormat == "Int" && Regex.IsMatch(ValueRegister, patternInt))
                    {                    
                        value = Convert.ToInt32(ValueRegister);
                    }
                    else
                    {                    
                        value = Convert.ToInt32(ValueRegister);
                    }
                    ModbusRTU.WriteHoldingRegister(slaveID, regAddress, value);
                    cellData.Format = dataFormat;
                }            
           
            window.Close();
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
            NameRegister = cellData.Name;
            ValueRegister = cellData.Value;
            SelectedTypeRegister = cellData.Type switch
            {
                "DI" => 1,
                "DO" => 2,
                "AI" => 3,
                "AO" => 4,
                 _ => 0
            };

            //Выставление флажка кнопки.
            for(int i = 0; i < CheckRadioButtons.Capacity; i++)
            {
                if (cellData.Format == nameButton[i])//Если формат выбранного элемента совпал с именем кнопки.
                {
                    CheckRadioButtons.Add(new ButtonProp(nameButton[i],true));                    
                }
                else
                {
                    CheckRadioButtons.Add(new ButtonProp(nameButton[i]));
                }
            }           
        }       

        /// <summary>
        /// Привязка выбранного элемента поля "Значение".
        /// </summary>
        public string ValueRegister
        {
            get => valueRegister;
            set
            {                
                SetOptions(nameof(ValueRegister), ref valueRegister, value); 
            }
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
        /// Имя регистра.
        /// </summary>
        public string NameRegister
        {
            get => nameRegister;
            set => SetOptions(nameof(NameRegister), ref nameRegister, value);
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
