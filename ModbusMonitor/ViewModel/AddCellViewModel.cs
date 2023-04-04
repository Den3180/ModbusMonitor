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
        private string cellAddress_AC = string.Empty;
        private string titleText = string.Empty;
        private bool canInsertCell;
        private readonly Command insertCellCommand;
        private readonly ICollection<string> cellType_AC = new ObservableCollection<string>();
        private readonly ICollection<string> cellFormat_AC = new ObservableCollection<string>();
        private readonly AddСellsWindow window;
        private readonly CellData cell;

        public AddCellViewModel(CellData cell, AddСellsWindow window)
        {
            this.window = window;
            this.cell = cell;
            insertionPosition = string.IsNullOrEmpty(cell.NumberReg)?"0":cell.NumberReg;
            TitleText = $"Добавление ячеек в позицию {insertionPosition}";
            insertCellCommand = new Command(InsertCell,()=> CanInsertCell);
            PropertyChanged += AddCellViewModel_PropertyChanged;
        }

        public IEnumerable<string> CellType_AC => cellType_AC;
        public IEnumerable<string> CellFormat_AC => cellFormat_AC;
        public ICommand InsertCellCommand => insertCellCommand;

        private void InsertCell()
        {
            window.Close();
        }

        #region[Привязки]
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
