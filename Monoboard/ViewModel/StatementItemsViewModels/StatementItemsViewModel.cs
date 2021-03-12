using MaterialDesignExtensions.Controls;
using MaterialDesignThemes.Wpf;
using Monoboard.Helpers.Command;
using Monoboard.Helpers.Formatter;
using Monoboard.Helpers.Miscellaneous;
using Monoboard.Properties;
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

namespace Monoboard.ViewModel.StatementItemsViewModels
{
	internal class StatementItemsViewModel : Helpers.ViewModel
	{
		#region Fields

		private ObservableCollection<Account> _cardList;
		private int _selectedIndex;
		private ObservableCollection<CustomStatement> _statementItemsCollection;
		private ObservableCollection<List<StatementItem>>? _statementItems;
		private SnackbarMessageQueue _messages;
		private bool _isBusy;
		private DateTime _dateSelected;
		private int _pageListIndex;
		private Visibility _previousPartButtonVisibility;
		private Visibility _nextPartButtonVisibility;
		private string? _downloadTextValue;

		public CancellationTokenSource CancellationToken = new CancellationTokenSource();
		private bool _isAdvancedControlling;

		private StatementInfoViewModel InfoViewModel { get; }

		public ObservableCollection<Account> CardList
		{
			get => _cardList;
			set
			{
				_cardList = value;
				OnPropertyChanged(nameof(CardList));
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

		public int PageListIndex
		{
			get => _pageListIndex;
			set
			{
				_pageListIndex = value;
				OnPropertyChanged(nameof(PageListIndex));
			}
		}

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

		public SnackbarMessageQueue Messages
		{
			get => _messages;
			set
			{
				_messages = value;
				OnPropertyChanged(nameof(Messages));
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

		public DateTime DateSelected
		{
			get => _dateSelected;
			set
			{
				_dateSelected = value;
				OnPropertyChanged(nameof(DateSelected));
			}
		}

		public string? DownloadTextValue
		{
			get => _downloadTextValue;
			set
			{
				_downloadTextValue = value;
				OnPropertyChanged(nameof(DownloadTextValue));
			}
		}

		public bool IsAdvancedControlling
		{
			get => _isAdvancedControlling;
			set
			{
				_isAdvancedControlling = value;
				OnPropertyChanged(nameof(IsAdvancedControlling));

				DownloadTextValue = value is false
					? App.GetResourceValue("MbDownloadThisMonth")
					: App.GetResourceValue("MbDownloadThisDate");
			}
		}

		public Visibility PreviousPartButtonVisibility
		{
			get => _previousPartButtonVisibility;
			set
			{
				_previousPartButtonVisibility = value;
				OnPropertyChanged(nameof(PreviousPartButtonVisibility));
			}
		}

		public Visibility NextPartButtonVisibility
		{
			get => _nextPartButtonVisibility;
			set
			{
				_nextPartButtonVisibility = value;
				OnPropertyChanged(nameof(NextPartButtonVisibility));
			}
		}

		#endregion

		#region Commands
		public ICommand GetDataCommand { get; }
		public ICommand DownloadAllCommand { get; }
		public ICommand GetPreviousMonthDataCommand { get; }
		public ICommand GetPreviousPartCommand { get; }
		public ICommand GetNextPartCommand { get; }

		#endregion

		public StatementItemsViewModel()
		{
			_messages ??= new SnackbarMessageQueue();
			_cardList ??= new ObservableCollection<Account>();
			_statementItemsCollection ??= new ObservableCollection<CustomStatement>();
			_statementItems ??= new ObservableCollection<List<StatementItem>>();
			InfoViewModel = new StatementInfoViewModel();
			SelectedIndex = 0;
			_dateSelected = DateTime.Today;
			_downloadTextValue = App.GetResourceValue("MbDownloadThisMonth");

			GetDataCommand = new DelegateCommand(async o => await GetDataAsync((bool)o));
			GetPreviousMonthDataCommand = new DelegateCommand(async o => await GetPreviousMonthDataAsync());
			DownloadAllCommand = new DelegateCommand(async o => await DownloadAllAsync());
			GetPreviousPartCommand = new DelegateCommand(async o =>
			{
				PageListIndex++;

				IsBusy = true;
				await ShowResult(
					StatementItemList?[SelectedIndex],
					PageListIndex,
					CancellationToken.Token);
			});
			GetNextPartCommand = new DelegateCommand(async o =>
			{
				PageListIndex--;

				IsBusy = true;
				await ShowResult(
					StatementItemList?[SelectedIndex],
					PageListIndex,
					CancellationToken.Token);
			});

			_isBusy = true;
		}

		#region Methods

		public async Task UpdateDataAsync()
		{
			await LoadCards();
			SelectedIndex = 0;
			await LoadItems();
		}

		public async Task LoadCards() =>
			await Application.Current.Dispatcher.BeginInvoke((Action)async delegate
			{
				if (CardList.Any()) CardList.Clear();

				var accounts = await MonoboardCore.Get.GetAccount.GetListAsync(Settings.Default.ClientId);
				foreach (var account in accounts) CardList.Add(account);
			});

		public async Task LoadItems() =>
			await Application.Current.Dispatcher.BeginInvoke((Action)async delegate
			{
				if (StatementItemList!.Any()) StatementItemList?.Clear();

				foreach (var account in CardList)
					StatementItemList?.Add(
						(await MonoboardCore.Get.GetStatementItems.GetStatementItemsList(account.CardCode) as
							List<StatementItem>)!);
			});

		/// <summary>
		/// Завантажує дані виписки за конкретною датою (місяць)
		/// </summary>
		private async Task GetDataAsync(bool isAdvancedControlling)
		{
			if (TimeWait() <= TimeSpan.Zero) App.InfoDelay = DateTime.Parse("01.01.21");

			if (App.InfoDelay == DateTime.Parse("01.01.21"))
			{
				DateTime firstDate;
				DateTime lastDate;

				var currentDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1, 0, 0, 0);

				if (isAdvancedControlling)
				{
					firstDate = new DateTime(DateSelected.Year, DateSelected.Month, 1, 0, 0, 0);

					lastDate = new DateTime(DateSelected.Year, DateSelected.Month,
						DateTime.DaysInMonth(DateSelected.Year, DateSelected.Month),
						23, 59, 59).ToUniversalTime();

					if (currentDate.Date != firstDate.Date && CardList[SelectedIndex].IsDownloaded)
					{
						Messages.Clear();

						Messages.Enqueue(App.GetResourceValue("MbStatementItemsDownloaded")!);

						return;
					}
				}
				else
				{
					firstDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month,
						1, 0, 0, 0);

					lastDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month,
						DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month),
						23, 59, 59).ToUniversalTime();
				}

				var isDownloadable = firstDate.Date == currentDate.Date || !await MonoboardCore.Get.GetStatementItemsChecker
					.CheckAsync(CardList[SelectedIndex].CardCode, firstDate);

				if (isDownloadable)
					await LoadingAsync(firstDate, lastDate, currentDate);
				else
				{
					Messages.Clear();

					Messages.Enqueue(App.GetResourceValue("MbDataAlreadyDownloaded")!);
				}
			}
			else
			{
				Messages.Clear();

				var startMessage = App.GetResourceValue("MbGetDataWaitStart");

				if (App.Language.Name == "uk-UA" || App.Language.Name == "ru-RU")
				{
					if (TimeWait().Seconds.ToString().Contains("11") ||
						TimeWait().Seconds.ToString().Contains("12") ||
						TimeWait().Seconds.ToString().Contains("13") ||
						TimeWait().Seconds.ToString().Contains("14"))
					{
						Messages.Enqueue(
							$"{startMessage} {TimeWait().Seconds} {App.GetResourceValue("MbGetDataWaitEnd")}");
					}
					else if (TimeWait().Seconds.ToString().EndsWith("1"))
					{
						Messages.Enqueue(
							$"{startMessage} {TimeWait().Seconds} {App.GetResourceValue("MbGetDataWaitEndAlt")}");
					}
					else if (TimeWait().Seconds.ToString().EndsWith("2") ||
							 TimeWait().Seconds.ToString().EndsWith("3") ||
							 TimeWait().Seconds.ToString().EndsWith("4"))
					{
						Messages.Enqueue(
							$"{startMessage} {TimeWait().Seconds} {App.GetResourceValue("MbGetDataWaitEndAlt2")}");
					}
					else
						Messages.Enqueue(
							$"{startMessage} {TimeWait().Seconds} {App.GetResourceValue("MbGetDataWaitEnd")}");
				}
				else
					Messages.Enqueue(
						$"{startMessage} {TimeWait().Seconds} {App.GetResourceValue("MbGetDataWaitEnd")}");
			}
		}

		/// <summary>
		/// Завантажує всі дані користувача за весь час
		/// </summary>
		private async Task DownloadAllAsync()
		{
			// Comming Soon
			Messages.Clear();

			Messages.Enqueue(App.GetResourceValue("MbCommingSoonFeature")!);
		}

		/// <summary>
		/// Завантажує дані за попередній місяць
		/// </summary>
		private async Task GetPreviousMonthDataAsync()
		{
			if (TimeWait() <= TimeSpan.Zero) App.InfoDelay = DateTime.Parse("01.01.21");

			if (App.InfoDelay == DateTime.Parse("01.01.21"))
			{
				var date = await MonoboardCore.Get.GetStatementItemsChecker.GetLastDate(CardList[SelectedIndex].CardCode);
				var currentDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1, 0, 0, 0);

				var firstDate = new DateTime(date.Year, date.Month, 1, 0, 0, 0);
				var lastDate = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 59, 59).ToUniversalTime();

				var isDownloadable = !await MonoboardCore.Get.GetStatementItemsChecker
										 .CheckAsync(CardList[SelectedIndex].CardCode, firstDate);

				if (currentDate.Date != firstDate.Date && isDownloadable is false && CardList[SelectedIndex].IsDownloaded)
				{
					Messages.Clear();

					Messages.Enqueue(App.GetResourceValue("MbStatementItemsDownloaded")!);

					return;
				}

				if (isDownloadable)
					await LoadingAsync(firstDate, lastDate, currentDate);
				else
				{
					IsBusy = false;

					Messages.Clear();

					Messages.Enqueue(App.GetResourceValue("MbDataAlreadyDownloaded")!);
				}
			}
			else
			{
				IsBusy = false;

				Messages.Clear();

				var startMessage = App.GetResourceValue("MbGetDataWaitStart");

				if (App.Language.Name == "uk-UA" || App.Language.Name == "ru-RU")
				{
					if (TimeWait().Seconds.ToString().Contains("11") ||
						TimeWait().Seconds.ToString().Contains("12") ||
						TimeWait().Seconds.ToString().Contains("13") ||
						TimeWait().Seconds.ToString().Contains("14"))
					{
						Messages.Enqueue(
							$"{startMessage} {TimeWait().Seconds} {App.GetResourceValue("MbGetDataWaitEnd")}");
					}
					else if (TimeWait().Seconds.ToString().EndsWith("1"))
					{
						Messages.Enqueue(
							$"{startMessage} {TimeWait().Seconds} {App.GetResourceValue("MbGetDataWaitEndAlt")}");
					}
					else if (TimeWait().Seconds.ToString().EndsWith("2") ||
							 TimeWait().Seconds.ToString().EndsWith("3") ||
							 TimeWait().Seconds.ToString().EndsWith("4"))
					{
						Messages.Enqueue(
							$"{startMessage} {TimeWait().Seconds} {App.GetResourceValue("MbGetDataWaitEndAlt2")}");
					}
					else
						Messages.Enqueue(
							$"{startMessage} {TimeWait().Seconds} {App.GetResourceValue("MbGetDataWaitEnd")}");
				}
				else
					Messages.Enqueue(
						$"{startMessage} {TimeWait().Seconds} {App.GetResourceValue("MbGetDataWaitEnd")}");
			}
		}

		/// <summary>
		/// Реалізація завантаження даних і інформування користувача
		/// </summary>
		/// <param name="firstDate">Дата початку "читання" виписки</param>
		/// <param name="lastDate">Дата кінця "читання" виписки</param>
		/// <param name="currentDate">Теперішня дата</param>
		private async Task LoadingAsync(DateTime firstDate, DateTime lastDate, DateTime currentDate)
		{
			if (Internet.IsConnectedToInternet() is false)
			{
				IsBusy = false;

				Messages.Clear();
				Messages.Enqueue(App.GetResourceValue("MbNoInternet")!);

				return;
			}

			IsBusy = true;

			var (statementItems, info) = await MonoboardCore.Get.GetStatementItems.DownloadAsync(
				Settings.Default.MonobankToken, CardList[SelectedIndex].CardCode,
				UnixConverter.DateTimeToUnixTimestamp(firstDate.ToUniversalTime()).ToString(),
				UnixConverter.DateTimeToUnixTimestamp(lastDate).ToString());

			StartDelay();

			if (statementItems != null)
			{
				statementItems =
					StatementItemsFormatter.Decorate(CardList[SelectedIndex].CurrencyCode, statementItems) as
						List<StatementItem>;

				statementItems = StatementItemsFormatter.MccDescriptor(statementItems!) as List<StatementItem>;

				var (isSaved, message) =
					await MonoboardCore.Set.SetStatementItems.SaveAsync(CardList[SelectedIndex].CardCode,
						statementItems!);

				if (isSaved)
				{
					foreach (var statementItem in statementItems!.Where(statementItem =>
						!StatementItemList![SelectedIndex]
							.Exists(item => item.Id == statementItem.Id)))
						StatementItemList![SelectedIndex].Add(statementItem);

					if (firstDate.Date != currentDate.Date)
						await MonoboardCore.Set.SetStatementItemsChecker.CheckSaveAsync(
							CardList[SelectedIndex].CardCode, firstDate, false);

					StatementItemsCollection.Clear();

					await ShowResult(StatementItemList![SelectedIndex], PageListIndex, CancellationToken.Token);

					Messages.Enqueue(App.GetResourceValue(message)!);
				}
				else
				{
					IsBusy = false;

					Messages.Clear();

					Messages.Enqueue(App.GetResourceValue(message)!);
				}
			}
			else
			{
				IsBusy = false;

				Messages.Clear();

				switch (info)
				{
					case "MbNoInternet":
						Messages.Enqueue(App.GetResourceValue("MbNoInternet")!);
						break;
					case "MbDataBeforeRegister":
						{
							var isAllDownloaded = await MonoboardCore.Set.SetAccount.SetStatementsDownloadedAsync(Settings.Default.ClientId,
									CardList[SelectedIndex].CardCode);

							Messages.Enqueue((isAllDownloaded
								? App.GetResourceValue("MbDataBeforeRegister")
								: App.GetResourceValue("MbSaveDataError"))!);

							if (firstDate.Date != currentDate.Date && isAllDownloaded)
								await MonoboardCore.Set.SetStatementItemsChecker.CheckSaveAsync(
									CardList[SelectedIndex].CardCode, firstDate, true);
						}
						break;
					default:
						{
							Messages.Enqueue($"{App.GetResourceValue("MbSaveDataError")}" +
											 $"\n{App.GetResourceValue("MbCauseInfo")} {info}");

							if (firstDate.Date != currentDate.Date)
								await MonoboardCore.Set.SetStatementItemsChecker.CheckSaveAsync(
									CardList[SelectedIndex].CardCode, firstDate, false);
						}
						break;
				}
			}
		}

		/// <summary>
		/// Виводить користувачу дані виписки
		/// </summary>
		/// <param name="statementItems">Дані виписки</param>
		/// <param name="list">Позиція сторінки пошуку</param>
		/// <param name="token">Токен скасування операції</param>
		public async Task ShowResult(List<StatementItem>? statementItems, int list, CancellationToken token)
		{
			if (StatementItemsCollection.Any()) StatementItemsCollection.Clear();

			if (statementItems!.Any())
			{
				var dates = statementItems!
					.OrderByDescending(item => item.Time)
					.Select(item => item.FormatDate)
					.Distinct();

				var offset = list == 0 ? 0 : list * 50;

				var selectedDates = dates.Skip(offset).Take(50);

				if (statementItems.Count < 50)
				{
					PreviousPartButtonVisibility = Visibility.Collapsed;
					NextPartButtonVisibility = Visibility.Collapsed;
				}
				else
				{
					if (list == 0)
						if (selectedDates.Count() < 50)
						{
							PreviousPartButtonVisibility = Visibility.Collapsed;
							NextPartButtonVisibility = Visibility.Collapsed;
						}
						else
						{
							PreviousPartButtonVisibility = Visibility.Visible;
							NextPartButtonVisibility = Visibility.Collapsed;
						}
					else
					{
						if (selectedDates.Count() < 50)
						{
							PreviousPartButtonVisibility = Visibility.Collapsed;
							NextPartButtonVisibility = Visibility.Visible;
						}
						else
						{
							PreviousPartButtonVisibility = Visibility.Visible;
							NextPartButtonVisibility = Visibility.Visible;
						}
					}
				}

				var collections = new List<StatementItem>();
				var delay = 0;

				foreach (var date in selectedDates)
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

					if (delay > 500) IsBusy = false;

				}

				if (IsBusy) IsBusy = false;
			}
			else
			{
				PreviousPartButtonVisibility = Visibility.Collapsed;
				NextPartButtonVisibility = Visibility.Collapsed;

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
		private static void StartDelay() => App.InfoDelay = DateTime.Now.AddMinutes(1);

		#endregion
	}
}
