using ModbusMonitor.Classes;
using ModbusMonitor.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace ModbusMonitor
{
    public partial class ViewModelMain 
    {
        /// <summary>
        /// Определяет какой элемент дерева выбран, узел или подузел.
        /// </summary>
        /// <param name="itemTree"></param>
        /// <returns>Возвращает подузел</returns>
        private static SubGroupsTree DefineNodeInTree(object itemTree)
        {
            if (itemTree is GroupsTreeNode)
            {
                return (itemTree as GroupsTreeNode).SubGroups.First();//Выбираем младший узел при выбранном старшемм узле.
            }
            else
            {
                return itemTree as SubGroupsTree;//Младший узел.
            }
        }
        /// <summary>
        /// Проверка на совпадение узлов дерева с добавляемым новым элементом.
        /// </summary>
        /// <returns></returns>
        private bool CheckForRepeatabilityOfNodes()
        {
            foreach (var node in treeNodes)
            {
                var subGroup = node.SubGroups.First();
                if (subGroup.ContentAddress == device.DeviceAdress_DC.ToString() &&
                    subGroup.ContentName == device.DeviceName_DC &&
                    subGroup.ContentPort == device.ConnectFromMap.PortType)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Есть ли устройство в списке устройств.
        /// </summary>
        /// <returns></returns>
        private bool CheckListDevice()
        {
            foreach (var dev in listDevices)
            {
                if (dev.Equals(device))
                {
                    //device = dev;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Получить выбранную ячейку или первую если ничего не выбрано.
        /// </summary>
        /// <returns></returns>
        private CellData GetSelectedCell()
        {          
            //Если поле имени объекта пустое, значит выделения не произошло, значит вернуть первую ячейку.
            return (Usercontrol.DataContext as ControlDeviceView).SelectedCell.NameDevice!=string.Empty?
                 (Usercontrol.DataContext as ControlDeviceView).SelectedCell:
                 new CellData();           
        }
        /// <summary>
        /// Установка выделенного элемента.
        /// </summary>
        private void SetSelectedDevice()
        {
            if (treeNodes.Count > 0)
            {
                foreach (var item in treeNodes)
                {
                    if (item.SubGroups.First().ContentName == device.DeviceName_DC)
                    {
                        SelectedItemTree = item;
                    }
                }
            }
        }
        /// <summary>
        /// Заполнение окна пакетов.
        /// </summary>
        /// <param name="tempDI"></param>
        /// <param name="tempDO"></param>
        /// <param name="tempAO"></param>
        /// <param name="tempAI"></param>
        private void FillPackage(string[] tempDI, string[] tempDO, ushort[] tempAO, ushort[] tempAI)
        {
            DataStruct dataPackage = new DataStruct();            
            if (tempDO != null)
            {
                dataPackage = PackagesData.GetDataStruct("DO", device);
                dispatcher.Invoke(() => (Usercontrol.DataContext as ViewingPackagesViewModel)?.sourceData.Add(dataPackage));
                tempDO = CheckDataArray(tempDO, device.CellsArray, "DO");
                dataPackage = PackagesData.GetDataStruct(tempDO, "DO", device);
                dispatcher.Invoke(() => (Usercontrol.DataContext as ViewingPackagesViewModel)?.sourceData.Add(dataPackage));
            }
            if (tempDI != null)
            {
                dataPackage = PackagesData.GetDataStruct("DI", device);
                dispatcher.Invoke(() => (Usercontrol.DataContext as ViewingPackagesViewModel)?.sourceData.Add(dataPackage));
                tempDI = CheckDataArray(tempDI, device.CellsArray, "DI");
                dataPackage = PackagesData.GetDataStruct(tempDI, "DI", device);
                dispatcher.Invoke(() => (Usercontrol.DataContext as ViewingPackagesViewModel)?.sourceData.Add(dataPackage));
            }
            if (tempAO != null)
            {
                dataPackage = PackagesData.GetDataStruct("AO", device);
                dispatcher.Invoke(() => (Usercontrol.DataContext as ViewingPackagesViewModel)?.sourceData.Add(dataPackage));
                tempAO = CheckDataArray(tempAO, device.CellsArray, "AO");
                dataPackage = PackagesData.GetDataStruct(tempAO, "AO", device);
                dispatcher.Invoke(() => (Usercontrol.DataContext as ViewingPackagesViewModel)?.sourceData.Add(dataPackage));
            }
            if (tempAI != null)
            {
                dataPackage = PackagesData.GetDataStruct("AI", device);
                dispatcher.Invoke(() => (Usercontrol.DataContext as ViewingPackagesViewModel)?.sourceData.Add(dataPackage));
                tempAI = CheckDataArray(tempAI, device.CellsArray, "AI");
                dataPackage = PackagesData.GetDataStruct(tempAI, "AI", device);
                dispatcher.Invoke(() => (Usercontrol.DataContext as ViewingPackagesViewModel)?.sourceData.Add(dataPackage));

            }
            //dispatcher.Invoke(() => (Usercontrol.DataContext as ViewingPackagesViewModel).sourceData.Add(dataPackage));
            answerRequest = false;
        }
        /// <summary>
        /// Заполнить таблицу.
        /// </summary>
        /// <param name="tempDI"></param>
        /// <param name="tempDO"></param>
        /// <param name="tempAO"></param>
        /// <param name="numOfDI"></param>
        /// <param name="numOfDO"></param>
        /// <param name="numOfAO"></param>
        private void FillCells(string[] tempDI, string[] tempDO, ushort[] tempAO, ushort[] tempAI)
        {
            int iDI = 0;
            int iDO = 0;
            int iAI = 0;
            int iAO = 0;
            int offsetDO = -1;
            int offsetDI = -1;
            int offsetAO = -1;
            int offsetAI = -1;
            foreach (var item in device.CellsArray)
            {
                if (item.Type == "DI" && tempDI != null)
                {
                    offsetDI = offsetDI < 0 ? item.Adress : offsetDI;
                    item.Value = (offsetDI + iDI) == item.Adress ? tempDI[iDI] : item.Value;
                    iDI++;
                }
                else if (item.Type == "DO" && tempDO != null)
                {
                    offsetDO = offsetDO < 0 ? item.Adress : offsetDO;
                    item.Value = (offsetDO + iDO) == item.Adress ? tempDO[iDO] : item.Value;
                    iDO++;
                }
                else if (item.Type == "AO" && tempAO != null)
                {
                    offsetAO = offsetAO < 0 ? item.Adress : offsetAO;
                    var val = (offsetAO + iAO) == item.Adress ? tempAO[iAO] : Int32.Parse(item.Value);
                    val = val > short.MaxValue ? val - ushort.MaxValue - 1 : val;
                    item.Value = ValueConverter.ConvertFormatData(val, item.Format);
                    iAO++;
                }
                else if (item.Type == "AI" && tempAI != null)
                {
                    offsetAI = offsetAI < 0 ? item.Adress : offsetAI;
                    var val = (offsetAI + iAI) == item.Adress ? tempAI[iAI] : Int32.Parse(item.Value);
                    val = val > short.MaxValue ? val - ushort.MaxValue - 1 : val;
                    item.Value = ValueConverter.ConvertFormatData(val, item.Format);
                    iAI++;
                }
            }
            answerRequest = false;
        }
        /// <summary>
        /// Поиск соответствия массива полученных данных соответсвующим эелементам в таблицах и пакетах. 
        /// </summary>
        /// <param name="objData"></param>
        /// <param name="listItems"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private string[] CheckDataArray(string [] objData, List<CellData> listItems, string type)
        {           
            List<string> temp=new List<string>();            
            string[] arrOffset=null;            
                foreach(var item in listItems)
                {
                    if (item.Type == type && (type == "DI" || type == "DO"))
                    {
                        arrOffset = new string[item.Adress + objData.Length];
                        Array.Fill(arrOffset, "0");
                        objData.CopyTo(arrOffset, item.Adress);
                        break;
                    }                   
                } 
                for(int i = 0; i < listItems.Count; i++)
                {
                    for(int j = 0; j < arrOffset?.Length; j++)
                    {
                        if (listItems[i].Adress == j && listItems[i].Type==type)
                        {
                            temp.Add(arrOffset[j]);
                        }
                    }
                }           
         return temp.ToArray();        
        }
        /// <summary>
        ///  Поиск соответствия массива полученных данных соответсвующим эелементам в таблицах и пакетах. 
        /// </summary>
        /// <param name="objData"></param>
        /// <param name="listItems"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private ushort[] CheckDataArray(ushort[] objData, List<CellData> listItems, string type)
        {
            List<ushort> temp = new List<ushort>();
            ushort[] arrOffset = null;
            foreach (var item in listItems)
            {
                if (item.Type == type && (type == "AI" || type == "AO"))
                {
                    arrOffset = new ushort[item.Adress + objData.Length];
                    Array.Fill<ushort>(arrOffset, 0);
                    objData.CopyTo(arrOffset, item.Adress);
                    break;
                }
            }
            for (int i = 0; i < listItems.Count; i++)
            {
                for (int j = 0; j < arrOffset?.Length; j++)
                {
                    if (listItems[i].Adress == j && listItems[i].Type == type)
                    {
                        temp.Add(arrOffset[j]);
                    }
                }
            }
            return temp.ToArray();
        }
    }
}
