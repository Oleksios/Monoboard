using Monoboard.Properties;
using Monoboard.View.Content.Card;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Monoboard.ViewModel.SettingsViewModels
{
	public partial class SettingsViewModel
	{
		#region Fields

		private ObservableCollection<CardSettings> _cards;
		private int _selectedIndexCardSettings;

		public ObservableCollection<CardSettings> Cards
		{
			get => _cards;
			set
			{
				_cards = value;
				OnPropertyChanged(nameof(Cards));
			}
		}

		public int SelectedIndexCardSettings
		{
			get => _selectedIndexCardSettings;
			set
			{
				_selectedIndexCardSettings = value;
				OnPropertyChanged(nameof(SelectedIndexCardSettings));
			}
		}

		#endregion

		/// <summary>
		/// Завантажує дані про картки/рахунки користувача
		/// </summary>
		public async Task LoadCard()
		{
			Cards ??= new ObservableCollection<CardSettings>();

			if (Cards.Any()) Cards.Clear();

			var accounts = await MonoboardCore.Get.GetAccount.GetListAsync(Settings.Default.ClientId);

			foreach (var account in accounts)
			{
				var background = new LinearGradientBrush();
				Brush textColor = new SolidColorBrush();
				Brush textBackground = new SolidColorBrush();
				ImageBrush imageBrush = new ImageBrush();

				if (account.CardType == nameof(MonoboardCore.Model.CardType.Black).ToLower())
				{
					background = new LinearGradientBrush(
						(Color)ColorConverter.ConvertFromString("#2a2a2a")!,
						(Color)ColorConverter.ConvertFromString("#0e0e0e")!,
						Point.Parse("0,0"),
						Point.Parse("0,1"));

					imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/BackgroundVariant1.png"));

					textColor = (Brush)Application.Current.Resources["MaterialDesignDarkForeground"];

					textBackground = (Brush)Application.Current.Resources["MaterialDesignLightBackground"];
				}
				else if (account.CardType == nameof(MonoboardCore.Model.CardType.White).ToLower())
				{
					background = new LinearGradientBrush(
						(Color)ColorConverter.ConvertFromString("#feffff")!,
						(Color)ColorConverter.ConvertFromString("#e5e8ed")!,
						Point.Parse("0,0"),
						Point.Parse("0,1"));

					imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/BackgroundVariant3.png"));

					textColor = (Brush)Application.Current.Resources["MaterialDesignLightForeground"];

					textBackground = (Brush)Application.Current.Resources["MaterialDesignDarkBackground"];
				}
				else if (account.CardType == nameof(MonoboardCore.Model.CardType.Iron).ToLower())
				{
					background = new LinearGradientBrush(
						(Color)ColorConverter.ConvertFromString("#35393c")!,
						(Color)ColorConverter.ConvertFromString("#1c1d1f")!,
						Point.Parse("1,0"),
						Point.Parse("0,1"));

					imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/BackgroundVariant2.png"));

					textColor = (Brush)Application.Current.Resources["MaterialDesignLightForeground"];

					textBackground = (Brush)Application.Current.Resources["MaterialDesignDarkBackground"];
				}
				else if (account.CardType == nameof(MonoboardCore.Model.CardType.Platinum).ToLower())
				{
					background = new LinearGradientBrush(
						(Color)ColorConverter.ConvertFromString("#e7e7ed")!,
						(Color)ColorConverter.ConvertFromString("#989898")!,
						Point.Parse("1,0"),
						Point.Parse("0,1"));

					imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/BackgroundVariant3.png"));

					textColor = (Brush)Application.Current.Resources["MaterialDesignLightForeground"];

					textBackground = (Brush)Application.Current.Resources["MaterialDesignDarkBackground"];
				}
				else if (account.CardType == nameof(MonoboardCore.Model.CardType.Yellow).ToLower())
				{
					background = new LinearGradientBrush(
						(Color)ColorConverter.ConvertFromString("#f4d302")!,
						(Color)ColorConverter.ConvertFromString("#d4b704")!,
						Point.Parse("1,0"),
						Point.Parse("0,1"));

					imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/BackgroundVariant4.png"));

					textColor = (Brush)Application.Current.Resources["MaterialDesignLightForeground"];

					textBackground = (Brush)Application.Current.Resources["MaterialDesignDarkBackground"];
				}
				else if (account.CardType == nameof(MonoboardCore.Model.CardType.Fop).ToLower())
				{
					background = new LinearGradientBrush(
						(Color)ColorConverter.ConvertFromString("#5c677e")!,
						(Color)ColorConverter.ConvertFromString("#444c60")!,
						Point.Parse("1,0"),
						Point.Parse("0,1"));

					imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Monoboard;component/Resources/Images/BackgroundVariant5.png"));

					textColor = (Brush)Application.Current.Resources["MaterialDesignDarkForeground"];

					textBackground = (Brush)Application.Current.Resources["MaterialDesignLightBackground"];
				}

				var cardEditItem = new BankCardEditTemplate(imageBrush,
						background,
						textColor,
						textBackground,
						account.CardCode,
						account.CustomCardName,
						account.CustomCardDescription)
				{ DataContext = this };

				Cards.Add(new CardSettings
				{
					Header = account.CustomCardName,
					Card = cardEditItem
				});
			}
		}

		/// <summary>
		/// Викликає функцію збереження оновлених даних карток/рахунків користувача
		/// </summary>
		/// <param name="cardCode">Унікальний код картки</param>
		/// <param name="cardName">Оновлене найменування картки/рахунку</param>
		/// <param name="cardDescription">Оновлений опис картки/рахунку</param>
		/// <returns></returns>
		public async Task<bool> SaveCardDataAsync(string cardCode, string cardName, string cardDescription) =>
			await MonoboardCore.Set.SetAccount.SetCardPersonalizationAsync(
				Settings.Default.ClientId,
				cardCode,
				cardName,
				cardDescription);
	}

	public class CardSettings
	{
		public string? Header { get; set; }
		public UserControl? Card { get; set; }
	}
}
