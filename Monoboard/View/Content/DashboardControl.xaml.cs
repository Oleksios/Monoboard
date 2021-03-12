using Monoboard.Helpers.Miscellaneous;
using Monoboard.ViewModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Monoboard.View.Content
{
	public partial class DashboardControl
	{
		private bool IsContentLoaded { get; set; }

		public DashboardControl()
		{
			InitializeComponent();

			Loaded += async delegate
			{
				var viewModel = (DashboardViewModel)DataContext;

				viewModel.CancellationToken.Cancel();
				viewModel.CancellationToken.Dispose();
				viewModel.CancellationToken = new CancellationTokenSource();

				CardListTransitioner.SelectedIndex = viewModel.SelectedIndex;

				SetDefaultState();

				viewModel.StatementItemsCollection.Clear();

				viewModel.IsBusy = true;

				await Task.Delay(500);

				viewModel.UpdateData();

				await viewModel.ShowResult(viewModel.StatementItemList?[viewModel.SelectedIndex], viewModel.CancellationToken.Token);

				if (IsContentLoaded is false) IsContentLoaded = true;
			};
		}

		private async void NextButton_OnClick(object sender, RoutedEventArgs e)
		{
			var viewModel = (DashboardViewModel)DataContext;

			viewModel.SelectedIndex++;

			SetDefaultState();

			viewModel.StatementItemsCollection.Clear();

			viewModel.IsBusy = true;

			await Task.Delay(500);

			await viewModel.ShowResult(viewModel.StatementItemList?[viewModel.SelectedIndex], viewModel.CancellationToken.Token);
		}

		private void SetDefaultState()
		{
			var viewModel = (DashboardViewModel)DataContext;

			if (viewModel == null) return;

			if (viewModel.Cards != null)
				NextButton.Visibility =
					viewModel.SelectedIndex == viewModel.Cards!.Count - 1
						? Visibility.Collapsed
						: Visibility.Visible;


			BackButton.Visibility =
				viewModel.SelectedIndex == 0
					? Visibility.Collapsed
					: Visibility.Visible;
		}

		private async void BackButton_OnClick(object sender, RoutedEventArgs e)
		{
			var viewModel = (DashboardViewModel)DataContext;

			viewModel.SelectedIndex--;

			SetDefaultState();

			viewModel.StatementItemsCollection.Clear();

			viewModel.IsBusy = true;

			await Task.Delay(500);

			await viewModel.ShowResult(viewModel.StatementItemList?[viewModel.SelectedIndex], viewModel.CancellationToken.Token);
		}

		private async void StatementItems_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var listBox = (ListBox)sender;
			if (listBox.SelectedIndex == -1) return;

			if (!(listBox.Items[listBox.SelectedIndex] is CustomView item)) return;

			await ((DashboardViewModel)DataContext).OpenConverter(
				DialogHost,
				(DataTemplate)FindResource("StatementInfoViewModelTemplate"),
				item.StatementItem);

			listBox.SelectedIndex = -1;
		}

		private void CardListTransitioner_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsContentLoaded) SetDefaultState();
		}
	}
}
