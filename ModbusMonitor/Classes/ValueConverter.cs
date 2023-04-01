using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
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
        public static string ConvertFormatData(ushort cellData, string format)
        {

            string value = format switch
            {
                "Bin" => Convert.ToString(cellData, 2),
                "Hex" => Convert.ToString(cellData, 16).ToUpper(),
                _ => Convert.ToString(cellData)
            };
            if(format=="Hex" && value.Length % 8 != 0)
            {
                return "0" + value;
            }
            return value;
        }

        /// <summary>
        /// Сменить формат в DataGrid при отключенном устройстве.
        /// </summary>
        /// <param name="cellData"></param>
        public static void ChangeFormatData(CellData cellData, string prevFormat)
        {            
            byte[] dataArr = prevFormat switch
            {
                "Bin" => BitConverter.GetBytes(Convert.ToUInt32(cellData.Value, 2)),
                "Hex" => Convert.FromHexString(cellData.Value),
                _ => BitConverter.GetBytes(Convert.ToUInt32(cellData.Value))
            } ;
            if (prevFormat == "Hex")
            {                
                if (dataArr.Length < 4)
                {
                    Array.Resize(ref dataArr, 4);
                }
            }
            if (prevFormat == "Hex" || cellData.Format == "Hex")
            {
                Array.Reverse(dataArr);
            }
            cellData.Value = cellData.Format switch
            {
                "Bin" => Convert.ToString(BitConverter.ToUInt32(dataArr),2),
                "Hex" => Convert.ToHexString(dataArr),
                _ => Convert.ToString(BitConverter.ToUInt32(dataArr))
            };
        }
    }
}
