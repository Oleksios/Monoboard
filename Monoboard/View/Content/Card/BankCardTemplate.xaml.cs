using MonoboardCore.Model;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Monoboard.View.Content.Card
{
	public partial class BankCardTemplate
	{
		public string Balance { get; set; }
		public string CreditLimit { get; set; }
		public string[]? CardNumberList { get; set; }
		public string? CardMaskedPan { get; set; }
		public string CardNameTextBlock { get; set; }
		public string CardDescriptionTooltip { get; set; }
		public string CardTypeValue { get; set; }
		public string Iban { get; set; }

		public BankCardTemplate(
			string balance,
			string creditLimit,
			string cardMaskedPan,
			string cardTypeName,
			string cardDescription,
			string cardTypeValue,
			string iban)
		{
			Balance = balance;
			CreditLimit = creditLimit;
			CardMaskedPan = cardMaskedPan;
			CardNameTextBlock = cardTypeName;
			CardDescriptionTooltip = cardDescription;
			CardTypeValue = cardTypeValue;
			Iban = iban;

			InitializeComponent();

			LoadContent();
		}

		public BankCardTemplate(
			string balance,
			string creditLimit,
			string[] cardNumberList,
			string cardTypeName,
			string cardDescription,
			string cardTypeValue,
			string iban)

		{
			Balance = balance;
			CreditLimit = creditLimit;
			CardNumberList = cardNumberList;
			CardNameTextBlock = cardTypeName;
			CardDescriptionTooltip = cardDescription;
			CardTypeValue = cardTypeValue;
			Iban = iban;

			InitializeComponent();

			LoadContent();
		}

		private void LoadContent()
		{
			CardSystem cardSystem;

			if (CardNumberList != null && CardNumberList!.Any())
			{
				CardListBox.ItemsSource = CardNumberList;
				CardListBox.Visibility = Visibility.Visible;
				CardNumber.Visibility = Visibility.Collapsed;
				CardListBox.SelectedIndex = 0;

				cardSystem = FindType(CardNumberList.First());
			}
			else
			{
				CardListBox.Visibility = Visibility.Collapsed;
				CardNumber.Text = CardMaskedPan;
				cardSystem = FindType(CardMaskedPan!);
			}

			var background = new LinearGradientBrush();
			var secondBackground = new SolidColorBrush();
			Brush textColor = new SolidColorBrush();
			ImageBrush imageBrush = new ImageBrush();
			BitmapImage cardSystemImage = new BitmapImage();

			//if (CardNumberList!.Any()){}
			//	foreach (var item in CardNumberList)
			//		CardListBox.Items.Add(item);
			//else

			MoneyCard.Text = Balance;
			CreditLimitCard.Text = CreditLimit;
			CardTypeName.Text = CardNameTextBlock;
			CardNumber.Text = CardMaskedPan;
			CardIban.Text = Iban;
			Flipper.ToolTip = CardDescriptionTooltip;

			if (CardTypeValue == nameof(CardType.Black).ToLower())
			{
				cardSystemImage = cardSystem == CardSystem.MasterCard
					? new BitmapImage(
						new Uri("pack://application:,,,/Monoboard;component/Resources/Images/Mastercard-Dark.png"))
					: new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/Visa-Dark.png"));

				background = new LinearGradientBrush(
					(Color)ColorConverter.ConvertFromString("#2a2a2a")!,
					(Color)ColorConverter.ConvertFromString("#0e0e0e")!,
					Point.Parse("0,0"),
					Point.Parse("0,1"));

				CardFront.Background = background;
				CardBack.Background = background;

				secondBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#39383d")!);

				GridBackground.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/BackgroundVariant1.png")));

				textColor = (Brush)Application.Current.Resources["MaterialDesignDarkForeground"];

				//Flipper.ToolTip = App.GetResourceValue("MbBlackCardDescription");
			}
			else if (CardTypeValue == nameof(CardType.White).ToLower())
			{
				cardSystemImage = cardSystem == CardSystem.MasterCard
				? new BitmapImage(
					new Uri("pack://application:,,,/Monoboard;component/Resources/Images/Mastercard-Light.png"))
				: new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/Visa-Light.png"));

				background = new LinearGradientBrush(
					(Color)ColorConverter.ConvertFromString("#feffff")!,
					(Color)ColorConverter.ConvertFromString("#e5e8ed")!,
					Point.Parse("0,0"),
					Point.Parse("0,1"));

				CardFront.Background = background;
				CardBack.Background = background;

				secondBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#b3bcc3")!);

				GridBackground.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/BackgroundVariant3.png")));

				textColor = (Brush)Application.Current.Resources["MaterialDesignLightForeground"];
			}
			else if (CardTypeValue == nameof(CardType.Iron).ToLower())
			{
				cardSystemImage = cardSystem == CardSystem.MasterCard
				? new BitmapImage(
					new Uri("pack://application:,,,/Monoboard;component/Resources/Images/Mastercard-Dark.png"))
				: new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/Visa-Dark.png"));

				background = new LinearGradientBrush(
					(Color)ColorConverter.ConvertFromString("#35393c")!,
					(Color)ColorConverter.ConvertFromString("#1c1d1f")!,
					Point.Parse("1,0"),
					Point.Parse("0,1"));

				CardFront.Background = background;
				CardBack.Background = background;

				secondBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#39383d")!);

				GridBackground.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/BackgroundVariant2.png")));

				textColor = (Brush)Application.Current.Resources["MaterialDesignDarkForeground"];
			}
			else if (CardTypeValue == nameof(CardType.Platinum).ToLower())
			{
				cardSystemImage = cardSystem == CardSystem.MasterCard
				? new BitmapImage(
					new Uri("pack://application:,,,/Monoboard;component/Resources/Images/Mastercard-Light.png"))
				: new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/Visa-Light.png"));

				background = new LinearGradientBrush(
					(Color)ColorConverter.ConvertFromString("#e7e7ed")!,
					(Color)ColorConverter.ConvertFromString("#989898")!,
					Point.Parse("1,0"),
					Point.Parse("0,1"));

				CardFront.Background = background;
				CardBack.Background = background;

				secondBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9d9d9d")!);

				GridBackground.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/BackgroundVariant3.png")));

				textColor = (Brush)Application.Current.Resources["MaterialDesignLightForeground"];
			}
			else if (CardTypeValue == nameof(CardType.Yellow).ToLower())
			{
				cardSystemImage = cardSystem == CardSystem.MasterCard
				? new BitmapImage(
					new Uri("pack://application:,,,/Monoboard;component/Resources/Images/Mastercard-Light.png"))
				: new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/Visa-Light.png"));

				background = new LinearGradientBrush(
					(Color)ColorConverter.ConvertFromString("#f4d302")!,
					(Color)ColorConverter.ConvertFromString("#d4b704")!,
					Point.Parse("1,0"),
					Point.Parse("0,1"));

				CardFront.Background = background;
				CardBack.Background = background;

				secondBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#cccccc")!);

				GridBackground.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/BackgroundVariant4.png")));

				textColor = (Brush)Application.Current.Resources["MaterialDesignLightForeground"];
			}
			else if (CardTypeValue == nameof(CardType.Fop).ToLower())
			{
				cardSystemImage = cardSystem == CardSystem.MasterCard
				? new BitmapImage(
					new Uri("pack://application:,,,/Monoboard;component/Resources/Images/Mastercard-Dark.png"))
				: new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/Visa-Dark.png"));

				background = new LinearGradientBrush(
					(Color)ColorConverter.ConvertFromString("#5c677e")!,
					(Color)ColorConverter.ConvertFromString("#444c60")!,
					Point.Parse("1,0"),
					Point.Parse("0,1"));

				CardFront.Background = background;
				CardBack.Background = background;

				secondBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#414a66")!);

				GridBackground.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/BackgroundVariant5.png")));

				textColor = (Brush)Application.Current.Resources["MaterialDesignDarkForeground"];
			}

			MoneyCardTitle.Foreground = MoneyCard.Foreground = textColor;
			CreditLimitCardTitle.Foreground = CreditLimitCard.Foreground = textColor;
			CardTypeName.Foreground = textColor;
			CardListBox.Foreground = textColor;
			CardNumber.Foreground = textColor;
			CardIban.Foreground = textColor;
			CardIbanTitle.Foreground = textColor;
			LogoMono.Foreground = textColor;
			LogoUniversal.Foreground = textColor;
			ContactlessIcon.Foreground = textColor;

			CardListBox.Background = IbanBorder.Background = CardLine.Background = secondBackground;
			CardSystemImage.Source = cardSystemImage;

			if (cardSystem == CardSystem.Visa) CardSystemImage.Margin = new Thickness(0, 0, 24, 32);
		}

		public CardSystem FindType(string cardNumber)
		{
			if (Regex.Match(cardNumber.Substring(0, 4), @"^(?:5[1-5][0-9]{2})$").Success)
				return CardSystem.MasterCard;

			if (int.Parse(cardNumber.Substring(0, 4)) >= 2221
				&& int.Parse(cardNumber.Substring(0, 4)) <= 2720)
				return CardSystem.MasterCard;

			return Regex.Match(cardNumber.Substring(0, 6), @"^4[0-9]{5,}$").Success ? CardSystem.Visa : CardSystem.Unknown;
		}

		private async void CopyIbanButton_OnClick(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText(Iban);

			CopyIbanButton.IsEnabled = false;

			Snackbar.MessageQueue?.Clear();
			Snackbar.MessageQueue?.Enqueue(App.GetResourceValue("MbMessageCopied")!);

			await Task.Delay(1000);

			CopyIbanButton.IsEnabled = true;
		}
	}
}
