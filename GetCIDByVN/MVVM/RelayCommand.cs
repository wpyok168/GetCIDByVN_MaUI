using System.Windows.Input;

namespace MVVM
{
    public class RelayCommand : ICommand
    {
        public Action<object> ExecuteAction { get; set; }
        public Func<bool> CanExecuteAction { get; set; }


        public RelayCommand(Action<object> executeAction) : this(executeAction, () => true)
        {
            ExecuteAction = executeAction;
        }

        public RelayCommand(Action<object> executeAction, Func<bool> canexecuteAction)
        {
            ExecuteAction = executeAction;
            CanExecuteAction = canexecuteAction;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return CanExecuteAction();
        }

        public void Execute(object parameter)
        {
            ExecuteAction?.Invoke(parameter);
        }

        //public void Execute(object parameter)
        //{
        //    if (ExecuteAction == null)
        //    {
        //        return;
        //    }
        //    this.ExecuteAction(parameter);
        //}
    }
    public class RelayCommand<T> : ICommand
    {
        public RelayCommand(Action<T> executeAction) : this(executeAction, (T o) => true)
        {
            ExecuteAction = executeAction;
        }

        public RelayCommand(Action<T> executeAction, Func<T, bool> canExecute)
        {
            ExecuteAction = executeAction;
            CanExecuteFunc = canExecute;
        }

        public event EventHandler CanExecuteChanged;
        public Func<T, bool> CanExecuteFunc { get; set; }
        public Action<T> ExecuteAction { get; set; }

        public bool CanExecute(object parameter)
        {
            if (CanExecuteFunc == null)
            {
                return true;
            }
            return this.CanExecuteFunc((T)parameter);
        }


        public void Execute(object parameter)
        {
            ExecuteAction?.Invoke((T)parameter);
        }

        //public void RaiseCanExecuteChanged()
        //{
        //    if (CanExecuteChanged != null)
        //    {
        //        CanExecuteChanged(this, EventArgs.Empty);
        //    }
        //}

    }
}
