using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ModbusMonitor.Classes
{
    /// <summary>
    /// Узел дерева элементов.
    /// </summary>
    public class GroupsTreeNode : ChangePropertyClass
    {
        private string state = "Отключено";
        private Brush colorTextTreeConnect = Brushes.Red;

        public GroupsTreeNode()
        {
            SubGroups = new ObservableCollection<SubGroupsTree>();            
        }
        public string NameCOM { get; set; } = "no name";
        public ObservableCollection<SubGroupsTree> SubGroups { get; set; }//Источник данных дерева.      

        public string State
        {
            get => state;
            set
            {
                SetOptions(nameof(State), ref state, value);
            }
        }

        public Brush ColorTextTreeConnect
        {
            get => colorTextTreeConnect;
            set => SetOptions(nameof(ColorTextTreeConnect), ref colorTextTreeConnect, value);
        }        
    }
    /// <summary>
    /// Конечные узлы дерева элементов.
    /// </summary>
    public class SubGroupsTree : ChangePropertyClass
    {
        
        private string contentPort = string.Empty;
        private string contentName = string.Empty;
        private string contentAddress = string.Empty;        
        public SubGroupsTree(string port = "не известно", string address="не известно", 
            string name="не известно", string nameCom="не известно")
        {
            SubHeaderPort ="Порт: " ;
            SubHeaderAddress = "Адрес: ";
            SubHeaderName = "Имя: ";
            ContentPort = port;
            ContentAddress = address;
            ContentName = name;
            NameComNode = nameCom;
        }
       
        /// <summary>
        /// Неизменяемая часть.
        /// </summary>
        public string SubHeaderPort { get; set; } = string.Empty;
        public string SubHeaderName { get; set; } = string.Empty;
        public string SubHeaderAddress { get; set; } = string.Empty;
        public string NameComNode { get; set; }
        public Guid DeviceID { get; set; }
        
        /// <summary>
        /// Изменяемая часть.
        /// </summary>
        public string ContentPort
        {
            get => contentPort;
            set => SetOptions(nameof(ContentPort), ref contentPort, value);      
        }
        public string ContentAddress
        {
            get => contentAddress;
            set => SetOptions(nameof(ContentAddress), ref contentAddress, value);            
        }
        public string ContentName
        {
            get => contentName;
            set => SetOptions(nameof(ContentName), ref contentName, value);           
        }        
    }
}
