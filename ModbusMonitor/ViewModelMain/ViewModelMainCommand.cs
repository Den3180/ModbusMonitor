using ModbusMonitor.Classes;
using ModbusMonitor.Controls;
using ModbusMonitor.Windows;
using ModbusMonitor.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Threading;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Xml;

namespace ModbusMonitor
{
    public partial class ViewModelMain : INotifyPropertyChanged
    {
        private readonly Command aboutCommand;//О программе.
        private readonly Command clearLogErrCommand;//Очистить лог ошибок.
        private readonly Command colorTypeCommand;//Цвета для типов.
        private readonly Command connectionCommand;//Подключение устройства.
        private readonly Command createConnectCommand;//Настройка подключения.
        private readonly Command disconnectionCommand;//Отлючение.
        private readonly Command exitCommand;//Выход.
        private readonly Command formatCommand;//Формат.
        private readonly Command helpCommand;//Справка.
        private readonly Command listenPortCommand;//Слушать порт.
        private readonly Command loadMapCommand;//Загрузить карту.
        private readonly Command openLogCommand;//Открыть лог.
        private readonly Command openLogErrCommand;//Открыть лог ошибок.
        private readonly Command paramCommand;//Параметры.
        private readonly Command ratioCommand;//Коэффициенты.
        private readonly Command resetColCommand;//Сброс ширины столбцов.
        private readonly Command saveMapCommand;//Сохранить карту регистров. 
        private readonly Command sendRequestCommand;//Запрос данных.
        private readonly Command tableCommand;//Показать таблицу.
        private readonly Command textCommand;//Показать как текст.
        private readonly Command viewingPackagesCommand;//Паказать пакеты.
        private readonly Command writeRegisterCommand;//Записать регистр.
        private readonly Command searchAddressCommand;
        private readonly Command disablePollCommand;

        //Комманды.Вкладка "Файл".
        public ICommand LoadMapCommand => loadMapCommand;
        public ICommand SaveMapcomman => saveMapCommand;
        public ICommand OpenLogCommand => openLogCommand;
        public ICommand ExitCommand => exitCommand;
        //Комманды.Вкладка "Вид".
        public ICommand TableCommand => tableCommand;
        public ICommand TextCommand => textCommand;
        public ICommand ViewingPackagesCommand => viewingPackagesCommand;
        public ICommand RatioCommand => ratioCommand;
        public ICommand FormatCommand => formatCommand;
        public ICommand ColorTypeCommand => colorTypeCommand;
        public ICommand ResetColCommand => resetColCommand;
        //Команды.Вкладка "Инструменты".
        public ICommand WriteRegisterCommand => writeRegisterCommand;
        public ICommand SendRequestCommand => sendRequestCommand;
        public ICommand OpenLogErrCommand => openLogErrCommand;
        public ICommand ClearLogErrCommand => clearLogErrCommand;
        public ICommand ListenPortCommand => listenPortCommand;
        public ICommand SearchAddressCommand => searchAddressCommand;
        public ICommand DisablePollCommand => disablePollCommand;
        //Команды.Вкладка "Сервис".
        public ICommand ParamCommand => paramCommand;
        //Команды.Вкладка "Справка".
        public ICommand HelpCommand => helpCommand;
        public ICommand AboutCommand => aboutCommand;
        //Кнопки области "Подключения".
        public ICommand CreateConnectCommand => createConnectCommand;
        public ICommand ConnectionCommand => connectionCommand;
        public ICommand DisconnectionCommand => disconnectionCommand;
              
    }
}