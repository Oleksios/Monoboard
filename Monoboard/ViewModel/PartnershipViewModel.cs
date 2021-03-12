using MaterialDesignExtensions.Model;
using MaterialDesignThemes.Wpf;
using Monoboard.Helpers.Command;
using MonoboardCore.Get;
using MonoboardCore.Hepler;
using MonoboardCore.Model;
using MonoboardCore.Set;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Monoboard.Helpers.Miscellaneous;

namespace Monoboard.ViewModel
{
	public class PartnershipViewModel : Helpers.ViewModel
	{
		#region Fields

		private ObservableCollection<PartnerView> _partners;
		private bool _isBusy;
		private SnackbarMessageQueue _messages;
		private string _searshTerm;
		private ISearchSuggestionsSource? _searchSuggestionsSource;
		private bool _isLoading;
		private int _loadingProgress;
		private int _maxLoadingProgress;

		public ObservableCollection<PartnerView> Partners
		{
			get => _partners;
			set
			{
				_partners = value;
				OnPropertyChanged(nameof(Partners));
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

		public bool IsLoading
		{
			get => _isLoading;
			set
			{
				_isLoading = value;
				OnPropertyChanged(nameof(IsLoading));
			}
		}

		public int LoadingProgress
		{
			get => _loadingProgress;
			set
			{
				_loadingProgress = value;
				OnPropertyChanged(nameof(LoadingProgress));
			}
		}

		public int MaxLoadingProgress
		{
			get => _maxLoadingProgress;
			set
			{
				_maxLoadingProgress = value;
				OnPropertyChanged(nameof(MaxLoadingProgress));
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
		public string SearshTerm
		{
			get => _searshTerm;
			set
			{
				_searshTerm = value;
				OnPropertyChanged(nameof(SearshTerm));
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

		public List<Partner> PartnersList { get; set; }
		public CancellationTokenSource CancellationToken = new CancellationTokenSource();

		#endregion

		#region Commands

		public ICommand RefreshSearchCommand { get; set; }
		public ICommand DownloadPartnersInfoCommand { get; set; }
		public ICommand GoToPartnerWebSiteCommand { get; set; }

		#endregion

		public PartnershipViewModel()
		{
			IsLoading = false;
			_messages = new SnackbarMessageQueue();
			_partners ??= new ObservableCollection<PartnerView>();

			RefreshSearchCommand = new DelegateCommand(async o => await Refresh());
			DownloadPartnersInfoCommand = new DelegateCommand(async o => await ReloadAsync());
			GoToPartnerWebSiteCommand = new DelegateCommand(async o => await GoToWebSite((string)o));

			Task.Run(async () => await GetInfo());
		}

		private async Task GoToWebSite(string url)
		{
			Messages.Clear();

			Messages.Enqueue(App.GetResourceValue("MbPartnerOpenBrowser")!);

			await Task.Delay(1500);

			Process.Start(new ProcessStartInfo(url)
			{
				UseShellExecute = true
			});
		}

		#region Methods

		/// <summary>
		/// Стирає дані пошуку та повертає стандартний вигляд
		/// </summary>
		private async Task Refresh()
		{
			SearshTerm = string.Empty;

			if (PartnersList.Count != Partners.Count)
			{
				Partners.Clear();
				await Show(PartnersList, CancellationToken.Token);
			}
		}

		/// <summary>
		/// Очищує всі дані для подальшого завантаження даних
		/// </summary>
		private async Task ReloadAsync()
		{
			if (Internet.IsConnectedToInternet() is false)
			{

				Messages.Clear();
				Messages.Enqueue(App.GetResourceValue("MbNoInternet")!);

				return;
			}

			if (await SetPartners.DeleteAsync())
			{
				SearshTerm = string.Empty;

				if (PartnersList != null && PartnersList.Any())
					PartnersList.Clear();

				if (Partners != null && Partners.Any())
					Partners.Clear();

				await GetInfo();
			}
			else
			{
				Messages.Clear();

				Messages.Enqueue(App.GetResourceValue("MbDeleteDataError")!);
			}
		}

		/// <summary>
		/// Виконує операцію завантаження даних про Партнерів Монобанку (Покупка частинами)
		/// </summary>
		/// <returns></returns>
		private async Task GetInfo()
		{
			if (Internet.IsConnectedToInternet() is false)
			{
				IsBusy = false;

				Messages.Clear();
				Messages.Enqueue(App.GetResourceValue("MbNoInternet")!);

				return;
			}

			PartnersList = await GetPartners.GetListAsync();

			if (PartnersList == null || PartnersList.Any() is false)
				await Application.Current.Dispatcher.BeginInvoke((Action)async delegate
				{
					IsBusy = IsLoading = true;

					var (partnerList, message) = await GetPartners.GetInfoAsync();

					if (partnerList != null && partnerList.Any())
					{
						MaxLoadingProgress = partnerList.Count;

						var isPartialError = false;

						foreach (var partner in partnerList.OrderBy(partner => partner.Title))
						{
							var base64 = await ImageManipulation.GetImageAsBase64Url(partner.LogoUrl);

							if (base64 == "")
							{
								isPartialError = true;

								var bitmapImage =
									new BitmapImage(new Uri(
										"pack://application:,,,/Monoboard;component/Resources/Images/NoLogo.jpg"));

								base64 = ImageManipulation.BitmapToBase64(bitmapImage);
							}

							partner.Logo = base64;

							var binaryData = Convert.FromBase64String(partner.Logo);

							PartnersList!.Add(partner);

							Partners.Add(new PartnerView
							{
								Logo = new ImageBrush(ImageManipulation.GetBitmapImage(binaryData)),
								Partner = partner
							});

							LoadingProgress++;

							await Task.Delay(10);
						}

						await SetPartners.SaveAsync(partnerList);
						SearchSuggestionsSource = new PartnerSearch(PartnersList!);

						MaxLoadingProgress = 0;
						LoadingProgress = 0;

						if (isPartialError)
						{
							Messages.Clear();
							Messages.Enqueue(App.GetResourceValue("MbNoInternetPartialError")!);
						}
					}
					else
					{
						Messages.Clear();

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

					IsLoading = false;
					IsBusy = false;
				});
			else
			{
				PartnersList = PartnersList.OrderBy(partner => partner.Title).ToList();
				SearchSuggestionsSource = new PartnerSearch(PartnersList);
			}
		}

		public async Task Show(List<Partner>? partners, CancellationToken token)
		{
			await Application.Current.Dispatcher.BeginInvoke((Action)async delegate
			{
				MaxLoadingProgress = partners.Count;
				if (IsBusy is false) IsBusy = true;
				IsLoading = true;

				foreach (var partner in partners!.OrderBy(partner => partner.Title)!)
				{
					if (token.IsCancellationRequested) continue;

					var binaryData = Convert.FromBase64String(partner.Logo!);

					Partners.Add(new PartnerView
					{
						Logo = new ImageBrush(ImageManipulation.GetBitmapImage(binaryData)),
						Partner = partner
					});

					LoadingProgress++;

					await Task.Delay(10, token);
				}

				IsBusy = false;
				IsLoading = false;

				MaxLoadingProgress = 0;
				LoadingProgress = 0;
			});
		}

		#endregion
	}

	/// <summary>
	/// Дані партнерів (для пошуку)
	/// </summary>
	public class PartnerSearch : ISearchSuggestionsSource
	{
		private readonly List<string> _currencies;

		public PartnerSearch(IEnumerable<Partner> partners) =>
			_currencies = partners.Select(partner => partner.Title)
				.ToList();

		public IList<string> GetAutoCompletion(string searchTerm) =>
			_currencies.Where(c => c.ToLower()
					.Contains(searchTerm.ToLower()))
				.ToList()!;

		public IList<string> GetSearchSuggestions() => _currencies!;
	}

	public class PartnerView
	{
		public ImageBrush Logo { get; set; }

		public Partner Partner { get; set; }
	}
}
