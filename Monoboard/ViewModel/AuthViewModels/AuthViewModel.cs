#nullable enable
using MaterialDesignExtensions.Controls;
using MaterialDesignThemes.Wpf;
using Monoboard.Helpers.Command;
using Monoboard.Helpers.Formatter;
using Monoboard.Helpers.Validation;
using Monoboard.Properties;
using MonoboardCore.Model;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonoboardCore.Hepler;

namespace Monoboard.ViewModel.AuthViewModels
{
	public class AuthViewModel : Helpers.ViewModel
	{
		#region Fields

		// private fields
		private readonly DialogHost _dialogHost;
		private int _transitionIndex;
		private int _registerTransitionerIndex;
		private string? _continueAsUser;
		private Visibility _applyUserVisibility;
		private string _monobankApiValue;
		private bool _isPasswordCorrect;
		private SecureString _confirmSecurePassword = new SecureString();
		private string? _passwordValidate;
		private SecureString _securePassword = new SecureString();
		private string? _confirmSecurePasswordValidate;
		private string? _loginValidate;
		private bool _isLoginPasswordCorrect;
		private Visibility _isLoginValidateBlockVisible;
		private SnackbarMessageQueue _messages;

		// public fields
		public int SelectedUserIndex { get; set; }

		public ObservableCollection<UserInfo> UserList { get; set; }

		public int TransitionIndex
		{
			get => _transitionIndex;
			set
			{
				_transitionIndex = value;
				OnPropertyChanged(nameof(TransitionIndex));
			}
		}

		public int RegisterTransitionerIndex
		{
			get => _registerTransitionerIndex;
			set
			{
				_registerTransitionerIndex = value;
				OnPropertyChanged(nameof(RegisterTransitionerIndex));
			}
		}

		public string? ContinueAsUser
		{
			get => _continueAsUser;
			set
			{
				_continueAsUser = value;
				OnPropertyChanged(nameof(ContinueAsUser));
			}
		}

		public Visibility ApplyUserVisibility
		{
			get => _applyUserVisibility;
			set
			{
				_applyUserVisibility = value;
				OnPropertyChanged(nameof(ApplyUserVisibility));
			}
		}

		public string MonobankApiValue
		{
			get => _monobankApiValue;
			set
			{
				_monobankApiValue = value;
				OnPropertyChanged(nameof(MonobankApiValue));
			}
		}

		public SecureString SecurePassword
		{
			private get => _securePassword;
			set
			{
				_securePassword = value;

				IsPasswordCorrect = PasswordValidation.Compare(ConfirmSecurePassword, SecurePassword) &&
									PasswordValidate == App.GetResourceValue("MbPasswordSuccess");
			}
		}

		public SecureString ConfirmSecurePassword
		{
			private get => _confirmSecurePassword;
			set
			{
				_confirmSecurePassword = value;

				IsPasswordCorrect = PasswordValidation.Compare(SecurePassword, ConfirmSecurePassword) &&
									ConfirmPasswordValidate == App.GetResourceValue("MbPasswordSuccess");
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

		public string? LoginValidate
		{
			get => _loginValidate;
			set
			{
				_loginValidate = value;
				OnPropertyChanged(nameof(LoginValidate));
			}
		}

		public bool IsLoginPasswordCorrect
		{
			get => _isLoginPasswordCorrect;
			set
			{
				_isLoginPasswordCorrect = value;
				OnPropertyChanged(nameof(IsLoginPasswordCorrect));
			}
		}

		public Visibility IsLoginValidateBlockVisible
		{
			get => _isLoginValidateBlockVisible;
			set
			{
				_isLoginValidateBlockVisible = value;
				OnPropertyChanged(nameof(IsLoginValidateBlockVisible));
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

		public AuthViewModel(DialogHost dialogHost)
		{

			Messages ??= new SnackbarMessageQueue();

			_dialogHost = dialogHost;
			_applyUserVisibility = Visibility.Collapsed;

			SelectUserCommand = new DelegateCommand(o => SelectUser((int)o));
			NextUserCommand = new DelegateCommand(o => ApplyUser((int)o));
			LoginCommand = new DelegateCommand(o => Login(((PasswordBox)o).Password));
			SaveMonobankApiCommand = new DelegateCommand(o => NextStep((string)o));
			AddNewUserCommand = new DelegateCommand(o => AddNewUser(((PasswordBox)o).Password));

			ContinueAsUser = App.GetResourceValue("MbSelectUsersAccount");

			UserList = MonoboardCore.Get.GetUserInfo.GetUserList(true);

			ApplyUserVisibility = UserList.Any() ? Visibility.Visible : Visibility.Collapsed;
		}

		#region Commands

		public DelegateCommand SelectUserCommand { get; set; }
		public DelegateCommand NextUserCommand { get; set; }
		public DelegateCommand LoginCommand { get; set; }
		public DelegateCommand SaveMonobankApiCommand { get; set; }
		public DelegateCommand AddNewUserCommand { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Відображає особу, яку було вибрано
		/// </summary>
		/// <param name="selectedIndex">Порядковий номер особи</param>
		private void SelectUser(int selectedIndex) =>
			ContinueAsUser = $"{App.GetResourceValue("MbContinueAs")} {UserList[selectedIndex].Name}";

		private async void NextStep(string token)
		{
			if (await MonoboardCore.Get.GetMonoboardUser.GetUserExistByToken(token))
			{
				await UserExistInfoAsync();

				TransitionIndex = 1;
			}
			else
			{
				MonobankApiValue = token;

				if (await SetPasswordDialogAsync())
					TransitionIndex = 3;
				else
				{
					TransitionIndex = 5;

					AddNewUser();
				}
			}
		}

		private async void Login(string password)
		{
			if (await MonoboardCore.Get.GetMonoboardUser.CheckIsResetPasswordAsync(UserList[SelectedUserIndex].ClientId))
			{
				if (await MonoboardCore.Get.GetMonoboardUser.CheckPassword(UserList[SelectedUserIndex]
						.ClientId,
					Sha256Hash.Compute($"user{UserList[SelectedUserIndex].ClientId}!")))
					LoginApp();
				else
				{
					IsLoginValidateBlockVisible = Visibility.Visible;
					LoginValidate = App.GetResourceValue("MbPasswordErrorAlt");
				}
			}
			else if (await MonoboardCore.Get.GetMonoboardUser.CheckPassword(UserList[SelectedUserIndex]
					.ClientId,
				Sha256Hash.Compute(
					password + UserList[SelectedUserIndex].ClientId)))
				LoginApp();
			else
			{
				IsLoginValidateBlockVisible = Visibility.Visible;
				LoginValidate = App.GetResourceValue("MbPasswordErrorAlt");
			}

			void LoginApp()
			{
				Settings.Default.ClientId = UserList[SelectedUserIndex].ClientId;
				Settings.Default.UserName = UserList[SelectedUserIndex].Name;
				Settings.Default.Language = new CultureInfo(UserList[SelectedUserIndex].MonoboardUser.Language);
				Settings.Default.MonobankToken = UserList[SelectedUserIndex].MonoboardUser.AccessToken;
				Settings.Default.Save();

				System.Diagnostics.Process.Start(
					Application.ResourceAssembly.Location.Replace(
						"Monoboard.dll",
						"Monoboard.exe"));

				Application.Current.Shutdown();
			}
		}

		/// <summary>
		/// Здійснює вхід за вибраного користувача
		/// </summary>
		/// <param name="selectedUser">Порядковий номер вибраного користувача</param>
		private async void ApplyUser(int selectedUser)
		{
			if (selectedUser != -1)
			{
				SelectedUserIndex = selectedUser;

				if (await MonoboardCore.Get.GetMonoboardUser.GetPasswordExistAsync(UserList[SelectedUserIndex].ClientId))
					TransitionIndex = 4;
				else
				{
					Settings.Default.ClientId = UserList[selectedUser].ClientId;
					Settings.Default.UserName = UserList[selectedUser].Name;
					Settings.Default.Language = new CultureInfo(UserList[selectedUser].MonoboardUser.Language);
					Settings.Default.MonobankToken = UserList[selectedUser].MonoboardUser.AccessToken;
					Settings.Default.Save();

					System.Diagnostics.Process.Start(
						Application.ResourceAssembly.Location.Replace(
							"Monoboard.dll",
							"Monoboard.exe"));

					Application.Current.Shutdown();
				}
			}
		}

		/// <summary>
		/// Запит на встановлення паролю
		/// </summary>
		/// <returns>Пароль потрібно / не потрібно встановлювати</returns>
		private async Task<bool> SetPasswordDialogAsync()
		{
			var dialogArgs = new ConfirmationDialogArguments
			{
				Title = App.GetResourceValue("MbIsSetPassword"),
				Message = App.GetResourceValue("MbSetPasswordInfoMessages"),
				OkButtonLabel = App.GetResourceValue("MbSetPassword"),
				CancelButtonLabel = App.GetResourceValue("MbNotSetPassword")
			};

			return await ConfirmationDialog.ShowDialogAsync(_dialogHost, dialogArgs);
		}

		/// <summary>
		/// Відображає інформацію про наявність користувача в системі
		/// </summary>
		private async Task UserExistInfoAsync()
		{
			var dialogArgs = new AlertDialogArguments
			{
				Title = App.GetResourceValue("MbInfoMessage"),
				Message = App.GetResourceValue("MbUserExistMessage"),
				OkButtonLabel = "OK"
			};

			await AlertDialog.ShowDialogAsync(_dialogHost, dialogArgs);
		}

		/// <summary>
		/// Додаємо нового користувача в базу даних
		/// </summary>
		/// <param name="password">Поле з паролем користувача</param>
		private async void AddNewUser(string password = "")
		{
			TransitionIndex = 5;

			(UserInfo user, string message) = await Task.Run(() => FillUserInfo(password));

			await Task.Delay(1000);

			if (user != null)
			{
				var userAdded = await MonoboardCore.Set.SetUserInfo.SetNewUserAsync(user);

				Settings.Default.MonobankToken = MonobankApiValue;
				Settings.Default.Language = App.Language;
				Settings.Default.UserName = user.Name;
				Settings.Default.ClientId = user.ClientId;
				Settings.Default.Save();

				RegisterTransitionerIndex = userAdded ? 1 : 2;
			}
			else
			{
				RegisterTransitionerIndex = 2;

				Messages.Clear();

				if (message == "MbNoInternet")
					Messages.Enqueue(App.GetResourceValue("MbNoInternet")!);
				else
					Messages.Enqueue(
						$"{App.GetResourceValue("MbSaveDataError")}" +
						$"\n{App.GetResourceValue("MbCauseInfo")} {message}");
			}
		}

		private async Task<(UserInfo user, string message)> FillUserInfo(string password)
		{
			(UserInfo user, string message) = await MonoboardCore.Get.GetUserInfo.GetClientInfoAsync(MonobankApiValue);

			if (user != null)
			{
				if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password))
					user.MonoboardUser = new MonoboardUser
					{
						ClientId = user.ClientId,
						Password = "",
						IsPasswordReset = false,
						Language = App.Language.Name,
						AccessToken = MonobankApiValue,
						PrimaryColor = "#fff43b3f",
						SecondaryColor = "#ff06b206"
					};
				else
					user.MonoboardUser = new MonoboardUser
					{
						ClientId = user.ClientId,
						Password = Sha256Hash.Compute(password + user.ClientId),
						IsPasswordReset = false,
						Language = App.Language.Name,
						AccessToken = MonobankApiValue,
						PrimaryColor = "#fff43b3f",
						SecondaryColor = "#ff06b206"
					};

				user.Accounts = AccountsDecorator.Decorate(user.Accounts);


				return (user, message);
			}

			if (message == "MbNoInternet")
				return (null, "MbNoInternet")!;

			return (null, message)!;
		}

		#endregion
	}
}
