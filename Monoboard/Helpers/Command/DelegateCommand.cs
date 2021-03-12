using System;
using System.Windows.Input;

namespace Monoboard.Helpers.Command
{
	public class DelegateCommand : ICommand
	{
		#region Fields

		private readonly Action<object> _execute;

		private readonly Predicate<object> _canExecute;

		#endregion

		#region Constructors

		public DelegateCommand(Action<object> execute) : this(execute, null!) { }

		public DelegateCommand(Action<object> execute, Predicate<object> canExecute)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		#endregion

		#region ICommand members

		public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter!) ?? true;

		public void Execute(object? parameter) => _execute(parameter!);

		public event EventHandler? CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;

			remove => CommandManager.RequerySuggested -= value;
		}

		#endregion
	}
}
