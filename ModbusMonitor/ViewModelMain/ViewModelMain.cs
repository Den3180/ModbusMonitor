using ModbusMonitor.Classes;
using ModbusMonitor.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace ModbusMonitor
{
    public partial class ViewModelMain : INotifyPropertyChanged
    {
        private readonly DispatcherTimer timerPoll;

        public ViewModelMain()
        {
            exitCommand = new Command(ExitApp);
            openLogCommand = new Command(OpenLog, () => CanOpenLog);
            loadMapCommand = new Command(LoadMap);
            saveMapCommand = new Command(SaveMap);
            tableCommand = new Command(MakeTable);
            textCommand = new Command(MakeText);
            viewingPackagesCommand = new Command(ViewingPackages);
            listenPortCommand = new Command(ListenPort, () => CanRequest);
            ratioCommand = new Command(EditRatio);
            formatCommand = new Command(EditFormat);
            colorTypeCommand = new Command(EditColorType);
            resetColCommand = new Command(ResetColumn);
            writeRegisterCommand = new Command(WriteRegister, () => CanWriteRegister);
            sendRequestCommand = new Command(SendRequest, () => CanRequest);
            openLogErrCommand = new Command(OpenLogError);
            clearLogErrCommand = new Command(ClearLogError);
            paramCommand = new Command(ShowParam);
            helpCommand = new Command(ShowHelp);
            aboutCommand = new Command(ShowAbout);
            createConnectCommand = new Command(CreateConnect, () => CanCreateConnect);
            connectionCommand = new Command(Connection, () => canConnection);
            disconnectionCommand = new Command(Disconnection, () => canDisconnection);
            searchAddressCommand = new Command(SearchAddress);
            disablePollCommand = new Command(DisablePoll, () => CanDisablePoll);
            clearTreeSingleCommand = new Command(ClearTreeSingle,()=>CanClearTreeSingle);
            clearTreeAllCommand = new Command(ClearTreeAll,()=>CanClearTreeAll);
            
            TreeViewEnabled = true;
            modbusRTU = new ModbusRTUASCII();
            listMaps = new List<(string, string)>();
            userControl = new UserControlDevices(new List<CellData>(), modbusRTU);
            device = new DeviceClass();
            listDevices = new List<DeviceClass>();//Список устройств на линии.
            treeNodes = new ObservableCollection<GroupsTreeNode>();//Источник данных дерева.            
            timerPoll = new DispatcherTimer();
            timerPoll.Interval = TimeSpan.FromMilliseconds(1000);
            
            PropertyChanged += ViewModelMain_PropertyChanged;
            timerPoll.Tick += TimerSec_Tick;
            CheckStartParam();
            Task.Run(() => modbusRTU.SendResponsePort(ModbusRTUASCII.SettingPortStart));
        }        
    }
}
