using ModbusMonitor.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ModbusMonitor.Windows;

namespace ModbusMonitor.ViewModel
{
    public class ControlDeviceView : ChangePropertyClass //INotifyPropertyChanged
    {
        private readonly Command writeRegisterCommand;
        private readonly Command editFormatCommand;
        private bool canWriteRegister;
        private CellData selectedCell;        
        public List<CellData> Cells { get; set; }
        public List<ButtonProp> MenuItemCheck { get; set; } = new List<ButtonProp>(8);
        public ModbusRTUASCII ModbusRTU { get; set; }
        public DeviceClass CurrentDevice { get; set; }
        private readonly string [] nameFormat = new string[] {"Bin","Hex","Int","Decimal", "Double", 
                                                     "swDouble","Float","swFloat" 
                                                    }; 

        public ControlDeviceView(params object[] objects)
        {
            selectedCell = new CellData();
            foreach (var item in objects)
            {
                if (item is List<CellData>) Cells = item as List<CellData>;
                if (item is ModbusRTUASCII) ModbusRTU = item as ModbusRTUASCII;
                if (item is DeviceClass) CurrentDevice = item as DeviceClass;
            }
            for (int i = 0; i < nameFormat.Length; i++)
            {
                MenuItemCheck.Add(new ButtonProp(nameFormat[i]));
            }
            writeRegisterCommand = new Command(WriteRegister,()=>CanWriteRegister);
            editFormatCommand = new Command(EditFormat);
            PropertyChanged += ControlDeviceView_PropertyChanged;
        }

        public ICommand WriteRegisterCommand => writeRegisterCommand;
        public ICommand EditFormatCommand => editFormatCommand;

        #region[Обработчики команд и методы]
        /// <summary>
        /// Вызов окна записи регистров.
        /// </summary>
        private void WriteRegister()
        {           
            WriteRegisterWindow writeRegisterWindow = new WriteRegisterWindow(SelectedCell,ModbusRTU);
            writeRegisterWindow.ShowDialog();
        }
        /// <summary>
        /// Редактирование формата при отключенном устройстве.
        /// </summary>
        private void EditFormat()
        {
            bool flag = false; //индикатор совпадений.
            string prevFormat = string.Empty;
            foreach(var item in MenuItemCheck)//Ищем новый чек формата.
            {
                if (item.CheckButton == true && SelectedCell.Format!=item.NameButtonProp)//Присваиваем новый тип формата данным.
                {
                    SelectedCell.Format = item.NameButtonProp;
                    break;
                }               
            }
            foreach (var item1 in MenuItemCheck)//Ищем предыдущие чеки.
            {
                if (SelectedCell.Format != item1.NameButtonProp && item1.CheckButton==true)//Сбрасываем их значения.
                {
                    item1.CheckButton = false;
                    prevFormat = item1.NameButtonProp;
                    flag = true;                    
                }
            }
                //Если устройство не подключено и изменеие формата не было.
            if (CurrentDevice.LinkDevice == EnumLink.LinkNo || CurrentDevice.LinkDevice == EnumLink.Unknown && flag)
            {
                ValueConverter.ChangeFormatData(SelectedCell,prevFormat);
            }
        }
        /// <summary>
        /// Выбор формата данных.
        /// </summary>
        private void SelectItemFormat()
        {
            foreach(var item in MenuItemCheck)
            {
                if (SelectedCell.Format!=item.NameButtonProp && item.CheckButton==true)
                {
                    item.CheckButton = false;
                }
                else if(SelectedCell.Format == item.NameButtonProp)
                {
                    item.CheckButton = true;
                }
            }
        }
        #endregion

        #region[Свойства-привязки]
        //Привязка к выделенному элементу DataGrid.
        public CellData SelectedCell
        {
            get => selectedCell;
            set
            {
                SetOptions(nameof(SelectedCell), ref selectedCell, value);
                if (value != null)
                {
                    //Запись достуна только для Coil и Holding.
                    if ((value.Type=="AO" || value.Type == "DO")&& ModbusRTUASCII.Mode==eMode.PortOpen 
                        && CurrentDevice.LinkDevice== EnumLink.LinkYes) 
                    {
                        CanWriteRegister = true;
                    }
                    else
                    {                        
                        CanWriteRegister = false;                        
                    }                    
                        SelectItemFormat();
                }
            }
        }
        #endregion

        #region[Свойства доступности команд]
        //Доступ команды "Записать регистр".
        public bool CanWriteRegister
        {
            get => canWriteRegister;
            set => SetOptions(nameof(CanWriteRegister),ref canWriteRegister,value);
        }

        #endregion
       
        private void ControlDeviceView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(CanWriteRegister)))
            {
                writeRegisterCommand.RaiseCanExecuteChanged();
            }
        }
       
    }
}
