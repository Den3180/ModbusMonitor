using ModbusMonitor.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusMonitor.ViewModel
{
    public class RegisterPropertiesViewModel
    {      
        public RegisterPropertiesViewModel(CellData cellData)
        {            
            CellData = cellData;
        }
        public CellData CellData { get; set; }
    }
}
