using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Monoboard.Helpers.Command;
using System;
using System.Windows.Controls;

namespace Monoboard.ViewModel.SettingsViewModels
{
	public partial class SettingsViewModel : Helpers.ViewModel
	{
		#region Fields

		private bool _isBusy;
		private int _progress;
		private SnackbarMessageQueue _messages;

		public bool IsBusy
		{
			get => _isBusy;
			set
			{
				_isBusy = value;
				OnPropertyChanged(nameof(IsBusy));
			}
		}

		public int Progress
		{
			get => _progress;
			set
			{
				_progress = value;

				OnPropertyChanged(nameof(Progress));
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

		#endregion

		public SettingsViewModel()
		{
			_isBusy = false;
			_progress = 0;

			#region Account

			SetNewPasswordCommand = new DelegateCommand(o => SavePassword(((PasswordBox)o).Password));
			UpdatePasswordCommand = new DelegateCommand(o => SavePassword((object[])o));

			LoadAccountState();

			#endregion

			#region Theme

			ThemeSwitcherCommand = new DelegateCommand(o => App.ApplyTheme((bool)o));
			SetAdvancedThemingCommand = new DelegateCommand(o => ApplyAdvanced((bool)o));
			ApplyPrimaryCommand = new DelegateCommand(o => ApplyPrimary((Swatch)o));
			ApplyAccentCommand = new DelegateCommand(o => ApplyAccent((Swatch)o));
			RestoreThemeCommand = new DelegateCommand(o => ResetTheme());

			Swatches = new SwatchesProvider().Swatches;

			var paletteHelper = new PaletteHelper();
			var theme = paletteHelper.GetTheme();

			IsDark = theme.GetBaseTheme() == BaseTheme.Dark;

			if (paletteHelper.GetThemeManager() is { } themeManager)
				themeManager.ThemeChanged += (_, e) =>
					IsDark = e.NewTheme.GetBaseTheme() == BaseTheme.Dark;

			LoadThemeState();

			#endregion

			#region Cards

			LoadCard();

			#endregion

			#region API

			Messages ??= new SnackbarMessageQueue();

			SaveMonobankTokenCommand = new DelegateCommand(async o => await SaveAccessTokenAsync((string)o));

			#endregion
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
	}
}
