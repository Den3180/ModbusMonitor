using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace ModbusMonitor.Classes
{
    public struct DataStruct
    {
        public string DataTransmissionDirection { get; set; }
        public string Type { get; set; }
        public string FunctionalCode { get; set; }
        public string AddressRegister { get; set; }
        public string Data { get; set; }
        public string CRC { get; set; }
    }
    public class PackagesData
    {
        /// <summary>
        /// Формирование данных для отображения пакетов.
        /// </summary>
        /// <param name="dataArray"></param>
        /// <param name="startAddress"></param>
        /// <returns></returns>
        public static DataStruct GetDataStruct(ushort[] dataArray,string type ,DeviceClass device)
        {
            string tempData = string.Empty;
            string tempFcode = string.Empty;
            byte[] y = new byte[dataArray.Length];
            for (int i = 0; i < y.Length; i++)
            {                
                y[i] = (byte)dataArray[i];
                tempData += "  " + Convert.ToString(dataArray[i], 16);
            }
            tempFcode = type switch
            {
               "DI"=>"0x02",
               "DO"=>"0x01",
               "AI"=>"0x04",
               "AO"=>"0x03",
               _=>string.Empty
            };            
            return new DataStruct()
            {
                DataTransmissionDirection = "Ответ",
                Type = type,
                FunctionalCode = tempFcode,
                AddressRegister = GetAddressesString(device.CellsArray, type),
                Data = tempData.ToUpper(),
                CRC = GetCRC(y).ToUpper()
            };
        }
        /// <summary>
        /// Формирование строки с адресами регистров.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetAddressesString(List<CellData> list,string type)
        {
            string addr = string.Empty;
            int start=-1;
            int deltaIndex = 2;
            int deltaOrder = 10;
            for (int i = 0; i < list.Count;i++)
            {
                if (list[i].Type != type) continue;
                if (start<0)
                {
                    start = list[i].Adress;
                    addr = start.ToString();
                }
                else if ((list[i].Adress - list[i-1].Adress) > 1)
                {
                    addr += $", {list[i].Adress}";
                }
                else if ((list[i].Adress - list[i - 1].Adress) == 1)
                {
                    if (addr.Length >=2 && addr[^deltaIndex] == '-')
                    {
                       addr=addr.Remove(addr.Length-deltaIndex+1);
                       addr += list[i].Adress.ToString();
                    }                   
                    else
                    {
                        addr += $"-{list[i].Adress}";
                    }
                    if (list[i].Adress/deltaOrder == 1)
                    {
                        deltaIndex++;
                        deltaOrder *= deltaOrder;
                    }
                }
            }
            return addr;
        }
        private static string GetCRC(byte[] y)
        {
            string tempCRC = string.Empty;
            var arrCRC = Modbus.Utility.ModbusUtility.CalculateCrc(y);
            foreach (var item in arrCRC)
            {
                tempCRC += Convert.ToString(item, 16);
            }
            return tempCRC;
        }
    }
}
