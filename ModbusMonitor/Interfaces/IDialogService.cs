using ModbusMonitor.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusMonitor.Interfaces
{
    internal interface IDialogService
    {
        void ShowMessage(string message);   // показ сообщения
        string FilePath { get; set; }   // путь к выбранному файлу
        DeviceClass OpenFileDialog();  // открытие файла
        bool SaveFileDialog(object obj);  // сохранение файла
    }
}
