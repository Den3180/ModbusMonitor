using ModbusMonitor.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusMonitor.ViewModel
{
    public class ControlDeviceView
    {
        public List<CellData> Cells { get; set; }

        public ControlDeviceView(List<CellData> Cells)
        {
            this.Cells = Cells;
        }
    }
}
