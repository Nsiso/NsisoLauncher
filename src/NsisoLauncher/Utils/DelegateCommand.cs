using System;
using System.Windows.Input;

namespace NsisoLauncher.Utils
{
    public class DelegateCommand : ICommand
    {
        private readonly Func<object, bool> canExecute;
        private bool canExecuteCache;
        private readonly Action<object> executeAction;

        public DelegateCommand(Action<object> executeAction) : this(executeAction, a => true)
        {
        }

        public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecute)
        {
            this.executeAction = executeAction;
            this.canExecute = canExecute;
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            var temp = canExecute(parameter);

            if (canExecuteCache != temp)
            {
                canExecuteCache = temp;
                CanExecuteChanged?.Invoke(this, new EventArgs());
            }

            return canExecuteCache;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            executeAction(parameter);
        }

        #endregion
    }
}