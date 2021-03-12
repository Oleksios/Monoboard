using MaterialDesignExtensions.Controls;
using Monoboard.ViewModel.ExchangeRatesViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Monoboard.View.Content
{
	public partial class ExchangesControl
	{
		public ExchangesControl() => InitializeComponent();

		private void SearchHandler_OnSearch(object sender, SearchEventArgs args)
		{
			if (!string.IsNullOrEmpty(args.SearchTerm) || !string.IsNullOrWhiteSpace(args.SearchTerm))
				((ExchangesViewModel)DataContext).OtherCurrencies = new ObservableCollection<CurrencyVisualize>(
					((ExchangesViewModel)DataContext).OtherCurrenciesBase
					.Where(currency => currency.CurrencyNameLocalized!.ToLower()
						.Contains(args.SearchTerm.ToLower()))
					.Select(currency => currency)
					.ToList());
			else
				((ExchangesViewModel)DataContext).OtherCurrencies = ((ExchangesViewModel)DataContext).OtherCurrenciesBase;
		}

		private void RefreshButton_OnClick(object sender, RoutedEventArgs e) =>
			PersistentSearch.SearchTerm = string.Empty;

		private async void SecondaryCurrencyList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (SecondaryCurrencyList.SelectedIndex == -1) return;

			if (SecondaryCurrencyList.Items[SecondaryCurrencyList.SelectedIndex] is CurrencyVisualize item)
			{
				await ((ExchangesViewModel)DataContext).OpenConverter(
					DialogHostConverter,
					(DataTemplate)FindResource("CurrencyConverterViewModelTemplate"),
					item.CurrencyNameLocalized!,
					item.CurrencyNameLocalizedShort!,
					item.RateCross);

				SecondaryCurrencyList.SelectedIndex = -1;
			}
		}

		private async void MainCurrencyList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (MainCurrencyList.SelectedIndex == -1) return;

			if (MainCurrencyList.Items[MainCurrencyList.SelectedIndex] is CurrencyVisualize item)
			{
				await((ExchangesViewModel)DataContext).OpenConverter(
					DialogHostConverter,
					(DataTemplate)FindResource("CurrencyConverterViewModelTemplate"),
					item.CurrencyNameLocalized!,
					item.CurrencyNameLocalizedShort!,
					item.RateBuy,
					item.RateSell);

				MainCurrencyList.SelectedIndex = -1;
			}
		}
	}
}
