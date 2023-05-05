using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using ModbusMonitor.Interfaces;
namespace ModbusMonitor.Classes
{
    internal class SaveLoadService : IDialogService
    {
        public string FilePath { get; set; } = string.Empty;        
        /// <summary>
        /// Открывает диалог загрузки карт.
        /// </summary>
        /// <returns></returns>
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
            saveFile.FileName = device.DeviceName_DC;
            if (saveFile.ShowDialog() == true)//Открываем окно.
            {
                FilePath = saveFile.FileName;//Путь хранения файла.
                device?.SaveMapReg(FilePath);//Схраняем посредством xml-сериализации.
                return true;
            }
            return false;
        }
        /// <summary>
        /// Проверка и сохранение не сохраненных карт.
        /// </summary>
        public static void CheckAndSaveUnsavedMaps(List<(string,string)>listMaps)
        {
            Stack<string> stackNameMaps = new Stack<string>();//Стек хранение адресов временных карт.
            FileInfo file = new FileInfo("ModbusMonitor.exe");
            string dir = file.DirectoryName + FileNameMap.MapsTemp;
            DirectoryInfo directory = new DirectoryInfo(dir);
            if (!directory.Exists)//Если временных карт не создано - выход.
            {
                return;
            }
            FileInfo[] fileList = directory.GetFiles(); //Список файлов во временной папке.            
            foreach (var map in listMaps)//Загрузка в стек адресов временных карт.
            {
                if (map.Item1.Contains(FileNameMap.MapsTemp))
                {
                    stackNameMaps.Push(map.Item1);
                }
            }
            //Окно опроса сохранения временных карт.
            if (stackNameMaps.Count > 0 && MessageBox.Show("Сохранить карты?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                MessageBoxResult.Yes)
            {
                while (stackNameMaps.Count > 0)//Пока в стеке есть элементы.
                {
                    //Прописываем путь.
                    var fPaph = file.DirectoryName + FileNameMap.MapsOrigin + "\\" + fileList[stackNameMaps.Count - 1].Name;
                    File.Copy(stackNameMaps.Peek(), fPaph);//Копируем в основной каталог карт
                    File.Delete(stackNameMaps.Pop());//Удаляем из стека пути временных файлов.
                }
            }
            directory.Delete(true);//Удаляем директорию временных файлов.
            listMaps.Clear();
        }
        /// <summary>
        /// Проверка и сохранение не сохраненных карт.
        /// </summary>
        public static void CheckAndSaveUnsavedMaps(string pathFile)
        {            
            FileInfo file = new FileInfo("ModbusMonitor.exe");
            string dir = file.DirectoryName + FileNameMap.MapsTemp;
            DirectoryInfo directory = new DirectoryInfo(dir);
            if (!directory.Exists)//Если временных карт не создано - выход.
            {
                return;
            }
                FileInfo fileMap = new FileInfo(pathFile);
            if (MessageBox.Show("Сохранить карты?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var fPaph = file.DirectoryName + FileNameMap.MapsOrigin + "\\" + fileMap.Name;
                fileMap.CopyTo(fPaph);
            }
                fileMap.Delete();
            if (directory.GetFiles().Length == 0)
            {
                directory.Delete();
            }
        }
        public void ShowMessage(string message)
        {

        }
    }
}
