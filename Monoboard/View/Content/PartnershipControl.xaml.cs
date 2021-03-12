using MaterialDesignExtensions.Controls;
using Monoboard.ViewModel;
using MonoboardCore.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Monoboard.View.Content
{
	public partial class PartnershipControl
	{
		public PartnershipControl()
		{
			InitializeComponent();

			Loaded += async delegate
			{
				var viewModel = (PartnershipViewModel)DataContext;

				viewModel.IsBusy = true;

				if (viewModel.IsLoading is false)
				{
					viewModel.CancellationToken.Cancel();
					viewModel.CancellationToken.Dispose();
					viewModel.CancellationToken = new CancellationTokenSource();
					viewModel.Partners.Clear();

					await Task.Delay(300);

					if (viewModel.PartnersList != null)
						await viewModel.Show(viewModel.PartnersList, viewModel.CancellationToken.Token);
				}
			};
		}

		private async void SearchHandler_OnSearch(object sender, SearchEventArgs args)
		{
			var viewModel = (PartnershipViewModel)DataContext;

			viewModel.Partners.Clear();

			if (!string.IsNullOrEmpty(args.SearchTerm) || !string.IsNullOrWhiteSpace(args.SearchTerm))
			{
				var searchList = new List<Partner>();

				searchList.AddRange(viewModel.PartnersList
					.Where(partnerItem => partnerItem.Title.ToLower().Contains(args.SearchTerm.ToLower()))
					.Select(partner => partner)
					.ToList());

				await viewModel.Show(searchList, viewModel.CancellationToken.Token);
			}
			else
				await viewModel.Show(viewModel.PartnersList, viewModel.CancellationToken.Token);
		}
	}
}
