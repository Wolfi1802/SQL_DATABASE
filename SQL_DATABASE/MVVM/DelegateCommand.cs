using System;
using System.Diagnostics;
using System.Windows.Input;

namespace SQL_DATABASE.MVVM
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _action;

        public DelegateCommand(Action action)
        {
            _action = action;
        }

        public void Execute(object parameter)
        {
            try
            {
                _action();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(nameof(DelegateCommand), nameof(Execute), ex);
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

#pragma warning disable 67
        public event EventHandler CanExecuteChanged;
#pragma warning restore 67
    }
}
