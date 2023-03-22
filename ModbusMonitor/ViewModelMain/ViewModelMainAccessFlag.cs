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

namespace ModbusMonitor
{
    public partial class ViewModelMain : INotifyPropertyChanged
    {
        #region [Флаги доступности]

        /// <summary>
        /// Доступность записи в регистр.
        /// </summary>
        public bool CanWriteRegister
        {
            get => canWriteRegister;
            set
            {
                SetOptions(nameof(CanWriteRegister), ref canWriteRegister, value);
            }
        }
        /// <summary>
        /// Доступность опроса порта.
        /// </summary>
        public bool CanDisablePoll
        {
            get => canDisablePoll;
            set => SetOptions(nameof(CanDisablePoll), ref canDisablePoll, value);
        }
        /// <summary>
        /// Доступность лога. 
        /// </summary>
        public bool CanOpenLog
        {
            get => canOpenLog;
            set => SetOptions(nameof(CanOpenLog), ref canOpenLog, value);
        }
        /// <summary>
        /// Доступность кнопки "Создать". 
        /// </summary>
        public bool CanCreateConnect
        {
            get => canCreateConnect;
            set => SetOptions(nameof(CanCreateConnect), ref canCreateConnect, value);
        }
        /// <summary>
        /// Доступность кнопки "Подключение". 
        /// </summary>
        public bool CanConnection
        {
            get => canConnection;
            set => SetOptions(nameof(CanConnection), ref canConnection, value);
        }
        /// <summary>
        /// Доступность кнопки "Отключение". 
        /// </summary>
        public bool CanDisconnection
        {
            get => canConnection;
            set => SetOptions(nameof(CanDisconnection), ref canDisconnection, value);
        }
        /// <summary>
        /// Доступность "Запрос данных","Слушать порт".
        /// </summary>
        public bool CanRequest
        {
            get => canRequest;
            set => SetOptions(nameof(CanRequest), ref canRequest, value);
        }
        #endregion
    }
}