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
        private bool canWriteRegister;
        private bool canConnection;
        private bool canCreateConnect=true;
        private bool canDisconnection;
        private bool canOpenLog;
        private bool canRequest;
        private bool canDisablePoll;
        private bool treeViewEnabled;
        private bool canClearTreeAll;
        private bool canClearTreeSingle;
        private bool canRefreshTree;
        private bool canChangeDevice;

        #region [Флаги доступности]

        public bool CanChangeDevice
        {
            get => canChangeDevice;
            set=> SetOptions(nameof(CanChangeDevice), ref canChangeDevice, value);
        }
        /// <summary>
        /// Доступность команды Удалить все(Дерево).
        /// </summary>
        public bool CanRefreshTree
        {
            get => canRefreshTree;
            set
            {
                SetOptions(nameof(CanRefreshTree), ref canRefreshTree, value);
            }
        }

        /// <summary>
        /// Доступность команды Удалить все(Дерево).
        /// </summary>
        public bool CanClearTreeAll
        {
            get => canClearTreeAll;
            set
            {
                SetOptions(nameof(CanClearTreeAll), ref canClearTreeAll, value);
            }
        }
        /// <summary>
        /// Доступность команды Удалить(Дерево).
        /// </summary>
        public bool CanClearTreeSingle
        {
            get => canClearTreeSingle;
            set
            {
                SetOptions(nameof(CanClearTreeSingle), ref canClearTreeSingle, value);
            }
        }

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

        /// <summary>
        /// Изменение доступности команд.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewModelMain_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == null)
            {
                return;
            }
            if (e.PropertyName.Equals(nameof(CanDisablePoll)))
            {
                disablePollCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName.Equals(nameof(CanOpenLog)))
            {
                openLogCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName.Equals(nameof(CanCreateConnect)))
            {
                createConnectCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName.Equals(nameof(CanConnection)))
            {
                connectionCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName.Equals(nameof(CanDisconnection)))
            {
                disconnectionCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName.Equals(nameof(CanRequest)))
            {
                listenPortCommand.RaiseCanExecuteChanged();
                sendRequestCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName.Equals(nameof(CanWriteRegister)))
            {
                writeRegisterCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName.Equals(nameof(CanClearTreeSingle)))
            {
                clearTreeSingleCommand.RaiseCanExecuteChanged();
                clearTreeAllCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName.Equals(nameof(CanRefreshTree)))
            {
                refreshTreeCommand.RaiseCanExecuteChanged();
            }
            if (e.PropertyName.Equals(nameof(CanChangeDevice)))                
            {
                changeDeviceCommand.RaiseCanExecuteChanged();
            }
        }
    }
}