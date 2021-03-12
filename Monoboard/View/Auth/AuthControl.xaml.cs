using MaterialDesignExtensions.Controls;
using Microsoft.Web.WebView2.Core;
using Monoboard.Helpers.Validation;
using Monoboard.ViewModel.AuthViewModels;
using Monoboard.ViewModel.SettingsViewModels;
using MonoboardCore.Get;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MonoboardCore.Hepler;

namespace Monoboard.View.Auth
{
	public partial class AuthControl
	{
		public bool IsReady;

		public AuthControl()
		{
			InitializeComponent();

			DataContext = new AuthViewModel(DialogHost);
		}

		private void WebControl_OnCoreWebView2Ready(object? sender, EventArgs e)
		{
			WebControl.CoreWebView2.Settings.IsStatusBarEnabled = false;
			WebControl.CoreWebView2.Settings.AreDevToolsEnabled = false;

			RefreshWebView();

			IsReady = true;
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
			if (!IsReady) return;

			if (WebControl.ActualWidth < 960 && WebControl.Source.OriginalString.Contains("api.monobank"))
				await WebControl.ExecuteScriptAsync("document.querySelector('.core').style.alignItems='initial';");
			else
				await WebControl.ExecuteScriptAsync("document.querySelector('.core').style.alignItems='center';");
		}

		private void Transitioner_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (WebControl != null)
				WebControl.Visibility = Transitioner.SelectedIndex != 2 ? Visibility.Collapsed : Visibility.Visible;
		}

		private async void WebControl_OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
		{
			if (e.Uri.StartsWith("https://mbnk.app"))
			{
				await Task.Delay(100);

				var path = Application.ResourceAssembly.Location.Replace(
					"Monoboard.dll",
					"Resources/JS/qrCodeMessages.js");

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
					"Resources/JS/qrCodeVisualFormatter.js");

				var script = await File.ReadAllTextAsync(path);

				await WebControl.ExecuteScriptAsync(script);
			}
		}

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
			var isEqual = string.Equals(ConfirmPasswordBox.Password, PasswordBox.Password);

			if (DataContext != null)
				((AuthViewModel)DataContext).PasswordValidate =
					PasswordValidation.Validate(PasswordBox.Password, isEqual)!.resourceValue;

			PasswordValidateBlock.Visibility =
				PasswordValidateBlock.Text == App.GetResourceValue("MbPasswordSuccess")
					? Visibility.Collapsed
					: Visibility.Visible;

			if (DataContext != null)
				((AuthViewModel)DataContext).IsPasswordCorrect =
					PasswordValidation.Compare(((PasswordBox)sender).SecurePassword, ConfirmPasswordBox.SecurePassword) &&
					((AuthViewModel)DataContext).PasswordValidate == App.GetResourceValue("MbPasswordSuccess");

			if (PasswordBox.Password == ConfirmPasswordBox.Password)
			{
				PasswordValidateBlock.Visibility = Visibility.Collapsed;
				ConfirmPasswordValidateBlock.Visibility = Visibility.Collapsed;
			}
		}

		private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
		{
			var isEqual = string.Equals(PasswordBox.Password, ConfirmPasswordBox.Password);

			if (DataContext != null)
				((AuthViewModel)DataContext).ConfirmPasswordValidate =
					PasswordValidation.Validate(ConfirmPasswordBox.Password, isEqual)!.resourceValue;

			ConfirmPasswordValidateBlock.Visibility =
				ConfirmPasswordValidateBlock.Text == App.GetResourceValue("MbPasswordSuccess")
					? Visibility.Collapsed
					: Visibility.Visible;

			if (DataContext != null)
				((AuthViewModel)DataContext).IsPasswordCorrect =
					PasswordValidation.Compare(((PasswordBox)sender).SecurePassword, PasswordBox.SecurePassword) &&
					((AuthViewModel)DataContext).ConfirmPasswordValidate == App.GetResourceValue("MbPasswordSuccess");

			if (PasswordBox.Password == ConfirmPasswordBox.Password)
			{
				PasswordValidateBlock.Visibility = Visibility.Collapsed;
				ConfirmPasswordValidateBlock.Visibility = Visibility.Collapsed;
			}
		}

		#endregion

		private void Back_OnClick(object sender, RoutedEventArgs e)
		{
			ShowPasswordToggleButton.IsChecked = false;
			ShowConfirmPasswordToggleButton.IsChecked = false;

			PasswordBox.Password = "";
			ConfirmPasswordBox.Password = "";

			Transitioner.SelectedIndex = 2;
		}
		private void Error_OnClick(object sender, RoutedEventArgs e)
		{
			((AuthViewModel)DataContext).RegisterTransitionerIndex = 0;
			Transitioner.SelectedIndex = 2;
		}

		private void Success_OnClick(object sender, RoutedEventArgs e)
		{
			WebControl = null;

			Process.Start(
				Application.ResourceAssembly.Location.Replace(
					"Monoboard.dll",
					"Monoboard.exe"));

			Application.Current.Shutdown();
		}

		private void TransitionerBackToUser_OnClick(object sender, RoutedEventArgs e)
		{
			if (LoginTransitioner.SelectedIndex != 0) LoginTransitioner.SelectedIndex = 0;

			PasswordBox.Clear();
			ShowPasswordBox.Clear();
			LoginPasswordBox.Clear();
			ShowLoginPasswordBox.Clear();
			ConfirmPasswordBox.Clear();
			ShowConfirmPasswordBox.Clear();
			MonobankApiKey.Clear();
			ForgotPasswordCheckMonobankApiKey.Clear();

			Transitioner.SelectedIndex = 1;
		}

		private void LoginPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
		{
			var (resourceValue, validate) = PasswordValidation.Validate(LoginPasswordBox.Password);

			if (DataContext != null)
				((AuthViewModel)DataContext).LoginValidate = resourceValue;

			LoginValidateBlock.Visibility =
				LoginValidateBlock.Text == App.GetResourceValue("MbPasswordSuccess")
					? Visibility.Collapsed
					: Visibility.Visible;

			(DataContext as AuthViewModel)!.IsLoginPasswordCorrect = validate;
		}

		private void ShowLoginPassword_OnClick(object sender, RoutedEventArgs e)
		{

			if (ShowLoginPasswordToggleButton.IsChecked is true)
			{
				LoginPasswordBox.Visibility = Visibility.Collapsed;
				ShowLoginPasswordBox.Text = LoginPasswordBox.Password;
				ShowLoginPasswordBox.Visibility = Visibility.Visible;
				ShowLoginPasswordBox.Focus();
			}
			else
			{
				LoginPasswordBox.Visibility = Visibility.Visible;
				ShowLoginPasswordBox.Visibility = Visibility.Collapsed;
				LoginPasswordBox.Focus();
			}
		}

		private void ShowLoginPasswordBox_OnTextChanged(object sender, TextChangedEventArgs e) =>
			LoginPasswordBox.Password = ShowLoginPasswordBox.Text;

		private void ToForgotPassword_OnClick(object sender, RoutedEventArgs e)
		{
			Transitioner.SelectedIndex = 2;
			LoginTransitioner.SelectedIndex = 1;
		}

		private async void ForgotPassword_OnClick(object sender, RoutedEventArgs e)
		{
			var user = ((AuthViewModel)DataContext).UserList[((AuthViewModel)DataContext).SelectedUserIndex];

			if (Internet.IsConnectedToInternet() is false)
			{
				((AuthViewModel)DataContext).Messages.Clear();
				((AuthViewModel)DataContext).Messages.Enqueue(App.GetResourceValue("MbNoInternet")!);

				return;
			}

			var (isSuccess, clientIdOrMessage) = await GetUserInfo.GetClientIdAsync(ForgotPasswordCheckMonobankApiKey.Text);

			var isEqualToken = user.MonoboardUser.AccessToken == ForgotPasswordCheckMonobankApiKey.Text;

			if (isEqualToken)
			{
				await AlertDialog.ShowDialogAsync(DialogHost, new AlertDialogArguments
				{
					Title = App.GetResourceValue("MbPasswordResetTitle"),
					Message = App.GetResourceValue("MbPasswordResetDescription"),
					OkButtonLabel = App.GetResourceValue("MbClose"),
					CustomContent = new ForgotPasswordViewModel(user.ClientId),
					CustomContentTemplate = FindResource("ForgotPasswordViewModelTemplate") as DataTemplate,
				}).ContinueWith(delegate
				{
					Application.Current.Dispatcher.BeginInvoke(delegate
					{
						Transitioner.SelectedIndex = 4;
					});
				});
			}
			else if (isSuccess)
			{
				if (string.Equals(clientIdOrMessage, user.ClientId))
				{
					await AlertDialog.ShowDialogAsync(DialogHost, new AlertDialogArguments
					{
						Title = App.GetResourceValue("MbPasswordResetTitle"),
						Message = App.GetResourceValue("MbPasswordResetDescription"),
						OkButtonLabel = App.GetResourceValue("MbClose"),
						CustomContent = new ForgotPasswordViewModel(user.ClientId),
						CustomContentTemplate = FindResource("ForgotPasswordViewModelTemplate") as DataTemplate,
					}).ContinueWith(delegate
					{
						Application.Current.Dispatcher.BeginInvoke(delegate
						{
							Transitioner.SelectedIndex = 4;
						});
					});
				}
				else
				{
					await AlertDialog.ShowDialogAsync(DialogHost, new AlertDialogArguments
					{
						Title = App.GetResourceValue("MbPasswordResetTitle"),
						Message = App.GetResourceValue("MbWrongAccount"),
						OkButtonLabel = App.GetResourceValue("MbClose")
					}).ContinueWith(delegate
					{
						Application.Current.Dispatcher.BeginInvoke(delegate
						{
							ForgotPasswordCheckMonobankApiKey.Clear();
							Transitioner.SelectedIndex = 1;
						});
					});
				}
			}
			else
			{
				await AlertDialog.ShowDialogAsync(DialogHost, new AlertDialogArguments
				{
					Title = App.GetResourceValue("MbPasswordResetTitle"),
					Message = App.GetResourceValue("MbNoInternet"),
					OkButtonLabel = App.GetResourceValue("MbClose")
				}).ContinueWith(delegate
				{
					Application.Current.Dispatcher.BeginInvoke(delegate
					{
						ForgotPasswordCheckMonobankApiKey.Clear();
						Transitioner.SelectedIndex = 1;
					});
				});
			}
		}
	}
}
