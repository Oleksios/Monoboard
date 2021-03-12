using MaterialDesignExtensions.Controls;
using MaterialDesignExtensions.Model;
using MaterialDesignThemes.Wpf;
using Monoboard.Helpers.Command;
using MonoboardCore.Hepler;
using MonoboardCore.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Monoboard.ViewModel.ExchangeRatesViewModels
{
	public class ExchangesViewModel : Helpers.ViewModel
	{
		#region Fields

		private bool _isBusy;
		private SnackbarMessageQueue _messages;
		private ISearchSuggestionsSource? _searchSuggestionsSource;
		private ObservableCollection<CurrencyVisualize> _otherCurrencies;
		private string _searshTerm;
		private bool _isOpenDialogHostConverter;

		private readonly ResourceDictionary _currencyDictionary;
		private readonly ResourceDictionary _currencyTagDictionary = Application.Current.Resources.MergedDictionaries.First(d =>
			d.Source != null && d.Source.OriginalString.StartsWith("/Resources/Localization/Currency/CurrencyTag"));

		private CurrencyConverterViewModel ConverterViewModel { get; }

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

		public ISearchSuggestionsSource? SearchSuggestionsSource
		{
			get => _searchSuggestionsSource;
			set
			{
				_searchSuggestionsSource = value;
				OnPropertyChanged(nameof(SearchSuggestionsSource));
			}
		}

		public ObservableCollection<CurrencyVisualize> MainCurrencies { get; set; }

		public ObservableCollection<CurrencyVisualize> OtherCurrencies
		{
			get => _otherCurrencies;
			set
			{
				_otherCurrencies = value;
				OnPropertyChanged(nameof(OtherCurrencies));
			}
		}

		public string SearshTerm
		{
			get => _searshTerm;
			set
			{
				_searshTerm = value;
				OnPropertyChanged(nameof(SearshTerm));
			}
		}

		public bool IsOpenDialogHostConverter
		{
			get => _isOpenDialogHostConverter;
			set
			{
				_isOpenDialogHostConverter = value;
				OnPropertyChanged(nameof(IsOpenDialogHostConverter));
			}
		}

		public ObservableCollection<CurrencyVisualize> OtherCurrenciesBase { get; set; }

		#endregion

		#region Commands
		public ICommand GetDataCommand { get; }
		public ICommand RefreshCommand { get; }

		#endregion

		public ExchangesViewModel()
		{
			_isBusy = true;
			Messages ??= new SnackbarMessageQueue();
			OtherCurrenciesBase ??= new ObservableCollection<CurrencyVisualize>();
			OtherCurrencies ??= new ObservableCollection<CurrencyVisualize>();
			MainCurrencies ??= new ObservableCollection<CurrencyVisualize>();
			ConverterViewModel ??= new CurrencyConverterViewModel();

			_currencyDictionary = App.Language.Name == "uk-UA"
				? Application.Current.Resources.MergedDictionaries.First(dictionary =>
					dictionary.Source != null &&
					dictionary.Source.OriginalString.StartsWith(
						"/Resources/Localization/Currency/CurrencyResources.xaml"))
				: Application.Current.Resources.MergedDictionaries.First(dictionary =>
					dictionary.Source != null &&
					dictionary.Source.OriginalString.StartsWith(
						$"/Resources/Localization/Currency/CurrencyResources.{App.Language.Name}.xaml"));

			GetDataCommand = new DelegateCommand(async o => await GetDataAsync());
			RefreshCommand = new DelegateCommand(o =>
			{
				if (OtherCurrenciesBase.Any()) OtherCurrencies = OtherCurrenciesBase;
			});

			GetInfo();
		}

		#region Methods

		private async void GetInfo() => await CurrencyListSplitter(MonoboardCore.Get.GetExchangeRates.GetInfo());

		private async Task CurrencyListSplitter(IEnumerable<ExchangeRates> exchangeRates)
		{
			IsBusy = true;

			if (OtherCurrencies.Any()) OtherCurrencies.Clear();
			if (MainCurrencies.Any()) MainCurrencies.Clear();

			foreach (var exchangeRate in exchangeRates)
			{
				if (exchangeRate.CurrencyCodeA != 985 &&
					exchangeRate.CurrencyCodeA != 978 &&
					exchangeRate.CurrencyCodeA != 643 &&
					exchangeRate.CurrencyCodeA != 840)
				{
					OtherCurrencies.Add(new CurrencyVisualize
					{
						RateCross = exchangeRate.RateCross,
						CurrencyNameLocalized = CurrencyNameLocalized(exchangeRate.CurrencyCodeA, _currencyDictionary),
						CurrencyNameLocalizedShort =
							CurrencyNameLocalizedShort(exchangeRate.CurrencyCodeA, _currencyTagDictionary),
						InfoDate = $"{App.GetResourceValue("MbAsOfDate")} {UnixConverter.UnixTimestampToDateTime(exchangeRate.Date).ToString(App.Language)}"
					});

					await Task.Delay(10);
				}
				else
				{
					if (exchangeRate.CurrencyCodeB != 840)
						MainCurrencies.Add(new CurrencyVisualize
						{
							RateBuy = exchangeRate.RateBuy,
							RateSell = exchangeRate.RateSell,
							RateCross = exchangeRate.RateCross,
							CurrencyNameLocalized =
								CurrencyNameLocalized(exchangeRate.CurrencyCodeA, _currencyDictionary),
							CurrencyNameLocalizedShort =
								CurrencyNameLocalizedShort(exchangeRate.CurrencyCodeA, _currencyTagDictionary),
							InfoDate =
								$"{App.GetResourceValue("MbAsOfDate")} " +
								$"{UnixConverter.UnixTimestampToDateTime(exchangeRate.Date).ToString(App.Language)}"
						});

					await Task.Delay(10);
				}
			}

			OtherCurrenciesBase = OtherCurrencies;

			SearchSuggestionsSource = new OtherCurrencySearch(OtherCurrencies);
			IsBusy = false;
		}

		/// <summary>
		/// Відкриває конвертер валют
		/// </summary>
		/// <param name="dialogHost">Діалогове вікно для виводу конвертера валют</param>
		/// <param name="dataTemplate">Контент конвертера валют</param>
		/// <param name="currencyName">Найменування валюти</param>
		/// <param name="currencyTag">Код валюти</param>
		/// <param name="rateCloss">Крос-курс</param>
		/// <returns></returns>
		public async Task OpenConverter(
			DialogHost dialogHost,
			DataTemplate dataTemplate,
			string currencyName,
			string currencyTag,
			float? rateCloss)
		{
			ConverterViewModel.SetValue(currencyName, currencyTag, rateCloss);

			await AlertDialog.ShowDialogAsync(dialogHost, new AlertDialogArguments
			{
				Title = App.GetResourceValue("MbCurrencyConverter"),
				CustomContent = ConverterViewModel,
				CustomContentTemplate = dataTemplate,
				OkButtonLabel = App.GetResourceValue("MbClose")
			});
		}

		/// <summary>
		/// Відкриває конвертер валют
		/// </summary>
		/// <param name="dialogHost">Діалогове вікно для виводу конвертера валют</param>
		/// <param name="dataTemplate">Контент конвертера валют</param>
		/// <param name="currencyName">Найменування валюти</param>
		/// <param name="currencyTag">Код валюти</param>
		/// <param name="rateBuy">Курс купівлі</param>
		/// <param name="rateSell">Курс продажу</param>
		/// <returns></returns>
		public async Task OpenConverter(
			DialogHost dialogHost,
			DataTemplate dataTemplate,
			string currencyName,
			string currencyTag,
			float? rateBuy,
			float? rateSell)
		{
			ConverterViewModel.SetValue(currencyName, currencyTag, rateBuy, rateSell);

			await AlertDialog.ShowDialogAsync(dialogHost, new AlertDialogArguments
			{
				Title = App.GetResourceValue("MbCurrencyConverter"),
				CustomContent = ConverterViewModel,
				CustomContentTemplate = dataTemplate,
				OkButtonLabel = App.GetResourceValue("MbClose")
			});
		}

		/// <summary>
		/// Завантажує дані про курси валют
		/// </summary>
		private async Task GetDataAsync()
		{
			if (Internet.IsConnectedToInternet() is false)
			{
				IsBusy = false;

				Messages.Clear();
				Messages.Enqueue(App.GetResourceValue("MbNoInternet")!);

				return;
			}

			if (TimeWait() <= TimeSpan.Zero) App.OtherDelay = DateTime.Parse("01.01.21");

			if (App.OtherDelay == DateTime.Parse("01.01.21"))
			{
				IsBusy = true;

				SearshTerm = string.Empty;

				var (exchangeRates, message) = await MonoboardCore.Get.GetExchangeRates.Downdload();

				StartDelay();

				Messages.Clear();

				if (exchangeRates != null)
				{
					var isSaved = await MonoboardCore.Set.SetExchangeRates.Save(exchangeRates);

					if (isSaved) GetInfo();

					Messages.Enqueue((isSaved
						? App.GetResourceValue("MbSaveDataSuccess")
						: App.GetResourceValue("MbSaveDataError"))!);
				}
				else
				{
					switch (message)
					{
						case "MbNoInternet":
							IsBusy = false;
							Messages.Enqueue(App.GetResourceValue("MbNoInternet")!);
							break;
						default:
							IsBusy = false;
							Messages.Enqueue(
								$"{App.GetResourceValue("MbSaveDataError")}" +
								$"\n{App.GetResourceValue("MbCauseInfo")} {message}");
							break;
					}
				}
			}
			else
			{
				Messages.Clear();

				Messages.Enqueue($"{App.GetResourceValue("MbGetDataWaitStart")}: {TimeWait():mm\\:ss}");
			}
		}

		/// <summary>
		/// Відображає час який залишився до повторної можливості отримання виписки
		/// </summary>
		/// <returns>Час який залишився</returns>
		public TimeSpan TimeWait() => App.OtherDelay - DateTime.Now;

		/// <summary>
		/// Запускає таймер для блокування від частих запитів до API Monobank
		/// </summary>
		private static void StartDelay() => App.OtherDelay = DateTime.Now.AddMinutes(5);

		private static string? CurrencyNameLocalized(int currencyCodeA, IDictionary currencyDictionary) =>
			currencyDictionary[$"CName{currencyCodeA:D3}"]?.ToString();

		private static string? CurrencyNameLocalizedShort(int currencyCodeA, IDictionary currencyTagDictionary) =>
			currencyTagDictionary[$"CTag{currencyCodeA:D3}"]?.ToString();

		#endregion
	}

	/// <summary>
	/// Дані валют (для подальшої візуалізації)
	/// </summary>
	public class CurrencyVisualize
	{
		public float? RateBuy { get; set; }
		public float? RateSell { get; set; }
		public float? RateCross { get; set; }
		public string? CurrencyNameLocalized { get; set; }
		public string? CurrencyNameLocalizedShort { get; set; }
		public string? InfoDate { get; set; }
	}

	/// <summary>
	/// Дані інших валют (для пошуку)
	/// </summary>
	public class OtherCurrencySearch : ISearchSuggestionsSource
	{
		private readonly List<string?> _currencies;

		public OtherCurrencySearch(IEnumerable<CurrencyVisualize> currencies) =>
			_currencies = currencies.Select(currency => currency.CurrencyNameLocalized)
				.ToList();

		public IList<string> GetAutoCompletion(string searchTerm) =>
			_currencies.Where(c => c.ToLower()
					.Contains(searchTerm.ToLower()))
				.ToList()!;

		public IList<string> GetSearchSuggestions() => _currencies!;
	}
}
