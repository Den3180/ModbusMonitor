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
        protected string patternBin = @"^[01]{4}\s([01]{4}\s)*[01]{4}$";       //Паттерн бинарного числа.
        protected string patternHex = @"^0x[0-9ABCDEF]*$"; //Паттерн hex.
        protected string patternInt = @"^[1-9]{1}[0-9]*$"; //Паттерн Int.

        protected readonly ICollection<string> typeRegister = new ObservableCollection<string>()
        {
            "None",
            "Discrete Inputs",
            "Coil",
            "Input Registers",
            "Holding Registers"
        };        
        protected string[] nameButton  = new string[]
           {
               "Bin","Hex","Int","Decimal","Float","swFloat",
               "Double","swDouble"
           };

        /// <summary>
        /// Настройка изменяющихся свойств.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Property"></param>
        /// <param name="variable"></param>
        /// <param name="value"></param>
        protected void SetOptions<T>(string Property, ref T variable, T value)
            {               
                    variable = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(Property));                
            }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
        
    

