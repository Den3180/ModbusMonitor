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

namespace ModbusMonitor.Classes
{
    /// <summary>
    /// Узел дерева элементов.
    /// </summary>
    public class GroupsTreeNode : INotifyPropertyChanged
    {
        public string NameCOM { get; set; } = "no name";
        private string state = "Отключено";

        public ObservableCollection<SubGroupsTree> SubGroups { get; set; }//Источник данных дерева.      
        public GroupsTreeNode()
        {
            SubGroups = new ObservableCollection<SubGroupsTree>();            
        }

        public string State
        {
            get => state;
            set
            {
                SetOptions(nameof(State), ref state, value);
            }
        }


        private void SetOptions<T>(string Property, ref T variable, T value)
        {
            if (variable != null && !variable.Equals(value))
            {
                variable = value;
                OnPropertyChanged(new PropertyChangedEventArgs(Property));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
    /// <summary>
    /// Конечные узлы дерева элементов.
    /// </summary>
    public class SubGroupsTree : INotifyPropertyChanged
    {
        
        private string contentPort = string.Empty;
        private string contentName = string.Empty;
        private string contentAddress = string.Empty;        
        public SubGroupsTree(string port = "не известно", string address="не известно", 
            string name="не известно")
        {
            SubHeaderPort ="Порт: " ;
            SubHeaderAddress = "Адрес: ";
            SubHeaderName = "Имя: ";
            ContentPort = port;
            ContentAddress = address;
            ContentName = name;
           
        }
        /// <summary>
        /// Неизменяемая часть.
        /// </summary>
        public string SubHeaderPort { get; set; } = string.Empty;
        public string SubHeaderName { get; set; } = string.Empty;
        public string SubHeaderAddress { get; set; } = string.Empty;
        
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

        private void SetOptions<T>(string Property, ref T variable, T value)
        {
            if (variable!=null && !variable.Equals(value))
            {
                variable = value;
                OnPropertyChanged(new PropertyChangedEventArgs(Property));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
