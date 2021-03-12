namespace Monoboard.ViewModel.SettingsViewModels
{
	public class CardEditViewModel : Helpers.ViewModel
	{
		private string? _cardCustomizationName;
		private string? _cardCustomizationDescription;

		public string? CardCustomizationName
		{
			get => _cardCustomizationName;
			set
			{
				_cardCustomizationName = value;
				OnPropertyChanged(nameof(CardCustomizationName));
			}
		}

		public string? CardCustomizationDescription
		{
			get => _cardCustomizationDescription;
			set
			{
				_cardCustomizationDescription = value;
				OnPropertyChanged(nameof(CardCustomizationDescription));
			}
		}
	}
}
