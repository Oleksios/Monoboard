#nullable enable
using MaterialDesignExtensions.Controls;
using MaterialDesignExtensions.Model;
using MaterialDesignThemes.Wpf;
using Monoboard.Properties;
using Monoboard.ViewModel;
using Monoboard.ViewModel.ExchangeRatesViewModels;
using Monoboard.ViewModel.SettingsViewModels;
using Monoboard.ViewModel.StatementItemsViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Monoboard.View
{
	public partial class MainWindow
	{
		private bool _isAppLoading;
		private object _oldContent;

		public ObservableCollection<INavigationItem> NavigationItems { get; set; }

		private readonly string[] _label =
		{
			"MbDashboard",
			"MbExchangeRate",
			"MbStatementAccount",
			"MbPartnership",
			"MbSettings"
		};

		private readonly PackIconKind[] _iconKinds =
		{
			PackIconKind.ViewDashboard,
			PackIconKind.CurrencyUsd,
			PackIconKind.Receipt,
			PackIconKind.Partnership,
			PackIconKind.Settings
		};

		public MainWindow()
		{
			NavigationItems = new ObservableCollection<INavigationItem>();

			SetNavigation();

			InitializeComponent();

			DataContext = new MainWindowViewModel();

			SideNav.DataContext = this;
			NavigationDrawerNav.DataContext = this;

			Loaded += LoadedHandler;
		}

		private void LoadedHandler(object sender, RoutedEventArgs args)
		{
			NavigationDrawerNav.SelectedItem = NavigationItems[0];
			SideNav.SelectedItem = NavigationItems[0];
			NavigationItems[0].IsSelected = true;

			SelectLanguage.SelectedIndex = App.Language.Name switch
			{
				"uk-UA" => 0,
				"en-US" => 1,
				"ru-RU" => 2,
				_ => 0
			};

			((MainWindowViewModel)DataContext).GetInfo();

			_isAppLoading = true;
		}

		public void SetNavigation()
		{
			if (IsLoaded)
			{
				var indexIsSelection = 0;

				if (NavigationItems.Any())
				{
					for (var i = 0; i < NavigationItems.Count; i++)
						if (NavigationItems[i].IsSelected)
							indexIsSelection = i;

					NavigationItems.Clear();
				}

				for (var i = 0; i < _label.Length; i++)
				{
					object? currentViewmodel = _label[i] switch
					{
						"MbDashboard" => new DashboardViewModel(),
						"MbExchangeRate" => new ExchangesViewModel(),
						"MbStatementAccount" => new StatementItemsViewModel(),
						"MbPartnership" => new PartnershipViewModel(),
						"MbSettings" => new SettingsViewModel(),
						_ => null
					};

					NavigationItems.Add(new FirstLevelNavigationItem
					{
						Label = App.GetResourceValue(_label[i]),
						Icon = _iconKinds[i],
						NavigationItemSelectedCallback = item => currentViewmodel
					});
				}

				NavigationDrawerNav.SelectedItem = NavigationItems![indexIsSelection];
				SideNav.SelectedItem = NavigationItems[indexIsSelection];
				NavigationItems[indexIsSelection].IsSelected = true;
			}
			else
			{
				for (var i = 0; i < _label.Length; i++)
				{
					object? currentViewmodel = _label[i] switch
					{
						"MbDashboard" => new DashboardViewModel(),
						"MbExchangeRate" => new ExchangesViewModel(),
						"MbStatementAccount" => new StatementItemsViewModel(),
						"MbPartnership" => new PartnershipViewModel(),
						"MbSettings" => new SettingsViewModel(),
						_ => null
					};

					NavigationItems.Add(new FirstLevelNavigationItem
					{
						Label = App.GetResourceValue(_label[i]),
						Icon = _iconKinds[i],
						NavigationItemSelectedCallback = item => currentViewmodel
					});
				}
			}
		}

		private void GoToAppHelperClickHandler(object sender, RoutedEventArgs args) =>
			MainTransitioner.SelectedIndex = 1;

		private void GoToGitHubButtonClickHandler(object sender,
			RoutedEventArgs args) =>
			Process.Start(new ProcessStartInfo("https://github.com/Oleksios/Monoboard")
			{
				UseShellExecute = true
			});

		private void NavigationItemSelectedHandler(object sender,
			NavigationItemSelectedEventArgs args)
		{
			SelectNavigationItem(args.NavigationItem);
		}

		private async void SelectNavigationItem(INavigationItem navigationItem)
		{
			if (navigationItem != null)
			{
				SideNav.IsEnabled = false;
				NavigationDrawerNav.IsEnabled = false;

				_oldContent ??= navigationItem;

				if (_oldContent.GetType() == typeof(ExchangesViewModel))
					((ExchangesViewModel)_oldContent).IsOpenDialogHostConverter = false;

				var newContent = navigationItem.NavigationItemSelectedCallback(navigationItem);

				if (ContentControl.Content == null || ContentControl.Content.GetType() != newContent.GetType())
				{
					RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
					ContentControl.Content = newContent;

					if (newContent.GetType() == typeof(DashboardViewModel))
						TitleBlock.Text = App.GetResourceValue("MbDashboard");
					else if (newContent.GetType() == typeof(ExchangesViewModel))
					{
						_oldContent = newContent;
						TitleBlock.Text = App.GetResourceValue("MbExchangeRate");
					}
					else if (newContent.GetType() == typeof(StatementItemsViewModel))
						TitleBlock.Text = App.GetResourceValue("MbStatementAccount");
					else if (newContent.GetType() == typeof(PartnershipViewModel))
						TitleBlock.Text = App.GetResourceValue("MbPartnership");
					else if (newContent.GetType() == typeof(SettingsViewModel))
						TitleBlock.Text = App.GetResourceValue("MbSettings");
				}

				await Task.Delay(750);

				SideNav.IsEnabled = true;
				NavigationDrawerNav.IsEnabled = true;
			}
			else
				ContentControl.Content = null;
		}

		private async void SelectLanguage_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			App.Language = SelectLanguage.SelectedIndex switch
			{
				0 => new CultureInfo("uk-UA"),
				1 => new CultureInfo("en-US"),
				2 => new CultureInfo("ru-RU"),
				_ => App.Language
			};

			Properties.Settings.Default.Language = App.Language;
			Properties.Settings.Default.Save();

			var isUpdated = await MonoboardCore.Set.SetMonoboardUserData.UpdateLanguage(
				Properties.Settings.Default.ClientId,
				App.Language.Name);

			if (!_isAppLoading) return;

			if (isUpdated)
			{
				MainWindowSnackbar.MessageQueue?.Clear();

				switch (App.Language.Name)
				{
					case "uk-UA":
						MainWindowSnackbar.MessageQueue?.Enqueue(
							$"{App.GetResourceValue("MbSelectedInfo")}: {App.GetResourceValue("MbUkrLanguage")}");
						break;
					case "ru-RU":
						MainWindowSnackbar.MessageQueue?.Enqueue(
							$"{App.GetResourceValue("MbSelectedInfo")}: {App.GetResourceValue("MbRusLanguage")}");
						break;
					case "en-US":
						MainWindowSnackbar.MessageQueue?.Enqueue(
							$"{App.GetResourceValue("MbSelectedInfo")}: {App.GetResourceValue("MbEngLanguage")}");
						break;
				}
			}

			SetNavigation();
		}

		private void ExitApp_OnClick(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.Reset();
			PaletteSettings.Default.Reset();

			Process.Start(
				Application.ResourceAssembly.Location.Replace(
					"Monoboard.dll",
					"Monoboard.exe"));

			Application.Current.Shutdown();
		}
	}
}