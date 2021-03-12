using Monoboard.Helpers.Miscellaneous;
using Monoboard.ViewModel.StatementItemsViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Monoboard.View.Content
{
	public partial class StatementsControl
	{
		private bool isLoaded { get; set; }

		public StatementsControl()
		{
			InitializeComponent();

			DatePicker.Language = XmlLanguage.GetLanguage(Properties.Settings.Default.Language.IetfLanguageTag);

			//Місяць запуску Монобанк
			DatePicker.DisplayDateStart ??= DateTime.Parse("01.10.2017");

			//Теперішній місяць
			DatePicker.DisplayDateEnd ??= DateTime.Now;

			//Теперішня дата
			DatePicker.SelectedDate ??= DateTime.Today;

			Loaded += async delegate
			{
				var viewModel = (StatementItemsViewModel)DataContext;

				viewModel.IsBusy = true;

				viewModel.CancellationToken.Cancel();
				viewModel.CancellationToken.Dispose();
				viewModel.CancellationToken = new CancellationTokenSource();
				viewModel.StatementItemsCollection.Clear();

				await Task.Delay(300);

				await viewModel.UpdateDataAsync();

				await viewModel.ShowResult(
					viewModel.StatementItemList?[viewModel.SelectedIndex],
					viewModel.PageListIndex,
					viewModel.CancellationToken.Token);

				isLoaded = true;
			};
		}

		private async void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!isLoaded) return;
			
			var viewModel = (StatementItemsViewModel)DataContext;

			await Task.Delay(300);

			if ((StatementItemsViewModel)DataContext == null) return;

			if (!viewModel.IsBusy) viewModel.IsBusy = true;
			viewModel.PageListIndex = 0;

			await viewModel.ShowResult(
				viewModel.StatementItemList?[viewModel.SelectedIndex],
				viewModel.PageListIndex,
				viewModel.CancellationToken.Token);
		}

		private async void StatementItems_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var listBox = (ListBox)sender;
			if (listBox.SelectedIndex == -1) return;

			if (!(listBox.Items[listBox.SelectedIndex] is CustomView item)) return;

			await ((StatementItemsViewModel)DataContext).OpenConverter(
				DialogHost,
				(DataTemplate)FindResource("StatementInfoViewModelTemplate"),
				item.StatementItem);

			listBox.SelectedIndex = -1;
		}
	}
}
