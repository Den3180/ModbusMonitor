using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModbusMonitor.Classes
{
    public class GroupsTreeNode
    {
        public string NameCOM { get; set; } = "no name";
        public ObservableCollection<SubGroupsTree> SubGroups { get; set; }//Источник данных дерева.      
        public GroupsTreeNode()
        {
            SubGroups = new ObservableCollection<SubGroupsTree>();
        }
    }

    public class SubGroupsTree : INotifyPropertyChanged
    {
        private string content = string.Empty;
        public SubGroupsTree(string text = "Статус:\t\t\tОтключено")
        {
            ContentClass = text;
        }

        public string ContentClass
        {
            get => content;
            set
            {
                SetOptions(nameof(ContentClass), ref content, value);
            }
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
