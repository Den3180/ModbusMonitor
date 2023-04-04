using System;
using System.Collections.Generic;
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
        
    

