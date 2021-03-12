using MaterialDesignExtensions.Controls;
using Microsoft.Web.WebView2.Core;
using Monoboard.Helpers.Validation;
using Monoboard.ViewModel.SettingsViewModels;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Monoboard.View.Settings
{
	public partial class SettingsControl
	{
		public bool IsPasswordValid { get; set; }
		public bool IsConfirmPasswordValid { get; set; }
		public bool IsChekPasswordValid { get; set; }
		public bool IsCardRefresh { get; set; }
		public bool IsWebViewReady { get; set; }

		public SettingsControl()
		{
			InitializeComponent();

			PasswordValidateBlock.Visibility = Visibility.Collapsed;
			ConfirmPasswordValidateBlock.Visibility = Visibility.Collapsed;

			// Fix UI state after changed view
			Loaded += async (sender, args) =>
			{
				CheckPasswordValidateBlock.Visibility = Visibility.Collapsed;
				UpdatePasswordValidateBlock.Visibility = Visibility.Collapsed;
				UpdateConfirmPasswordValidateBlock.Visibility = Visibility.Collapsed;

				ToggleCompatibilityMode.IsChecked = Properties.Settings.Default.IsCompatibilityMode;

				if (((SettingsViewModel)DataContext).IsSetPasswordVisible)
					((SettingsViewModel)DataContext).PasswordSelectedIndex = 0;

				((SettingsViewModel)DataContext).CheckPasswordValidate = "";
				((SettingsViewModel)DataContext).PasswordValidate = "";
				((SettingsViewModel)DataContext).ConfirmPasswordValidate = "";
				((SettingsViewModel)DataContext).IsPasswordCorrect = false;

				await Task.Delay(500);

				RenderOptions.ProcessRenderMode = Properties.Settings.Default.IsCompatibilityMode
				? System.Windows.Interop.RenderMode.SoftwareOnly
				: System.Windows.Interop.RenderMode.Default;
			};
		}

		private void SettingsTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (Properties.Settings.Default.IsCompatibilityMode)
			{
				RenderOptions.ProcessRenderMode = ((TabControl)sender).SelectedIndex == 2
					? System.Windows.Interop.RenderMode.SoftwareOnly
					: System.Windows.Interop.RenderMode.Default;
			}
		}

		#region Account

		#region Password manipulation

		/// <summary>
		/// Відображає пароль
		/// </summary>
		private void ShowPassword_OnClick(object sender, RoutedEventArgs e)
		{
			if (ShowPasswordToggleButton.IsChecked is true)
			{
				PasswordBox.Visibility = Visibility.Collapsed;
				ShowPasswordBox.Text = PasswordBox.Password;
				ShowPasswordBox.Visibility = Visibility.Visible;
				ShowPasswordBox.Focus();
			}
			else
			{
				PasswordBox.Visibility = Visibility.Visible;
				ShowPasswordBox.Visibility = Visibility.Collapsed;
				PasswordBox.Focus();
			}
		}

		/// <summary>
		/// Відображає пароль для підтвердження
		/// </summary>
		private void ShowConfirmPassword_OnClick(object sender, RoutedEventArgs e)
		{
			if (ShowConfirmPasswordToggleButton.IsChecked is true)
			{
				ConfirmPasswordBox.Visibility = Visibility.Collapsed;
				ShowConfirmPasswordBox.Text = ConfirmPasswordBox.Password;
				ShowConfirmPasswordBox.Visibility = Visibility.Visible;
				ShowConfirmPasswordBox.Focus();
			}
			else
			{
				ConfirmPasswordBox.Visibility = Visibility.Visible;
				ShowConfirmPasswordBox.Visibility = Visibility.Collapsed;
				ConfirmPasswordBox.Focus();
			}
		}

		/// <summary>
		/// Переписує прихований пароль з поля в якому пароль відображено користувачу
		/// </summary>
		private void ShowPasswordBox_OnTextChanged(object sender, TextChangedEventArgs e) =>
			PasswordBox.Password = ShowPasswordBox.Text;

		/// <summary>
		/// Переписує прихований пароль (для підтвердження) з поля в якому
		/// пароль для підтвердження відображено користувачу
		/// </summary>
		private void ShowConfirmPasswordBox_OnTextChanged(object sender, TextChangedEventArgs e) =>
			ConfirmPasswordBox.Password = ShowConfirmPasswordBox.Text;

		private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
		{
			if (DataContext != null)
			{
				var (resourceValue, validate) = PasswordValidation.Validate(PasswordBox.Password);

				PasswordValidateBlock.Text = resourceValue;
				IsPasswordValid = validate;
			}

			PasswordValidateBlock.Visibility =
				PasswordValidateBlock.Text == App.GetResourceValue("MbPasswordSuccess")
					? Visibility.Collapsed
					: Visibility.Visible;

			PasswordValidate();
		}

		private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
		{
			if (DataContext != null)
			{
				var (resourceValue, validate) = PasswordValidation.Validate(ConfirmPasswordBox.Password);

				ConfirmPasswordValidateBlock.Text = resourceValue;
				IsConfirmPasswordValid = validate;
			}

			ConfirmPasswordValidateBlock.Visibility =
				ConfirmPasswordValidateBlock.Text == App.GetResourceValue("MbPasswordSuccess")
					? Visibility.Collapsed
					: Visibility.Visible;

			PasswordValidate();
		}

		private void PasswordValidate()
		{
			if (DataContext != null)
				(DataContext as SettingsViewModel)!.IsPasswordCorrect =
					PasswordValidation.Compare(ConfirmPasswordBox.SecurePassword, PasswordBox.SecurePassword) &&
					IsPasswordValid && IsConfirmPasswordValid;


			if (PasswordBox.Password == ConfirmPasswordBox.Password &&
				(DataContext as SettingsViewModel)!.IsPasswordCorrect)
			{
				if (PasswordBox.IsFocused || ShowPasswordBox.IsFocused)
				{
					if (string.IsNullOrWhiteSpace(PasswordBox.Password) ||
						string.IsNullOrEmpty(PasswordBox.Password))
						PasswordValidateBlock.Visibility = Visibility.Visible;
				}
				else
					PasswordValidateBlock.Visibility = Visibility.Collapsed;

				if (ConfirmPasswordBox.IsFocused || ShowConfirmPasswordBox.IsFocused)
				{
					if (string.IsNullOrWhiteSpace(ConfirmPasswordBox.Password) ||
						string.IsNullOrEmpty(ConfirmPasswordBox.Password))
						ConfirmPasswordValidateBlock.Visibility = Visibility.Visible;
				}
				else
					ConfirmPasswordValidateBlock.Visibility = Visibility.Collapsed;
			}
			else
			{
				PasswordValidateBlock.Visibility = Visibility.Visible;
				if (IsPasswordValid)
					PasswordValidateBlock.Text = App.GetResourceValue("MbPasswordError");

				ConfirmPasswordValidateBlock.Visibility = Visibility.Visible;
				if (IsConfirmPasswordValid)
					ConfirmPasswordValidateBlock.Text = App.GetResourceValue("MbPasswordError");
			}
		}

		#endregion

		#region UpdatePassword

		/// <summary>
		/// Відображає пароль
		/// </summary>
		private void UpdateShowPassword_OnClick(object sender, RoutedEventArgs e)
		{
			if (UpdateShowPasswordToggleButton.IsChecked is true)
			{
				UpdatePasswordBox.Visibility = Visibility.Collapsed;
				UpdateShowPasswordBox.Text = UpdatePasswordBox.Password;
				UpdateShowPasswordBox.Visibility = Visibility.Visible;
				UpdateShowPasswordBox.Focus();
			}
			else
			{
				UpdatePasswordBox.Visibility = Visibility.Visible;
				UpdateShowPasswordBox.Visibility = Visibility.Collapsed;
				UpdatePasswordBox.Focus();
			}
		}

		/// <summary>
		/// Відображає пароль для підтвердження
		/// </summary>
		private void UpdateShowConfirmPassword_OnClick(object sender, RoutedEventArgs e)
		{
			if (UpdateShowConfirmPasswordToggleButton.IsChecked is true)
			{
				UpdateConfirmPasswordBox.Visibility = Visibility.Collapsed;
				UpdateShowConfirmPasswordBox.Text = UpdateConfirmPasswordBox.Password;
				UpdateShowConfirmPasswordBox.Visibility = Visibility.Visible;
				UpdateShowConfirmPasswordBox.Focus();
			}
			else
			{
				UpdateConfirmPasswordBox.Visibility = Visibility.Visible;
				UpdateShowConfirmPasswordBox.Visibility = Visibility.Collapsed;
				UpdateConfirmPasswordBox.Focus();
			}
		}

		/// <summary>
		/// Переписує прихований пароль з поля в якому пароль відображено користувачу
		/// </summary>
		private void UpdateShowPasswordBox_OnTextChanged(object sender, TextChangedEventArgs e) =>
			UpdatePasswordBox.Password = UpdateShowPasswordBox.Text;

		/// <summary>
		/// Переписує прихований пароль (для підтвердження) з поля в якому
		/// пароль для підтвердження відображено користувачу
		/// </summary>
		private void UpdateShowConfirmPasswordBox_OnTextChanged(object sender, TextChangedEventArgs e) =>
			UpdateConfirmPasswordBox.Password = UpdateShowConfirmPasswordBox.Text;

		private void UpdatePasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
		{
			if (DataContext != null)
			{
				var (resourceValue, validate) = PasswordValidation.Validate(UpdatePasswordBox.Password);

				UpdatePasswordValidateBlock.Text = resourceValue;
				IsPasswordValid = validate;
			}

			UpdatePasswordValidateBlock.Visibility =
				UpdatePasswordValidateBlock.Text == App.GetResourceValue("MbPasswordSuccess")
					? Visibility.Collapsed
					: Visibility.Visible;

			CheckAndValidate();
		}


		private void UpdateConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
		{
			if (DataContext != null)
			{
				var (resourceValue, validate) = PasswordValidation.Validate(UpdateConfirmPasswordBox.Password);

				UpdateConfirmPasswordValidateBlock.Text = resourceValue;
				IsConfirmPasswordValid = validate;
			}

			UpdateConfirmPasswordValidateBlock.Visibility =
				UpdateConfirmPasswordValidateBlock.Text == App.GetResourceValue("MbPasswordSuccess")
					? Visibility.Collapsed
					: Visibility.Visible;

			CheckAndValidate();
		}

		/// <summary>
		/// Переписує прихований пароль з поля в якому пароль відображено користувачу
		/// </summary>
		private void CheckShowPasswordBox_OnTextChanged(object sender, TextChangedEventArgs e) =>
			CheckPasswordBox.Password = CheckShowPasswordBox.Text;

		private void CheckShowPassword_OnClick(object sender, RoutedEventArgs e)
		{
			if (CheckShowPasswordToggleButton.IsChecked is true)
			{
				CheckPasswordBox.Visibility = Visibility.Collapsed;
				CheckShowPasswordBox.Text = CheckPasswordBox.Password;
				CheckShowPasswordBox.Visibility = Visibility.Visible;
				CheckShowPasswordBox.Focus();
			}
			else
			{
				CheckPasswordBox.Visibility = Visibility.Visible;
				CheckShowPasswordBox.Visibility = Visibility.Collapsed;
				CheckPasswordBox.Focus();
			}
		}

		private void CheckPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
		{
			if (DataContext != null)
			{
				var (resourceValue, validate) = PasswordValidation.Validate(CheckPasswordBox.Password);

				CheckPasswordValidateBlock.Text = resourceValue;
				IsChekPasswordValid = validate;
			}


			CheckPasswordValidateBlock.Visibility =
				CheckPasswordValidateBlock.Text == App.GetResourceValue("MbPasswordSuccess")
					? Visibility.Collapsed
					: Visibility.Visible;

			CheckAndValidate();
		}

		private void CheckAndValidate()
		{
			if (DataContext != null)
				(DataContext as SettingsViewModel)!.IsPasswordCorrect =
					PasswordValidation.Compare(UpdateConfirmPasswordBox.SecurePassword, UpdatePasswordBox.SecurePassword) &&
					 IsPasswordValid && IsConfirmPasswordValid && IsChekPasswordValid;


			if (UpdatePasswordBox.Password == UpdateConfirmPasswordBox.Password &&
				(DataContext as SettingsViewModel)!.IsPasswordCorrect)
			{
				if (UpdatePasswordBox.IsFocused || UpdateShowPasswordBox.IsFocused)
				{
					if (string.IsNullOrWhiteSpace(UpdatePasswordBox.Password) ||
						string.IsNullOrEmpty(UpdatePasswordBox.Password))
						UpdatePasswordValidateBlock.Visibility = Visibility.Visible;
				}
				else
					UpdatePasswordValidateBlock.Visibility = Visibility.Collapsed;


				if (UpdateConfirmPasswordBox.IsFocused || UpdateShowConfirmPasswordBox.IsFocused)
				{
					if (string.IsNullOrWhiteSpace(UpdateConfirmPasswordBox.Password) ||
						string.IsNullOrEmpty(UpdateConfirmPasswordBox.Password))
						UpdateConfirmPasswordValidateBlock.Visibility = Visibility.Visible;
				}
				else
					UpdateConfirmPasswordValidateBlock.Visibility = Visibility.Collapsed;
			}
			else
			{
				UpdatePasswordValidateBlock.Visibility = Visibility.Visible;
				if (IsPasswordValid)
					UpdatePasswordValidateBlock.Text = App.GetResourceValue("MbPasswordError");

				UpdateConfirmPasswordValidateBlock.Visibility = Visibility.Visible;
				if (IsConfirmPasswordValid)
					UpdateConfirmPasswordValidateBlock.Text = App.GetResourceValue("MbPasswordError");
			}
		}

		private async void ChangePassword_OnClick(object sender, RoutedEventArgs e)
		{
			CheckPasswordBox.IsEnabled = false;
			CheckShowPasswordBox.IsEnabled = false;
			UpdatePasswordBox.IsEnabled = false;
			UpdateShowPasswordBox.IsEnabled = false;
			UpdateConfirmPasswordBox.IsEnabled = false;
			UpdateShowConfirmPasswordBox.IsEnabled = false;

			await Task.Delay(3000);

			if (((SettingsViewModel)DataContext).IsValid)
			{
				CheckPasswordBox.IsEnabled = true;
				CheckPasswordBox.Clear();
				CheckShowPasswordBox.IsEnabled = true;
				CheckShowPasswordBox.Clear();

				UpdatePasswordBox.IsEnabled = true;
				UpdatePasswordBox.Clear();
				UpdateShowPasswordBox.IsEnabled = true;
				UpdateShowPasswordBox.Clear();

				UpdateConfirmPasswordBox.IsEnabled = true;
				UpdateConfirmPasswordBox.Clear();
				UpdateShowConfirmPasswordBox.IsEnabled = true;
				UpdateShowConfirmPasswordBox.Clear();

				CheckPasswordValidateBlock.Visibility = Visibility.Collapsed;
				UpdatePasswordValidateBlock.Visibility = Visibility.Collapsed;
				UpdateConfirmPasswordValidateBlock.Visibility = Visibility.Collapsed;
			}
			else
			{
				CheckPasswordBox.IsEnabled = true;
				CheckShowPasswordBox.IsEnabled = true;
				UpdatePasswordBox.IsEnabled = true;
				UpdateShowPasswordBox.IsEnabled = true;
				UpdateConfirmPasswordBox.IsEnabled = true;
				UpdateShowConfirmPasswordBox.IsEnabled = true;
			}
		}

		/// <summary>
		/// Викликає з SettingsViewModel функцію "Забув пароль"
		/// </summary>
		private async void ForgotPassword_OnClick(object sender, RoutedEventArgs e)
		{
			ForgotPasswordViewModel viewModel = new ForgotPasswordViewModel();

			AlertDialogArguments dialogArgs = new AlertDialogArguments
			{
				Title = App.GetResourceValue("MbPasswordResetTitle"),
				Message = App.GetResourceValue("MbPasswordResetDescription"),
				OkButtonLabel = App.GetResourceValue("MbClose"),
				CustomContent = viewModel,
				CustomContentTemplate = FindResource("ForgotPasswordViewModelTemplate") as DataTemplate,
			};

			await AlertDialog.ShowDialogAsync(DialogHost, dialogArgs);
		}

		#endregion

		private void Selector_OnSelectionChanged(object sender,
			SelectionChangedEventArgs e) =>
			SetPasswordContent.Visibility = PasswordTransitioner.SelectedIndex == 1
				? Visibility.Visible
				: Visibility.Collapsed;

		/// <summary>
		/// Викликає з SettingsViewModel функцію видалення користувача з застосунку
		/// </summary>
		private async void DeleteAccountButton_OnClick(object sender, RoutedEventArgs e) =>
			await ((SettingsViewModel)DataContext).DeleteAccountAsync(DialogHost);

		#endregion

		#region CardSettings

		private void TabControlCardCustomization_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var tabControl = (TabControl)sender;

			if ((SettingsViewModel)DataContext != null)
				if (tabControl.Items.IsEmpty is false)
					if (IsCardRefresh)
					{
						tabControl.SelectedIndex = ((SettingsViewModel)DataContext).SelectedIndexCardSettings;
						IsCardRefresh = false;
					}
					else
						((SettingsViewModel)DataContext).SelectedIndexCardSettings = tabControl.SelectedIndex;
				else
					IsCardRefresh = true;
		}

		#endregion

		#region ApiSettings

		private async void WebControl_OnCoreWebView2Ready(object? sender, EventArgs e)
		{
			WebControl.CoreWebView2.Settings.IsStatusBarEnabled = false;
			WebControl.CoreWebView2.Settings.AreDevToolsEnabled = false;

			var path = Application.ResourceAssembly.Location.Replace(
				"Monoboard.dll",
				"Resources\\JS\\qrCodeVisualFormatter.js");

			var script = await File.ReadAllTextAsync(path);

			await WebControl.ExecuteScriptAsync(script);

			RefreshWebView();

			IsWebViewReady = true;
		}

		private async void RefreshWebView()
		{
			while (true)
			{
				await Task.Delay(TimeSpan.FromMinutes(1));

				WebControl.Reload();
			}
		}

		private async void WebControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (!IsWebViewReady) return;

			if (WebControl.ActualHeight < 750 && WebControl.Source.OriginalString.Contains("api.monobank"))
				WebControl.ZoomFactor = 0.85;
			else
				WebControl.ZoomFactor = 1;

			var path = Application.ResourceAssembly.Location.Replace(
				"Monoboard.dll",
				"Resources\\JS\\qrCodeVisualFormatter.js");

			var script = await File.ReadAllTextAsync(path);

			await WebControl.ExecuteScriptAsync(script);
		}

		private async void WebControl_OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
		{
			if (e.Uri.StartsWith("https://mbnk.app"))
			{
				await Task.Delay(250);

				var path = Application.ResourceAssembly.Location.Replace(
					"Monoboard.dll",
					"Resources\\JS\\qrCodeMessages.js");

				var script = await File.ReadAllTextAsync(path);

				await WebControl.ExecuteScriptAsync(script);

				await Task.Delay(5000);

				WebControl?.CoreWebView2.Navigate("https://api.monobank.ua/");
			}
			else if (e.Uri.StartsWith("https://api.monobank.ua/"))
			{
				await Task.Delay(100);

				var path = Application.ResourceAssembly.Location.Replace(
					"Monoboard.dll",
					"Resources\\JS\\qrCodeVisualFormatter.js");

				var script = await File.ReadAllTextAsync(path);

				await WebControl.ExecuteScriptAsync(script);
			}

			if (WebControl != null &&
				WebControl.ActualHeight < 750 &&
				WebControl.Source.OriginalString.Contains("api.monobank"))
				WebControl.ZoomFactor = 0.85;
			else if (WebControl != null)
				WebControl.ZoomFactor = 1;
		}

		#endregion

		#region Compatibility Section

		private void CompatibilityMode_Checked(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.IsCompatibilityMode = true;
			Properties.Settings.Default.Save();
		}

		private void CompatibilityMode_OnUnchecked(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.IsCompatibilityMode = false;
			Properties.Settings.Default.Save();
		}

		private void ToggleCompatibilityMode_OnMouseEnter(object sender, MouseEventArgs e) =>
			RenderOptions.ProcessRenderMode = Properties.Settings.Default.IsCompatibilityMode
				? System.Windows.Interop.RenderMode.SoftwareOnly
				: System.Windows.Interop.RenderMode.Default;

		private void PasswordTransitioner_OnMouseEnter(object sender, MouseEventArgs e) =>
			RenderOptions.ProcessRenderMode = Properties.Settings.Default.IsCompatibilityMode
				? System.Windows.Interop.RenderMode.SoftwareOnly
				: System.Windows.Interop.RenderMode.Default;

		#endregion
	}
}
