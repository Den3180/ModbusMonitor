using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusMonitor.Classes
{
    /// <summary>
    /// Класс-конвертер величин.
    /// </summary>
    class ValueConverter
    {
        /// <summary>
        /// Преобразование форматов.
        /// </summary>
        /// <param name="cellData"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ConvertFormatData(ushort cellData, string format) => format switch
        {
            "Bin" => Convert.ToString(cellData, 2),
            "Hex" => Convert.ToString(cellData, 16),
            _ => Convert.ToString(cellData)
        };

        public static void ChangeFormatData(CellData cellData)
        {
            
        }
    }
}
