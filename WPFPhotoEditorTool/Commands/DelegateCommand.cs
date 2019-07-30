using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPFPhotoEditorTool.Commands
{
    public class DelegateCommand<TArgs> : ICommand
    {
        public DelegateCommand(Action<TArgs> exDelegate)
        {
            _exDelegate = exDelegate;
        }

        public DelegateCommand(Action<TArgs> exDelegate, Func<TArgs, bool> canDelegate)
        {
            _exDelegate = exDelegate;
            _canDelegate = canDelegate;
        }

        protected Action<TArgs> _exDelegate;
        protected Func<TArgs, bool> _canDelegate;

        #region ICommand Members

        public bool CanExecute(TArgs parameter)
        {
            if (_canDelegate == null)
                return true;

            return _canDelegate(parameter);
        }

        public void Execute(TArgs parameter)
        {
            if (_exDelegate != null)
            {
                _exDelegate(parameter);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (parameter != null)
            {
                var parameterType = parameter.GetType();
                if (parameterType.FullName.Equals("MS.Internal.NamedObject"))
                    return false;
            }

            return CanExecute((TArgs)parameter);
        }

        void ICommand.Execute(object parameter)
        {
            Execute((TArgs)parameter);
        }

        #endregion
    }

    public class DelegateCommand : ICommand
    {
        public DelegateCommand(Action exDelegate)
        {
            _exDelegate = exDelegate;
        }

        public DelegateCommand(Action exDelegate, Func<bool> canDelegate)
        {
            _exDelegate = exDelegate;
            _canDelegate = canDelegate;
        }

        protected Action _exDelegate;
        protected Func<bool> _canDelegate;

        #region ICommand Members

        public bool CanExecute()
        {
            if (_canDelegate == null)
                return true;

            return _canDelegate();
        }

        public void Execute()
        {
            if (_exDelegate != null)
            {
                _exDelegate();
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (parameter != null)
            {
                var parameterType = parameter.GetType();
                if (parameterType.FullName.Equals("MS.Internal.NamedObject"))
                    return false;
            }

            return CanExecute();
        }

        void ICommand.Execute(object parameter)
        {
            Execute();
        }

        #endregion
    }
}
