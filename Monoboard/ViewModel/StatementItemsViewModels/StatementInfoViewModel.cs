using MaterialDesignThemes.Wpf;
using Monoboard.Helpers.Command;
using MonoboardCore.Model;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Monoboard.ViewModel.StatementItemsViewModels
{
	internal class StatementInfoViewModel : Helpers.ViewModel
	{
		#region Fields

		private StatementItem _statementItem;
		private Brush _foreground;
		private SnackbarMessageQueue _messages;
		private bool _isHyperlinkEnabled = true;
		private Visibility _operationAmountFormatVisibility;

		public StatementItem StatementItem
		{
			get => _statementItem;
			set
			{
				_statementItem = value;
				OnPropertyChanged(nameof(StatementItem));
			}
		}

		public Brush Foreground
		{
			get => _foreground;
			set
			{
				_foreground = value;
				OnPropertyChanged(nameof(Foreground));
			}
		}

		public SnackbarMessageQueue Messages
		{
			get => _messages;
			set
			{
				_messages = value;
				OnPropertyChanged(nameof(Messages));
			}
		}

		public bool IsHyperlinkEnabled
		{
			get => _isHyperlinkEnabled;
			set
			{
				_isHyperlinkEnabled = value;
				OnPropertyChanged(nameof(IsHyperlinkEnabled));
			}
		}

		public Visibility OperationAmountFormatVisibility
		{
			get => _operationAmountFormatVisibility;
			set
			{
				_operationAmountFormatVisibility = value;
				OnPropertyChanged(nameof(OperationAmountFormatVisibility));
			}
		}

		#endregion

		#region Command

		public ICommand GetReceiptCommand => new DelegateCommand(async o => await GetReceipt((string)o));

		#endregion

		public StatementInfoViewModel() => Messages ??= new SnackbarMessageQueue();

		#region Methods

		/// <summary>
		/// Встановлює необхідні значення для їх подальшого відображення
		/// </summary>
		/// <param name="statementItem">Основні значення</param>
		public void SetValue(StatementItem statementItem)
		{
			StatementItem = statementItem;

			Foreground = statementItem.AmountFormat.Contains("-")
				? new SolidColorBrush(Colors.Red)
				: new SolidColorBrush(Colors.Green);

			OperationAmountFormatVisibility =
				statementItem.OperationAmount == statementItem.Amount
				? Visibility.Collapsed
				: Visibility.Visible;
		}

		/// <summary>
		/// Копіювання коду квитанції і відкриття сервісу для перевірки квитанції
		/// </summary>
		/// <param name="receipt">Код квитанції</param>
		private async Task GetReceipt(string receipt)
		{
			IsHyperlinkEnabled = false;

			Clipboard.SetText(receipt);

			Messages.Clear();

			Messages.Enqueue(App.GetResourceValue("MbStatementReceiptCopied")!);

			await Task.Delay(2000);

			IsHyperlinkEnabled = true;

			Process.Start(new ProcessStartInfo("https://check.gov.ua/")
			{
				UseShellExecute = true
			});
		}

		#endregion
	}
}
