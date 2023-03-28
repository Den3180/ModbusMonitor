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

        public DeviceClass OpenFileDialog()//fff.
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

        /// <summary>
        /// Окрывает диалог сохранения карты регистров.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool SaveFileDialog(object obj)
        {
            DeviceClass device = obj as DeviceClass;//Кастуем класс устройства.
            FileInfo file = new FileInfo("ModbusMonitor.exe");//Собираем файловую информацию о исполняемом файле.
            string dir = file.DirectoryName + FileNameMap.MapsOrigin;//Добавляем в путь к исполняемому файлу каталог с картами.
            if (!Directory.Exists(dir))//Если каталога с картами по этому пути нет,то создаем его.
            {
                Directory.CreateDirectory(dir);
            }
            SaveFileDialog saveFile = new SaveFileDialog();//Создаем диалоговое окно сохранения карты.
            saveFile.Filter = "Файлы XML (*.xml)|*.xml|Все файлы (*.*)|*.*";//Устанавливаем фильтр расширений.
            saveFile.InitialDirectory = dir;//Указывем директорию сохранения карты.
            if (saveFile.ShowDialog() == true)//Открываем окно.
            {
                FilePath = saveFile.FileName;//Путь хранения файла.
                device?.SaveMapReg(FilePath);//Схраняем посредством xml-сериализации.
                return true;
            }
            return false;
        }

        public void ShowMessage(string message)
        {

        }
    }
}
