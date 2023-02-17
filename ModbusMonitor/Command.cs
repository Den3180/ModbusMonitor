using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModbusMonitor
{
    internal class Command : ICommand
    {
        private static readonly Func<bool> defaultCanExecuteMethod = () => true;

        private readonly Action executeMethod;
        private readonly Func<bool> canExecuteMethod;

        public Command(Action executeMethod) :
           this(executeMethod, defaultCanExecuteMethod)
        {
        }
        public Command(Action executeMethod, Func<bool> canExecuteMethod)
        {
            this.canExecuteMethod = canExecuteMethod;
            this.executeMethod = executeMethod;
        }
        public bool CanExecute(object parameter)
        {
            return canExecuteMethod();
        }

        public void Execute(object parameter)
        {
            executeMethod();
        }
        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
