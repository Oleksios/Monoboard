using Monoboard.Helpers;
using Monoboard.Properties;
using System.Windows;
using System.Windows.Input;
using Monoboard.Helpers.Command;

namespace Monoboard.ViewModel.SettingsViewModels
{
	public class ForgotPasswordViewModel : Helpers.ViewModel
	{
		private bool _isValid;
		private string? _newPassword;
		private int _forgotPasswordTransitionerIndex;

		public bool IsValid
		{
			get => _isValid;
			set
			{
				_isValid = value;
				OnPropertyChanged(nameof(IsValid));
			}
		}

		public int ForgotPasswordTransitionerIndex
		{
			get => _forgotPasswordTransitionerIndex;
			set
			{
				_forgotPasswordTransitionerIndex = value;
				OnPropertyChanged(nameof(ForgotPasswordTransitionerIndex));
			}
		}

		public string? NewPassword
		{
			get => _newPassword;
			set
			{
				_newPassword = value;
				OnPropertyChanged(nameof(NewPassword));
			}
		}

		public ICommand PasswordResetCommand { get; set; }
		public ICommand CopyPasswordCommand { get; set; }

		public ForgotPasswordViewModel(string clientId = "")
		{
			if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrEmpty(clientId))
				PasswordResetCommand = new DelegateCommand(o => ResetPassword());
			else
				PasswordResetCommand = new DelegateCommand(o => ResetPassword(clientId));

			CopyPasswordCommand = new DelegateCommand(o => CopyPassword());
		}

		private async void ResetPassword(string clientId = "")
		{
			ForgotPasswordTransitionerIndex = 1;

			var (isResetCompleted, newPassword) = string.IsNullOrWhiteSpace(clientId) || string.IsNullOrEmpty(clientId)
				? await MonoboardCore.Set.SetMonoboardUserData.ResetPasswordAsync(Settings.Default.ClientId)
				: await MonoboardCore.Set.SetMonoboardUserData.ResetPasswordAsync(clientId);

			if (isResetCompleted) NewPassword = newPassword;
		}

		private void CopyPassword() => Clipboard.SetText(NewPassword!);
	}
}
