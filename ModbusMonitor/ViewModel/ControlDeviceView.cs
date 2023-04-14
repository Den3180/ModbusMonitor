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
using ModbusMonitor.Controls;
using System.Windows.Controls;

namespace ModbusMonitor.ViewModel
{
    public class ControlDeviceView : ChangePropertyClass 
    {
        private readonly Command writeRegisterCommand;
        private readonly Command editFormatCommand;
        private readonly Command addCellsCommand;
        private readonly Command deleteLineCommand;
        private readonly Command showPropertiesCommand;
        private bool canWriteRegister;
        private bool canAddCells;
        private bool canDeleteLine;
        private bool canShowProperties;
        private CellData selectedCell;
        private DataGridColumn gridColumn;
        private List<CellData> cells = new List<CellData>();
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
                if (item is List<CellData>) Cells = item as List<CellData>;//Это элементы уже из карты.
                if (item is ModbusRTUASCII) ModbusRTU = item as ModbusRTUASCII;
                if (item is DeviceClass) CurrentDevice = item as DeviceClass;                
            }
            CanDeleteLine=CanAddCells=CanShowProperties = CurrentDevice!=null;
            for (int i = 0; i < nameFormat.Length; i++)
            {
                MenuItemCheck.Add(new ButtonProp(nameFormat[i]));
            }            
            writeRegisterCommand = new Command(WriteRegister,()=>CanWriteRegister);
            editFormatCommand = new Command(EditFormat);
            addCellsCommand = new Command(AddCells, () => CanAddCells);
            deleteLineCommand = new Command(DeleteLine, () => CanDeleteLine);
            showPropertiesCommand = new Command(ShowProperties,()=>CanShowProperties);
            PropertyChanged += ControlDeviceView_PropertyChanged;
        }

        public ICommand WriteRegisterCommand => writeRegisterCommand;
        public ICommand EditFormatCommand => editFormatCommand;
        public ICommand AddCellsCommand => addCellsCommand;
        public ICommand DeleteLineCommand => deleteLineCommand;
        public ICommand ShowPropertiesCommand => showPropertiesCommand;

        #region[Обработчики команд и методы]
        /// <summary>
        /// Свойства регистра.
        /// </summary>
        private void ShowProperties()
        {
            PropertyRegisterWindow propertyRegisterWindow = new PropertyRegisterWindow(SelectedCell);            
            propertyRegisterWindow.ShowDialog();        
            SelectItemFormat();            
        }

        /// <summary>
        /// Удалить строку.
        /// </summary>
        private void DeleteLine()
        {           
            CurrentDevice.CellsArray.Remove(SelectedCell);           
            DeviceClass.NumberTheList(CurrentDevice);
            Cells = null;
            Cells = CurrentDevice.CellsArray;
        }
        /// <summary>
        /// Вызов окна записи регистров.
        /// </summary>
        private void WriteRegister()
        {
            if (GridColumn.Header.ToString() != "Значение") return;
                WriteRegisterWindow writeRegisterWindow = new WriteRegisterWindow(SelectedCell,ModbusRTU);
                writeRegisterWindow.ShowDialog();                      
                SelectItemFormat();
        } 
        /// <summary>
        /// Добавление ячеек.
        /// </summary>
        private void AddCells()
        {
            AddСellsWindow addСellsWindow = new AddСellsWindow(CurrentDevice);
            addСellsWindow.ShowDialog();
            if (addСellsWindow.Content is not List<CellData> addCells || addCells.Count == 0) return;//Выход, если список пуст.
            CurrentDevice.CellsArray.AddRange(addCells);            
           DeviceClass.NumberTheList(CurrentDevice);
           Cells = null;
           Cells = CurrentDevice.CellsArray;            
        }

        /// <summary>
        /// Редактирование формата при отключенном устройстве.
        /// </summary>
        private void EditFormat()
        {
            foreach (var item in MenuItemCheck)//Ищем новый чек формата.
            {
                //Присваиваем новый тип формата данным.
                if (SelectedCell.Format != item.NameButtonProp && item.CheckButton == true)
                {
                    SelectedCell.Format = item.NameButtonProp;//Изменение формата в таблице.
                    break;
                }
            }
            foreach (var item1 in MenuItemCheck)//Ищем предыдущие чеки.
            {
                //Находим предыдущий формат и сбрасываем его.
                if (SelectedCell.Format != item1.NameButtonProp && item1.CheckButton == true)
                {
                    item1.CheckButton = false;
                }
            }
            SelectItemFormat();
            ValueConverter.ChangeFormatData(SelectedCell);            
        }
        /// <summary>
        /// Выбор формата данных.
        /// </summary>
        private void SelectItemFormat()
        {
            foreach (var item in MenuItemCheck)
            {
                if (SelectedCell.Format != item.NameButtonProp && item.CheckButton == true)
                {
                    item.CheckButton = false;
                }
                else if (SelectedCell.Format == item.NameButtonProp)
                {
                    item.CheckButton = true;
                }
            }
        }
        #endregion

        #region[Свойства-привязки]

        /// <summary>
        /// Привязка к колонке DataGrid.
        /// </summary>
        public DataGridColumn GridColumn
        {
            get => gridColumn;
            set
            {
                SetOptions(nameof(GridColumn), ref gridColumn, value);
            }
        }
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
        /// <summary>
        /// Привязка к списку источника данных.
        /// </summary>
        public List<CellData> Cells
        {
            get => cells;
            set => SetOptions(nameof(Cells), ref cells, value);
        }
        #endregion

        #region[Свойства доступности команд]

        //Доступ команды Свойства.
        public bool CanShowProperties
        {
            get => canShowProperties;
            set => SetOptions(nameof(CanShowProperties), ref canShowProperties, value);
        }
        //Доступ команды удалить.
        public bool CanDeleteLine
        {
            get => canDeleteLine;
            set => SetOptions(nameof(CanDeleteLine), ref canDeleteLine, value);
        }
        //Доступ команды "Записать регистр".
        public bool CanWriteRegister
        {
            get => canWriteRegister;
            set => SetOptions(nameof(CanWriteRegister),ref canWriteRegister,value);
        }
        //Доступ команды "Добавить ячейки".
        public bool CanAddCells
        {
            get => canAddCells;
            set => SetOptions(nameof(CanAddCells), ref canAddCells, value);
        }
        #endregion

        private void ControlDeviceView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(CanWriteRegister)))
            {
                writeRegisterCommand.RaiseCanExecuteChanged();
            }
            if(e.PropertyName.Equals(nameof(CanShowProperties)))
            {
                showPropertiesCommand.RaiseCanExecuteChanged();
            }
        }
       
    }
}
