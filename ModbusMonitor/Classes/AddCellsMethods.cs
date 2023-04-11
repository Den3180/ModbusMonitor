using ModbusMonitor.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusMonitor.Classes
{
    class AddCellsMethods
    {
    /// <summary>
    /// Заполнение списка результатов.
    /// </summary>
    /// <returns></returns>
    public static List<CellData> CheckFormatAddressCells(string selectedTypeReg,string selectedFormatData, string cellAddress_AC)
    {
        string typeRegView = selectedTypeReg switch
        {
            "Discrete Inputs" => "DI",
            "Coil" => "DO",
            "Input Registers" => "AI",
            "Holding Registers" => "AO",
            _ => "None"
        };
        List<CellData> resCells = new List<CellData>();
        string[] arrayTemp = cellAddress_AC.Split(',');//Делим строку на подстроки по ','.           
        for (int i = 0; i < arrayTemp.Length; i++)//Заносим в список адреса ячеек.
        {
            if (!arrayTemp[i].Contains('-'))
            {
                resCells.Add(new CellData
                {
                    Type = typeRegView,
                    Format = selectedFormatData,
                    Adress = Int32.Parse(arrayTemp[i])
                });
            }
            else
            {
                string[] subString = arrayTemp[i].Split('-');
                int startIndex = Int32.Parse(Convert.ToString(subString[0]));
                int endIndex = Int32.Parse(Convert.ToString(subString[^1]));
                for (int j = startIndex; j <= endIndex; j++)
                {
                    resCells.Add(new CellData
                    {
                        Type = typeRegView,
                        Format = selectedFormatData,
                        Adress = j
                    });
                }
            }
        }
        resCells.Sort();
        return resCells;
    }
    }
}