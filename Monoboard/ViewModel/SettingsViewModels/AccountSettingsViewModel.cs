using MaterialDesignExtensions.Controls;
using MaterialDesignThemes.Wpf;
using Monoboard.Helpers;
using Monoboard.Properties;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Monoboard.ViewModel.SettingsViewModels
{
	public partial class SettingsViewModel
	{
		#region Fields

		private bool _isPasswordCorrect;
		private string? _passwordValidate;
		private string? _confirmSecurePasswordValidate;

		private bool _isSetPasswordVisible;
		private bool _isChangePasswordVisible;
		private int _passwordSelectedIndex;
		private string? _checkPasswordValidate;
		private Visibility _isCheckPasswordValidateBlockVisible;

		public bool IsSetPasswordVisible
		{
			get => _isSetPasswordVisible;
			set
			{
				_isSetPasswordVisible = value;
				OnPropertyChanged(nameof(IsSetPasswordVisible));
			}
		}

		public bool IsChangePasswordVisible
		{
			get => _isChangePasswordVisible;
			set
			{
				_isChangePasswordVisible = value;
				OnPropertyChanged(nameof(IsChangePasswordVisible));
			}
		}

		public string? PasswordValidate
		{
			get => _passwordValidate;
			set
			{
				_passwordValidate = value;
				OnPropertyChanged(nameof(PasswordValidate));
			}
		}

		public string? CheckPasswordValidate
		{
			get => _checkPasswordValidate;
			set
			{
				_checkPasswordValidate = value;
				OnPropertyChanged(nameof(CheckPasswordValidate));
			}
		}

		public string? ConfirmPasswordValidate
		{
			get => _confirmSecurePasswordValidate;
			set
			{
				_confirmSecurePasswordValidate = value;
				OnPropertyChanged(nameof(ConfirmPasswordValidate));
			}
		}

		public bool IsPasswordCorrect
		{
			get => _isPasswordCorrect;
			set
			{
				_isPasswordCorrect = value;
				OnPropertyChanged(nameof(IsPasswordCorrect));
			}
		}

		public int PasswordSelectedIndex
		{
			get => _passwordSelectedIndex;
			set
			{
				_passwordSelectedIndex = value;
				OnPropertyChanged(nameof(PasswordSelectedIndex));
			}
		}

		public Visibility IsCheckPasswordValidateBlockVisible
		{
			get => _isCheckPasswordValidateBlockVisible;
			set
			{
				_isCheckPasswordValidateBlockVisible = value;
				OnPropertyChanged(nameof(IsCheckPasswordValidateBlockVisible));
			}
		}

		/// <summary>
		/// Для SettingsControl.xaml
		/// </summary>
		public bool IsValid { get; set; }

		#endregion

		#region Commands

		public ICommand SetNewPasswordCommand { get; }
		public ICommand UpdatePasswordCommand { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Застосовує відповідні дії виходячи з того чи становлений пароль у користувача
		/// </summary>
		public async void LoadAccountState()
		{
			IsChangePasswordVisible = await MonoboardCore.Get.GetMonoboardUser.GetPasswordExistAsync(Settings.Default.ClientId);
			IsSetPasswordVisible = !IsChangePasswordVisible;

			PasswordSelectedIndex = IsChangePasswordVisible ? 4 : 0;
		}

		/// <summary>
		/// Зберігає новий пароль користувача
		/// </summary>
		/// <param name="password">Пароль користувача</param>
		public async void SavePassword(string password)
		{
			var isSetNewPassword = await MonoboardCore.Set.SetMonoboardUserData.SetNewPassword(Settings.Default.ClientId,
				MonoboardCore.Hepler.Sha256Hash.Compute(password + Settings.Default.ClientId));

			PasswordSelectedIndex = isSetNewPassword ? 2 : 3;

			await Task.Delay(3000);

			if (isSetNewPassword)
			{
				PasswordSelectedIndex = 4;

				LoadAccountState();
			}
			else
				PasswordSelectedIndex = 0;

			IsPasswordCorrect = false;
			PasswordValidate = "";
			ConfirmPasswordValidate = "";
		}

		/// <summary>
		/// Змінює пароль користувача 
		/// </summary>
		/// <param name="passwords">Поточний і новий паролі користувача</param>
		public async void SavePassword(object[] passwords)
		{
			var currentPassword = "";
			var newPassword = "";

			for (var i = 0; i < passwords.Length; i++)
				if (i == 0)
					currentPassword = ((PasswordBox)passwords[i]).Password;
				else
					newPassword = ((PasswordBox)passwords[i]).Password;

			var isResetPassword = await MonoboardCore.Get.GetMonoboardUser.CheckIsResetPasswordAsync(Settings.Default.ClientId);

			var checkPassword = IsValid = isResetPassword
				? await MonoboardCore.Get.GetMonoboardUser.CheckPassword(
					Settings.Default.ClientId,
					MonoboardCore.Hepler.Sha256Hash.Compute($"user{Settings.Default.ClientId}!"))
				: await MonoboardCore.Get.GetMonoboardUser.CheckPassword(
					Settings.Default.ClientId,
					MonoboardCore.Hepler.Sha256Hash.Compute(currentPassword + Settings.Default.ClientId));

			if (checkPassword is false)
			{
				IsCheckPasswordValidateBlockVisible = Visibility.Visible;
				CheckPasswordValidate = App.GetResourceValue("MbPasswordErrorAlt");
				return;
			}

			if (currentPassword == newPassword)
			{
				IsCheckPasswordValidateBlockVisible = Visibility.Visible;
				CheckPasswordValidate = App.GetResourceValue("MbNotAllowedIdenticalPasswords");
				return;
			}


			var isSetNewPassword = await MonoboardCore.Set.SetMonoboardUserData.SetNewPassword(
				Settings.Default.ClientId,
				MonoboardCore.Hepler.Sha256Hash.Compute(newPassword + Settings.Default.ClientId));

			PasswordSelectedIndex = isSetNewPassword ? 5 : 3;

			await Task.Delay(3000);

			if (isSetNewPassword)
			{
				PasswordSelectedIndex = 4;

				LoadAccountState();
			}
			else
				PasswordSelectedIndex = 0;

			IsPasswordCorrect = false;
			PasswordValidate = "";
			CheckPasswordValidate = "";
			ConfirmPasswordValidate = "";
		}

		/// <summary>
		/// Видаляє користувача з бази даних і всю інформацію пов'язану з ним
		/// </summary>
		public async Task DeleteAccountAsync(DialogHost dialogHost)
		{
			var dialogArgs = new ConfirmationDialogArguments
			{
				Title = App.GetResourceValue("MbDeleteAccountTitle"),
				Message = App.GetResourceValue("MbDeleteAccountDescriptionShort"),
				OkButtonLabel = App.GetResourceValue("MbDeleteAccountOk"),
				CancelButtonLabel = App.GetResourceValue("MbDeleteAccountCancel")
			};

			if (await ConfirmationDialog.ShowDialogAsync(dialogHost, dialogArgs))
			{
				if (await MonoboardCore.Set.SetUserInfo.DeleteUserDataAsync(Settings.Default.ClientId))
				{
					Settings.Default.Reset();
					PaletteSettings.Default.Reset();

					try
					{
						Progress = 0;
						IsBusy = true;

						await Task.Run(async () =>
						{
							var progress = 0;

							while (progress < 100)
							{
								await Task.Delay(50);

								progress += 2;

								Progress = progress;
							}
						});
					}
					finally
					{
						IsBusy = false;
						await Task.Delay(100);
					}

					System.Diagnostics.Process.Start(
						Application.ResourceAssembly.Location.Replace(
							"Monoboard.dll",
							"Monoboard.exe"));

					Application.Current.Shutdown();
				}
				else
				{
					Messages.Clear();

					Messages.Enqueue(App.GetResourceValue("MbDeleteDataError")!);
				}
			}
		}

		#endregion
	}
}
