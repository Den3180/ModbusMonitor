using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusMonitor
{
    /// <summary>
    /// Класс изменяемых свойств.
    /// </summary>
    public class ChangePropertyClass : INotifyPropertyChanged
    {
        private readonly ICollection<string> typeRegister = new ObservableCollection<string>()
        {
            "None",
            "Discrete Inputs",
            "Coil",
            "Input Registers",
            "Holding Registers"
        };
        public IEnumerable<string> TypeRegister => typeRegister;


        /// <summary>
        /// Настройка изменяющихся свойств.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Property"></param>
        /// <param name="variable"></param>
        /// <param name="value"></param>
        protected void SetOptions<T>(string Property, ref T variable, T value)
            {
                if (variable != null && !variable.Equals(value))
                {
                    variable = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(Property));
                }
            }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
        
    

