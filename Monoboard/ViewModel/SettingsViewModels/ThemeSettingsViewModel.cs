using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Monoboard.Properties;
using MonoboardCore.Set;

namespace Monoboard.ViewModel.SettingsViewModels
{
	public partial class SettingsViewModel
	{
		#region Fields

		private static bool _isAdvancedTheme;
		private bool _isDark;
		private bool _isRestoreEnable;

		public bool IsDark
		{
			get => _isDark;
			set
			{
				_isDark = value;

				PaletteSettings.Default.IsDark = value;
				PaletteSettings.Default.Save();

				SetMonoboardUserData.
					UpdateTheme(Settings.Default.ClientId, value)
					.Wait();

				if (value ||
					PaletteSettings.Default.PrimaryColor.Name != "#fff43b3f" ||
					PaletteSettings.Default.SecondaryColor.Name != "#ff06b206")
					IsRestoreEnable = true;
				else
					IsRestoreEnable = false;

				OnPropertyChanged(nameof(IsDark));
			}
		}

		public bool IsAdvancedTheme
		{
			get => _isAdvancedTheme;
			set
			{
				_isAdvancedTheme = value;

				PaletteSettings.Default.IsAdvancedTheme = value;
				PaletteSettings.Default.Save();

				OnPropertyChanged(nameof(IsAdvancedTheme));
			}
		}

		public bool IsRestoreEnable
		{
			get => _isRestoreEnable;
			set
			{
				_isRestoreEnable = value;
				OnPropertyChanged(nameof(IsRestoreEnable));
			}
		}

		public IEnumerable<Swatch> Swatches { get; }
		#endregion

		#region Commands

		public ICommand ThemeSwitcherCommand { get; }
		public ICommand SetAdvancedThemingCommand { get; }
		public ICommand RestoreThemeCommand { get; }
		public ICommand ApplyPrimaryCommand { get; }
		public ICommand ApplyAccentCommand { get; }

		#endregion

		#region Methods

		private void LoadThemeState()
		{
			IsDark = PaletteSettings.Default.IsDark;
			IsAdvancedTheme = PaletteSettings.Default.IsAdvancedTheme;
		}

		private void ApplyPrimary(Swatch swatch)
		{
			App.ModifyTheme(theme => theme.SetPrimaryColor(swatch.ExemplarHue.Color));

			IsRestoreEnable = true;
		}

		private void ApplyAccent(Swatch swatch)
		{
			if (swatch.AccentExemplarHue != null)
				App.ModifyTheme(theme => theme.SetSecondaryColor(swatch.AccentExemplarHue.Color));

			IsRestoreEnable = true;
		}

		public void ApplyAdvanced(bool isAdvanced) => IsAdvancedTheme = isAdvanced;

		private async void ResetTheme()
		{
			App.ApplyTheme(false);

			var isPrimaryChanged = PaletteSettings.Default.PrimaryColor.Name.Contains("#")
				? PaletteSettings.Default.PrimaryColor.Name != "#fff43b3f"
				: PaletteSettings.Default.PrimaryColor.Name != "fff43b3f";

			var isSecondaryChanged = PaletteSettings.Default.PrimaryColor.Name.Contains("#")
				? PaletteSettings.Default.SecondaryColor.Name != "#ff06b206"
				: PaletteSettings.Default.SecondaryColor.Name != "ff06b206";

			if (isPrimaryChanged || isSecondaryChanged)
			{
				PaletteSettings.Default.Reset();

				await SetMonoboardUserData.ResetColorTheme(Settings.Default.ClientId);

				Process.Start(
					Application.ResourceAssembly.Location.Replace(
						"Monoboard.dll",
						"Monoboard.exe"));

				Application.Current.Shutdown();
			}
			else
			{
				IsAdvancedTheme = false;
				PaletteSettings.Default.Reset();
			}
		}

		#endregion
	}
}
