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
using System.Windows.Input;
using System.Windows.Media;

namespace ModbusMonitor.ViewModel
{
    public class AddCellViewModel : ChangePropertyClass
    {
        //private string insertionPosition="0";
        private string connectionText = string.Empty;
        private string deviceName_AC = string.Empty;
        private string deviceAddress_AC = string.Empty;
        private string cellAddress_AC = "Пример: 1,2,3,5-15";
        private string titleText = string.Empty;
        private string selectedTypeReg = string.Empty;
        private string selectedFormatData=string.Empty;
        private string patternAddressCells = @"^(\d{1,3},?-?)*\d?$";
        private bool canInsertCell;
        private readonly Command insertCellCommand;            
        private readonly AddСellsWindow window;
        private readonly DeviceClass device_AC;
        private Brush foregrounBrush = Brushes.Gray;

        public AddCellViewModel(DeviceClass device, AddСellsWindow window)
        {
            this.window = window;
            device_AC = device;            
            ConnectionText = ModbusRTUASCII.PortsEnabled.Count>0? 
                device_AC.DeviceName_DC+"-"+ModbusRTUASCII.PortsEnabled.First(): device_AC.DeviceName_DC + "- нет доступных портов";
            DeviceName_AC = device_AC.DeviceName_DC;
            DeviceAddress_AC = device_AC.DeviceName_DC;
            TitleText = $"Добавление ячеек";// в позицию {insertionPosition}";
            SelectedTypeReg = TypeRegister_AC.FirstOrDefault();
            SelectedFormatData = FormatCell_AC.FirstOrDefault(item=>item=="Int");
            CanInsertCell = true;
            insertCellCommand = new Command(InsertCell,()=> CanInsertCell);
            PropertyChanged += AddCellViewModel_PropertyChanged;
        }

        public IEnumerable<string> TypeRegister_AC => typeRegister;
        public IEnumerable<string> FormatCell_AC => nameButton;
        public ICommand InsertCellCommand => insertCellCommand;

        /// <summary>
        /// Сохранение информации об изменениях и выход.
        /// </summary>
        private void InsertCell()
        {
            window.Content = AddCellsMethods.CheckFormatAddressCells(SelectedTypeReg,SelectedFormatData,CellAddress_AC);
            window.Close();
        }
       
        #region[Привязки]

        /// <summary>
        /// Привязка к цветц текста.
        /// </summary>
        public Brush ForegrounBrush
        {
            get => foregrounBrush;
            set => SetOptions(nameof(ForegrounBrush), ref foregrounBrush, value);
        }
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
            set 
            {
                if (!Regex.IsMatch(value, patternAddressCells)) return;
                SetOptions(nameof(CellAddress_AC), ref cellAddress_AC, value);
                ForegrounBrush = Brushes.Black;
            } 
        }
        /// <summary>
        /// Заголовок окна.
        /// </summary>
        public string TitleText
        {
            get => titleText;
            set => SetOptions(nameof(TitleText), ref titleText, value);
        }
        #endregion
        /// <summary>
        /// Доступность команды вставить ячейки.
        /// </summary>
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
