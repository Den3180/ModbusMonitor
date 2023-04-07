using ModbusMonitor.Classes;
using ModbusMonitor.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModbusMonitor.ViewModel
{
    public class AddCellViewModel : ChangePropertyClass
    {
        private string insertionPosition="0";
        private string connectionText = string.Empty;
        private string deviceName_AC = string.Empty;
        private string deviceAddress_AC = string.Empty;
        private string cellAddress_AC = "Пример: 1,2,3,5-15";
        private string titleText = string.Empty;
        private string selectedTypeReg = string.Empty;
        private string selectedFormatData=string.Empty;
        private bool canInsertCell;
        private readonly Command insertCellCommand;            
        private readonly AddСellsWindow window;
        private readonly CellData cell_AC;

        public AddCellViewModel(CellData cell, AddСellsWindow window)
        {
            this.window = window;
            cell_AC = cell;
            insertionPosition = string.IsNullOrEmpty(cell.NumberReg)?"0":cell.NumberReg;
            ConnectionText = ModbusRTUASCII.PortsEnabled.Count>0? 
                cell_AC.NameDevice+"-"+ModbusRTUASCII.PortsEnabled.First(): cell_AC.NameDevice + "- нет доступных портов";
            DeviceName_AC = cell_AC.NameDevice;
            DeviceAddress_AC = cell_AC.DeviceAdress;
            TitleText = $"Добавление ячеек в позицию {insertionPosition}";
            SelectedTypeReg = TypeRegister_AC.FirstOrDefault();
            SelectedFormatData = FormatCell_AC.FirstOrDefault(item=>item=="Int");
            CanInsertCell = true;
            insertCellCommand = new Command(InsertCell,()=> CanInsertCell);
            PropertyChanged += AddCellViewModel_PropertyChanged;
        }

        public IEnumerable<string> TypeRegister_AC => typeRegister;
        public IEnumerable<string> FormatCell_AC => nameButton;
        public ICommand InsertCellCommand => insertCellCommand;



        private void InsertCell()
        {            
            window.Content = new List<string>()
            {
                insertionPosition,
                CellAddress_AC,
                SelectedTypeReg,
                SelectedFormatData
            };
            window.Close();
        }

        #region[Привязки]
        /// <summary>
        /// Выбранный формат данных.
        /// </summary>
        public string SelectedFormatData
        {
            get => selectedFormatData;
            set => SetOptions(nameof(SelectedFormatData), ref selectedFormatData, value);
        }
        /// <summary>
        /// Выбранный тип регистра
        /// </summary>
        public string SelectedTypeReg
        {
            get => selectedTypeReg;
            set => SetOptions(nameof(SelectedTypeReg), ref selectedTypeReg, value);        
        }
        /// <summary>
        /// Подключение.
        /// </summary>
        public string ConnectionText
        {
            get => connectionText;
            set => SetOptions(nameof(ConnectionText), ref connectionText, value);
        }
        /// <summary>
        /// Имя устройства.
        /// </summary>
        public string DeviceName_AC
        {
            get => deviceName_AC;
            set => SetOptions(nameof(DeviceName_AC), ref deviceName_AC, value);
        }
        /// <summary>
        /// Адрес устройства.
        /// </summary>
        public string DeviceAddress_AC
        {
            get => deviceAddress_AC;
            set => SetOptions(nameof(DeviceAddress_AC), ref deviceAddress_AC, value);
        }
        /// <summary>
        /// Адреса ячеек.
        /// </summary>
        public string CellAddress_AC
        {
            get => cellAddress_AC;
            set => SetOptions(nameof(CellAddress_AC), ref cellAddress_AC, value);
        }

        public string TitleText
        {
            get => titleText;
            set => SetOptions(nameof(TitleText), ref titleText, value);
        }

        #endregion
        public bool CanInsertCell
        {
            get => canInsertCell;
            set => SetOptions(nameof(CanInsertCell), ref canInsertCell, value);
        }

        private void AddCellViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(CanInsertCell)))
            {
                insertCellCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
