using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using ModbusMonitor.Interfaces;
namespace ModbusMonitor.Classes
{
    internal class DefaultDialogService : IDialogService
    {
        public string FilePath { get; set; } = string.Empty;

        public DeviceClass OpenFileDialog()
        {
            FileInfo file = new FileInfo("ModbusMonitor.exe");
            string dir = file.DirectoryName + @"\Maps";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Файлы XML (*.xml)|*.xml|Все файлы (*.*)|*.*";
            openFile.InitialDirectory = dir;
            if (openFile.ShowDialog() == true)
            {
                FilePath = openFile.FileName;
                return DeviceClass.LoadMapReg(FilePath);
            }
            return null; 
        }

        public bool SaveFileDialog(object obj)
        {
            DeviceClass device = obj as DeviceClass;
            FileInfo file = new FileInfo("ModbusMonitor.exe");
            string dir = file.DirectoryName + @"\Maps";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "Файлы XML (*.xml)|*.xml|Все файлы (*.*)|*.*";
            saveFile.InitialDirectory = dir;
            if (saveFile.ShowDialog() == true)
            {
                FilePath = saveFile.FileName;
                device?.SaveMapReg(FilePath);
                return true;
            }
            return false;
        }

        public void ShowMessage(string message)
        {

        }
    }
}
