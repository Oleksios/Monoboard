using Monoboard.ViewModel.SettingsViewModels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Monoboard.View.Content.Card
{
	public partial class BankCardEditTemplate
	{
		public string CardNameValue { get; set; }
		public string CardDescriptionValue { get; set; }
		public string CardCode { get; set; }

		public BankCardEditTemplate(
			ImageBrush imageBrush,
			LinearGradientBrush background,
			Brush textForeground,
			Brush textBackground,
			string cardCode,
			string cardName,
			string cardDescription)
		{
			InitializeComponent();

			var viewmodel = new CardEditViewModel();

			CardName.DataContext = viewmodel;
			CardDescription.DataContext = viewmodel;

			CardCode = cardCode;

			Panel.Background = imageBrush;
			Card.Background = background;

			CardName.Foreground = textForeground;
			CardName.BorderBrush = textBackground;

			CardDescription.Foreground = textForeground;
			CardDescription.BorderBrush = textBackground;

			CardName.Text = CardNameValue = cardName;
			CardDescription.Text = CardDescriptionValue = cardDescription;
		}

		private void DiscardChanges_OnClick(object sender, RoutedEventArgs e)
		{
			CardName.Text = CardNameValue;
			CardDescription.Text = CardDescriptionValue;
		}

		private async void SaveChanges_OnClick(object sender, RoutedEventArgs e)
		{
			var isSaveCard = await ((SettingsViewModel)DataContext).SaveCardDataAsync(
				CardCode,
				CardName.Text,
				CardDescription.Text);

			Transitioner.SelectedIndex = isSaveCard ? 1 : 2;

			await Task.Delay(3000);

			Transitioner.SelectedIndex = 0;

			await Task.Delay(500);

			await ((SettingsViewModel) DataContext).LoadCard();
		}

		private void CardName_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			ToDefaultButton.IsEnabled = ((TextBox)sender).Text != CardNameValue;
			SaveButton.IsEnabled = ((TextBox)sender).Text != CardNameValue;

			((CardEditViewModel) CardDescription.DataContext).CardCustomizationName = ((TextBox) sender).Text;
		}

		private void CardDescription_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			ToDefaultButton.IsEnabled = ((TextBox)sender).Text != CardDescriptionValue;
			SaveButton.IsEnabled = ((TextBox)sender).Text != CardDescriptionValue;

			((CardEditViewModel)CardName.DataContext).CardCustomizationDescription = ((TextBox)sender).Text;
		}
	}
}
