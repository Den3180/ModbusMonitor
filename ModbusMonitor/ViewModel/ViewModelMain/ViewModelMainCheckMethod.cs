using ModbusMonitor.Classes;
using ModbusMonitor.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    device = dev;
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
            return (Usercontrol.DataContext as ControlDeviceView).SelectedCell.NameDevice!=string.Empty?
                 (Usercontrol.DataContext as ControlDeviceView).SelectedCell:
                 (Usercontrol.DataContext as ControlDeviceView).Cells[0];
        }

    }
}
