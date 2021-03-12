using MaterialDesignExtensions.Controls;
using Monoboard.Helpers.Command;
using Monoboard.Properties;
using MonoboardCore.Get;
using QRCoder;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;
using Monoboard.Helpers.Miscellaneous;

namespace Monoboard.ViewModel
{
	class MainWindowViewModel : Helpers.ViewModel
	{
		#region Fields

		private string? _userName;
		private string? _userNameShort;
		private ImageSource _qrCodeImage;
		private BitmapSource? _bitmapSource;
		private string _shareLink;
		private int _qrCodeTransitionerIndex;
		private string _amount;
		private string _comment;
		private SnackbarMessageQueue _messages;
		private bool _isCopyEnable;

		public string? UserName
		{
			get => _userName;
			set
			{
				_userName = value;
				OnPropertyChanged(nameof(UserName));
			}
		}

		public string? UserNameShort
		{
			get => _userNameShort;
			set
			{
				_userNameShort = value;
				OnPropertyChanged(nameof(UserNameShort));
			}
		}

		public int QrCodeTransitionerIndex
		{
			get => _qrCodeTransitionerIndex;
			set
			{
				_qrCodeTransitionerIndex = value;
				OnPropertyChanged(nameof(QrCodeTransitionerIndex));
			}
		}

		public string Amount
		{
			get => _amount;
			set
			{
				_amount = value;
				OnPropertyChanged(nameof(Amount));
			}
		}

		public string Comment
		{
			get => _comment;
			set
			{
				_comment = value;
				OnPropertyChanged(nameof(Comment));
			}
		}

		public ImageSource QrCodeImage
		{
			get => _qrCodeImage;
			set
			{
				_qrCodeImage = value;
				OnPropertyChanged(nameof(QrCodeImage));
			}
		}

		public string ShareLink
		{
			get => _shareLink;
			set
			{
				_shareLink = value;
				OnPropertyChanged(nameof(ShareLink));
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

		public bool IsCopyEnable
		{
			get => _isCopyEnable;
			set
			{
				_isCopyEnable = value;
				OnPropertyChanged(nameof(IsCopyEnable));
			}
		}

		#endregion

		#region Commands

		public ICommand GetQrCodeCommand { get; set; }
		public ICommand GenerateQrCodeCommand { get; set; }
		public ICommand CopyLinkCommand { get; set; }
		public ICommand CopyImageCommand { get; set; }

		#endregion

		public MainWindowViewModel()
		{
			Messages = new SnackbarMessageQueue();
			_isCopyEnable = true;

			GetQrCodeCommand = new DelegateCommand(o => GetQrCodeModal((string)o));
			GenerateQrCodeCommand = new DelegateCommand(o => QrCodeResult());
			CopyLinkCommand = new DelegateCommand(o => CopyLink());
			CopyImageCommand = new DelegateCommand(o => CopyImage());
		}

		#region Methods

		/// <summary>
		/// Відображає користувачу модальне вікно з QR кодом на поповнення рахунку
		/// </summary>
		/// <param name="dialigHostName">Ідентифікатор діалогового вікна</param>
		private async void GetQrCodeModal(string dialigHostName)
		{
			var dataTemplate = Application.Current.FindResource("QrCodeViewModelTemplate") as DataTemplate;

			await AlertDialog.ShowDialogAsync(dialigHostName, new AlertDialogArguments
			{
				Title = App.GetResourceValue("MbRefill"),
				CustomContent = this,
				CustomContentTemplate = dataTemplate,
				OkButtonLabel = App.GetResourceValue("MbClose")
			});

			Amount = string.Empty;
			Comment = string.Empty;
			ShareLink = string.Empty;
			QrCodeTransitionerIndex = 0;
		}

		/// <summary>
		/// Генерація QR-коду
		/// </summary>
		public void QrCodeResult()
		{
			var (link, imageSource) = GenerateQrCode.Generate(Amount, Comment);

			ShareLink = link;
			QrCodeImage = imageSource;
			_bitmapSource = imageSource as BitmapSource;

			QrCodeTransitionerIndex = 1;
		}

		/// <summary>
		/// Отримання і вивід осноної інформації про користувача
		/// </summary>
		public async void GetInfo()
		{
			var user = await GetUserInfo.GetUserAsync(Settings.Default.ClientId);

			UserName = user.Name;
			UserNameShort = user.NameShort;
		}

		/// <summary>
		/// Копіювання зображення QR-коду
		/// </summary>
		private async void CopyImage()
		{
			IsCopyEnable = false;

			Clipboard.SetImage(_bitmapSource);

			Messages.Clear();
			Messages.Enqueue(App.GetResourceValue("MbQrCodeCopied")!);

			await Task.Delay(1000);

			IsCopyEnable = true;
		}

		/// <summary>
		/// Копіювання посилання
		/// </summary>
		private async void CopyLink()
		{
			IsCopyEnable = false;

			Clipboard.SetText(ShareLink);

			Messages.Clear();
			Messages.Enqueue(App.GetResourceValue("MbLinkCopied")!);

			await Task.Delay(1000);

			IsCopyEnable = true;
		}

		#endregion
	}
}
