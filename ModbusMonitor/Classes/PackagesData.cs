using System;
using System.Collections;
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
        public static DataStruct GetDataStruct<T>(T dataArray,string type ,DeviceClass device)
        {
            string tempData = string.Empty;
            string tempFcode;
            byte[] y;
            if (dataArray.GetType().Name == "UInt16[]")
            {
                ushort[] arrtemp = dataArray as ushort[];
                y = new byte[arrtemp.Length];
                for (int i = 0; i < y.Length; i++)
                {                
                    y[i] = (byte)arrtemp[i];
                    tempData += "  " + Convert.ToString(arrtemp[i], 16);
                }
            }
            else
            {
                y = new byte[(dataArray as string[]).Length];                 
                for(int i=0;i<(dataArray as string[]).Length;i++)
                {
                    y[i] = Convert.ToByte((dataArray as string[])[i]);
                    tempData += " " + Convert.ToString(y[i],16);
                }                
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
                CRC = GetCRC(y,device.DeviceAdress_DC,tempFcode)
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
                else if ((list[i].Adress - list[i-1].Adress) != 1)
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
        /// <summary>
        /// Вычисление контрольной суммы.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="address"></param>
        /// <param name="fCode"></param>
        /// <returns></returns>
        private static string GetCRC(byte[] y, int address,string fCode)
        {
            string tempCRC = string.Empty;            
            byte[] addrByte = BitConverter.GetBytes(address);
            byte[] fCodeByte = BitConverter.GetBytes(Convert.ToUInt16(fCode.Substring(fCode.Length-1)));
            byte[] tempData = new byte[y.Length + addrByte.Length + fCodeByte.Length];
            addrByte.CopyTo(tempData,0);
            fCodeByte.CopyTo(tempData, addrByte.Length);
            y.CopyTo(tempData, addrByte.Length + fCodeByte.Length);

            var arrCRC = Modbus.Utility.ModbusUtility.CalculateCrc(tempData);
            foreach (var item in arrCRC)
            {
                tempCRC += Convert.ToString(item, 16);
            }
                if(tempCRC.Length<4) tempCRC = "0" + tempCRC;
            return "0x"+tempCRC.ToUpper();
        }
    }
}
