using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

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
        public static string ConvertFormatData(int cellData, string format)
        {
            string value = format switch//Преобразование полученных данных в строку.
            {
                "Bin" =>RepresentBinFormat(Convert.ToString(cellData, 2)),
                "Hex" =>RepresentHexFormat(Convert.ToString(cellData, 16).ToUpper(),false),
                _ => Convert.ToString(cellData)
            };           
            return value;
        }
        /// <summary>
        /// Сменить формат.
        /// </summary>
        /// <param name="cellData"></param>
        public static void ChangeFormatData(CellData cellData)
        {
            byte[] dataArr;
            bool pos_negFlag=false;
            if (Int32.TryParse(cellData.Value,out int res)==true && res<0)
            {
                pos_negFlag = true;
            } 
            if (cellData.Value.Contains("0x"))//Если предыдущий формат Hex.
            {                
                cellData.Value= cellData.Value.Remove(0, 2);//Удаляем 0x
                dataArr = Convert.FromHexString(cellData.Value);
                Array.Reverse(dataArr);
            }
            else if (cellData.Value.Contains(' '))//Если предыдущий формат Bin.
            {
              for(int i = 0; i < cellData.Value.Length; i++)//Удаляем пробелы.
              {
                    if (cellData.Value[i]==' ')
                    {
                       cellData.Value= cellData.Value.Remove(i,1);
                        i = 0;
                    }
              }
                dataArr = BitConverter.GetBytes(Convert.ToInt16(cellData.Value, 2));
            }
            else
            {
                dataArr = BitConverter.GetBytes(Convert.ToInt16(cellData.Value));
            }
            if(cellData.Format=="Hex") Array.Reverse(dataArr);            
            cellData.Value = cellData.Format switch
            {
                "Bin" => RepresentBinFormat( Convert.ToString(BitConverter.ToInt16(dataArr), 2)),
                "Hex" => RepresentHexFormat(Convert.ToHexString(dataArr),pos_negFlag),
                _ => Convert.ToString(BitConverter.ToInt16(dataArr))
            };
        } 
        /// <summary>
        /// Сменить формат.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ChangeFormatData(string data,string format)
        {
            byte[] dataArr;
            bool pos_negFlag = false;
            if (Int32.TryParse(data, out int res) == true && res < 0)
            {
                pos_negFlag = true;
            }
            if (data.Contains("0x"))//Если предыдущий формат Hex.
            {
                data = data.Remove(0, 2);//Удаляем 0x
                dataArr = Convert.FromHexString(data);
                Array.Reverse(dataArr);
            }
            else if (data.Contains(' '))//Если предыдущий формат Bin.
            {
                for (int i = 0; i < data.Length; i++)//Удаляем пробелы.
                {
                    if (data[i] == ' ')
                    {
                        data = data.Remove(i, 1);
                        i = 0;
                    }
                }
                dataArr = BitConverter.GetBytes(Convert.ToInt16(data, 2));
            }
            else
            {
                dataArr = BitConverter.GetBytes(Convert.ToInt16(data));
            }
            if (format == "Hex") Array.Reverse(dataArr);
             return format switch
                            {
                                "Bin" => RepresentBinFormat(Convert.ToString(BitConverter.ToInt16(dataArr), 2)),
                                "Hex" => RepresentHexFormat(Convert.ToHexString(dataArr), pos_negFlag),
                                _ => Convert.ToString(BitConverter.ToInt16(dataArr))
                            };
        }
        /// <summary>
        /// Конечный бинарный вид.
        /// </summary>
        /// <param name="valueData"></param>
        /// <returns></returns>
        public static string RepresentBinFormat(string valueData)
        {
            int delta1 = 16 - valueData.Length < 0 ? 4 - valueData.Length % 4 : 16 - valueData.Length;
            string valcell = new string('0', delta1);
            valcell += valueData;
            int j = 0;
            for (int i = 0; i < valcell.Length; i++)
            {
                if (i > 0 && j > 0 && j % 4 == 0 && valcell[i] != ' ')
                {
                    valcell = valcell.Insert(i, " ");
                    j = 0;
                    continue;
                }
                j++;
            }
            if(valcell.Split(' ')[0]=="0000" && valcell.Length>16)//Проверка на нулевое значение старшего бита.
            {
                valcell=valcell.Remove(0, 5);//Удаляем бит с нулями.
            }
            return valcell;
        }
        /// <summary>
        /// Конечный вид Hex.
        /// </summary>
        /// <param name="valueData"></param>
        /// <returns></returns>
        public static string RepresentHexFormat(string valueData, bool pos_negFlag)
        {
            int delta1 = 4 - valueData.Length < 0 ? 4 - valueData.Length % 4 : 4 - valueData.Length;            
            string valcell = pos_negFlag == false? new string('0', delta1): new string('F', delta1);
            valcell = "0x"+ valcell+valueData;
            return valcell;
        }
        /// <summary>
        /// Перевод конечного формата Hex в строку формата записи.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string RepresentHexToString(string data)
        {
            if (data.Contains("0x"))//Если предыдущий формат Hex.
            {
                byte[] dataArr;
                data = data.Remove(0, 2);//Удаляем 0x
                dataArr = Convert.FromHexString(data);                
                return Convert.ToHexString(dataArr);
            }
            return string.Empty;
        }
        /// <summary>
        /// Перевод конечного формата Bin в строку формата записи.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string RepresentBinToString(string data)
        {
            if (data.Contains(' '))//Если предыдущий формат Bin.
            {
                for (int i = 0; i < data.Length; i++)//Удаляем пробелы.
                {
                    if (data[i] == ' ')
                    {
                        data = data.Remove(i, 1);
                        i = 0;
                    }
                }
                return data;
            }
            return string.Empty;
        }  
    }
}
