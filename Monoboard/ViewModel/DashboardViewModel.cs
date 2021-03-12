using MaterialDesignExtensions.Controls;
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Transitions;
using Monoboard.Helpers.Command;
using Monoboard.Helpers.Formatter;
using Monoboard.Helpers.Miscellaneous;
using Monoboard.Properties;
using Monoboard.View.Content.Card;
using Monoboard.ViewModel.StatementItemsViewModels;
using MonoboardCore.Hepler;
using MonoboardCore.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Monoboard.ViewModel
{
	public class DashboardViewModel : Helpers.ViewModel
	{
		#region Fields

		private ObservableCollection<TransitionerSlide> _cardList;
		private ObservableCollection<List<StatementItem>>? _statementItems;
		private ObservableCollection<CustomStatement> _statementItemsCollection;
		private int _selectedIndex;
		private bool _isBusy;

		public CancellationTokenSource CancellationToken = new CancellationTokenSource();
		private SnackbarMessageQueue _messages;

		public ObservableCollection<TransitionerSlide> CardList
		{
			get => _cardList;
			set
			{
				_cardList = value;
				OnPropertyChanged(nameof(CardList));
			}
		}

		public List<Account>? Cards { get; set; }

		public ObservableCollection<List<StatementItem>>? StatementItemList
		{
			get => _statementItems;
			set
			{
				_statementItems = value;
				OnPropertyChanged(nameof(StatementItemList));
			}
		}

		/// <summary>
		/// Дані виписки
		/// </summary>
		public ObservableCollection<CustomStatement> StatementItemsCollection
		{
			get => _statementItemsCollection;
			set
			{
				_statementItemsCollection = value;
				OnPropertyChanged(nameof(StatementItemsCollection));
			}
		}

		public int SelectedIndex
		{
			get => _selectedIndex;
			set
			{
				_selectedIndex = value;
				OnPropertyChanged(nameof(SelectedIndex));
			}
		}

		public bool IsBusy
		{
			get => _isBusy;
			set
			{
				_isBusy = value;
				OnPropertyChanged(nameof(IsBusy));
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

		private StatementInfoViewModel InfoViewModel { get; }

		#endregion

		#region Commands

		public ICommand GetUpdateDataCommand { get; set; }

		#endregion

		public DashboardViewModel()
		{
			_messages ??= new SnackbarMessageQueue();
			_cardList ??= new ObservableCollection<TransitionerSlide>();
			_statementItemsCollection ??= new ObservableCollection<CustomStatement>();
			_statementItems ??= new ObservableCollection<List<StatementItem>>();
			InfoViewModel = new StatementInfoViewModel();
			Cards = new List<Account>();

			GetUpdateDataCommand = new DelegateCommand(async o => await UpadteAsync());

			Task.Run(LoadUserInfoAsync);
			Task.Run(LoadStatementItemsAsync);
		}

		#region Methods

		/// <summary>
		/// Завантажує інформацію про картки/рахунки користувача
		/// </summary>
		private async Task LoadUserInfoAsync()
		{
			if (Cards!.Any()) Cards?.Clear();

			Cards = await MonoboardCore.Get.GetAccount.GetListAsync(Settings.Default.ClientId) as List<Account>;

			await Application.Current.Dispatcher.BeginInvoke((Action)delegate
			{
				if (CardList.Any()) CardList.Clear();

				foreach (var userAccount in Cards!)
					if (userAccount.MaskedPanList != null
						&& userAccount.MaskedPanList.Any())
						CardList.Add(new TransitionerSlide
						{
							Content = new BankCardTemplate(
								userAccount.BalanceFormat!,
								userAccount.CreditLimitFormat!,
								userAccount.MaskedPanList.ToArray(),
								userAccount.CustomCardName!,
								userAccount.CustomCardDescription!,
								userAccount.CardType,
								userAccount.Iban),
							OpeningEffect = new TransitionEffect(TransitionEffectKind.FadeIn)
						});
					else
						CardList.Add(new TransitionerSlide
						{
							Content = new BankCardTemplate(
								userAccount.BalanceFormat!,
								userAccount.CreditLimitFormat!,
								userAccount.MaskedPan,
								userAccount.CustomCardName!,
								userAccount.CustomCardDescription!,
								userAccount.CardType,
								userAccount.Iban),
							OpeningEffect = new TransitionEffect(TransitionEffectKind.FadeIn)
						});

				SelectedIndex = 0;
			});
		}

		/// <summary>
		/// Завантажує дані виписки з бази даних
		/// </summary>
		/// <returns></returns>
		private async Task LoadStatementItemsAsync()
		{
			if (StatementItemList!.Any())
				StatementItemList?.Clear();

			var accounts = await MonoboardCore.Get.GetAccount.GetListAsync(Settings.Default.ClientId);

			var firstDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month,
				1, 0, 0, 0).ToUniversalTime();

			var lastDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month,
				DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month),
				23, 59, 59).ToUniversalTime();

			foreach (var account in accounts)
				StatementItemList?.Add((await MonoboardCore.Get.GetStatementItems.GetStatementItemsList(account.CardCode,
					UnixConverter.DateTimeToUnixTimestamp(firstDate),
					UnixConverter.DateTimeToUnixTimestamp(lastDate)) as List<StatementItem>)!);
		}

		public async void UpdateData() => await LoadStatementItemsAsync();

		/// <summary>
		/// Виконує завантаження особистих даних та виписки за поточний місяць
		/// </summary>
		/// <returns></returns>
		private async Task UpadteAsync()
		{
			if (TimeWait() <= TimeSpan.Zero) App.InfoDelay = DateTime.Parse("01.01.21");

			if (App.InfoDelay == DateTime.Parse("01.01.21"))
			{
				var firstDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month,
					1, 0, 0, 0);

				var lastDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month,
					DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month),
					23, 59, 59).ToUniversalTime();

				await LoadingAsync(firstDate, lastDate);
			}
			else
			{
				Messages.Clear();

				Messages.Enqueue($"{App.GetResourceValue("MbGetDataWaitStart")}: {TimeWait():mm\\:ss}");
			}
		}

		/// <summary>
		/// Реалізація завантаження даних і інформування користувача
		/// </summary>
		/// <param name="firstDate">Дата початку "читання" виписки</param>
		/// <param name="lastDate">Дата кінця "читання" виписки</param>
		private async Task LoadingAsync(DateTime firstDate, DateTime lastDate)
		{
			if (Internet.IsConnectedToInternet() is false)
			{
				IsBusy = false;

				Messages.Clear();
				Messages.Enqueue(App.GetResourceValue("MbNoInternet")!);

				return;
			}

			var index = SelectedIndex;

			IsBusy = true;

			var (statementItems, info) = await MonoboardCore.Get.GetStatementItems.DownloadAsync(
				Settings.Default.MonobankToken, Cards![SelectedIndex].CardCode,
				UnixConverter.DateTimeToUnixTimestamp(firstDate.ToUniversalTime()).ToString(),
				UnixConverter.DateTimeToUnixTimestamp(lastDate).ToString());

			var (userInfo, getUserInfoMessage) = await MonoboardCore.Get.GetUserInfo.GetClientInfoAsync(Settings.Default.MonobankToken);

			StartDelay();

			if (userInfo != null)
			{
				var user = await MonoboardCore.Get.GetUserInfo.GetUserAsync(
					Settings.Default.ClientId,
					true,
					true);

				user.Accounts = AccountsDecorator.Merge(user.Accounts, userInfo.Accounts);

				user.MonoboardUser.ClientId = userInfo.ClientId;

				var isUserUpdated = await MonoboardCore.Set.SetUserInfo.UpdateUserAsync(user);

				if (isUserUpdated)
				{
					await LoadUserInfoAsync();
					await LoadStatementItemsAsync();

				}
				else
					Messages.Enqueue(App.GetResourceValue("MbSaveDataError")!);

			}
			else
			{
				Messages.Clear();

				switch (info)
				{
					case "MbNoInternet":
						Messages.Enqueue(App.GetResourceValue("MbNoInternet")!);
						break;
					default:
						Messages.Enqueue($"{App.GetResourceValue("MbSaveDataError")}" +
										 $"\n{App.GetResourceValue("MbCauseInfo")} {getUserInfoMessage}");
						break;
				}
			}

			if (statementItems != null)
			{
				statementItems =
					StatementItemsFormatter.Decorate(Cards[index].CurrencyCode, statementItems) as
						List<StatementItem>;

				statementItems = StatementItemsFormatter.MccDescriptor(statementItems!) as List<StatementItem>;

				var (isSaved, message) =
					await MonoboardCore.Set.SetStatementItems.SaveAsync(Cards[index].CardCode,
						statementItems!);

				if (isSaved)
				{
					foreach (var statementItem in statementItems!.Where(statementItem =>
						!StatementItemList![index]
							.Exists(item => item.Id == statementItem.Id)))
						StatementItemList![index].Add(statementItem);

					Messages.Enqueue(App.GetResourceValue(message)!);
				}
				else
				{
					IsBusy = false;

					Messages.Enqueue(App.GetResourceValue(message)!);
				}

				await ShowResult(StatementItemList![0], CancellationToken.Token);
			}
			else
			{
				Messages.Clear();

				switch (info)
				{
					case "MbNoInternet":
						Messages.Enqueue(App.GetResourceValue("MbNoInternet")!);
						break;
					default:
						Messages.Enqueue($"{App.GetResourceValue("MbSaveDataError")}" +
										 $"\n{App.GetResourceValue("MbCauseInfo")} {info}");
						break;
				}
			}

			IsBusy = false;
		}

		/// <summary>
		/// Виводить користувачу дані виписки
		/// </summary>
		/// <param name="statementItems">Дані виписки</param>
		/// <param name="token">Токен скасування операції</param>
		public async Task ShowResult(ICollection<StatementItem>? statementItems, CancellationToken token)
		{
			if (StatementItemsCollection.Any()) StatementItemsCollection.Clear();

			if (statementItems!.Any())
			{
				var dates = statementItems!
					.OrderByDescending(item => item.Time)
					.Select(item => item.FormatDate)
					.Distinct();

				var collections = new List<StatementItem>();
				var delay = 0;

				foreach (var date in dates)
				{
					if (token.IsCancellationRequested) continue;

					collections.Clear();

					collections.AddRange(statementItems!.Where(item => string.Equals(
						item.FormatDate,
						date, StringComparison.Ordinal)));

					collections = StatementItemsFormatter
						.MccDescriptor(collections)
						.OrderByDescending(item => item.Time)
						.ToList();

					var customView = new List<CustomView>();

					customView.AddRange(collections.Select(statementItem => new CustomView
					{
						Foreground = statementItem.AmountFormat.Contains("-")
							? new SolidColorBrush(Colors.Red)
							: new SolidColorBrush(Colors.Green),
						StatementItem = statementItem
					}));

					StatementItemsCollection.Add(new CustomStatement
					{
						Date = DateTime.Parse(date).ToString("D", Settings.Default.Language),
						StatementItemsView = customView
					});

					await Task.Delay(10, token);
					delay += 10;

					if (delay > 300) IsBusy = false;
				}

				if (IsBusy) IsBusy = false;
			}
			else
			{
				IsBusy = false;
			}
		}

		/// <summary>
		/// Відкриває діалогове вікно з детальною інформацією про вииску
		/// </summary>
		/// <param name="dialogHost">Діалогове вікно що відкриється</param>
		/// <param name="dataTemplate">Шаблон для відображення</param>
		/// <param name="statementItem">Дані виписки</param>
		/// <returns></returns>
		public async Task OpenConverter(DialogHost dialogHost, DataTemplate dataTemplate, StatementItem statementItem)
		{
			InfoViewModel.SetValue(statementItem);

			await AlertDialog.ShowDialogAsync(dialogHost, new AlertDialogArguments
			{
				Title = statementItem.Description,
				CustomContent = InfoViewModel,
				CustomContentTemplate = dataTemplate,
				OkButtonLabel = App.GetResourceValue("MbClose")
			});
		}

		/// <summary>
		/// Відображає час який залишився до повторної можливості отримання виписки
		/// </summary>
		/// <returns>Час який залишився</returns>
		public TimeSpan TimeWait() => App.InfoDelay - DateTime.Now;

		/// <summary>
		/// Запускає таймер для блокування від частих запитів до API Monobank
		/// </summary>
		private static void StartDelay() => App.InfoDelay = DateTime.Now.AddMinutes(2);

		#endregion
	}
}
