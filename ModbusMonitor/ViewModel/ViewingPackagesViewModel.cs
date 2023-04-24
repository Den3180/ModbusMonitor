using ModbusMonitor.Classes;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusMonitor.ViewModel
{
   public class ViewingPackagesViewModel:ChangePropertyClass
   {
        private string temp = string.Empty;
        public ICollection<DataStruct> sourceData = new ObservableCollection<DataStruct>();

        public ViewingPackagesViewModel()
        {
            
        }        
        
        public IEnumerable<DataStruct> SourceData => sourceData;
        public string Temp
        {
            get => temp;
            set => SetOptions(nameof(Temp), ref temp, value);
        }
    }
}
