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
    public class ControlDeviceView : INotifyPropertyChanged
    {
        private readonly Command writeRegisterCommand;
        private bool canWriteRegister;
        private CellData selectedCell;
        public List<CellData> Cells { get; set; }

        public ControlDeviceView(List<CellData> Cells)
        {
            this.Cells = Cells;
            selectedCell = null;
            writeRegisterCommand = new Command(WriteRegister,()=>CanWriteRegister);
            PropertyChanged += ControlDeviceView_PropertyChanged;
        }

        public ICommand WriteRegisterCommand => writeRegisterCommand;

        #region[Обработчики команд]
        /// <summary>
        /// Вызов окна записи регистров.
        /// </summary>
        private void WriteRegister()
        {
            writeRegisterWindow writeRegisterWindow = new writeRegisterWindow();
            writeRegisterWindow.ShowDialog();
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
                    if (value.Type=="AO" || value.Type == "DO")
                    {
                        CanWriteRegister = true;
                    }
                    else
                    {                        
                        CanWriteRegister = false;
                    }
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
        private void ControlDeviceView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(CanWriteRegister)))
            {
                writeRegisterCommand.RaiseCanExecuteChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
