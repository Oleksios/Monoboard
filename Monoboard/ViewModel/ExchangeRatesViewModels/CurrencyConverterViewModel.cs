using Monoboard.Helpers.Command;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Monoboard.ViewModel.ExchangeRatesViewModels
{
	public class CurrencyConverterViewModel : Helpers.ViewModel
	{
		#region Fields

		private string? _currencyName;
		private string? _currencyTag;
		private float? _rateBuy;
		private float? _rateSell;
		private float? _rateCross;
		private bool _isFirstCurrencySell;
		private string _result;
		private string _count;

		/// <summary>
		/// Найменування валюти
		/// </summary>
		public string? CurrencyName
		{
			get => _currencyName;
			set
			{
				_currencyName = value;
				OnPropertyChanged(nameof(CurrencyName));
			}
		}

		/// <summary>
		/// Код валюти
		/// </summary>
		public string? CurrencyTag
		{
			get => _currencyTag;
			set
			{
				_currencyTag = value;
				OnPropertyChanged(nameof(CurrencyTag));
			}
		}

		/// <summary>
		/// Курс купівлі
		/// </summary>
		public float? RateBuy
		{
			get => _rateBuy;
			set
			{
				_rateBuy = value;
				OnPropertyChanged(nameof(RateBuy));
			}
		}

		/// <summary>
		/// Курс продажу
		/// </summary>
		public float? RateSell
		{
			get => _rateSell;
			set
			{
				_rateSell = value;
				OnPropertyChanged(nameof(RateSell));
			}
		}

		/// <summary>
		/// Крос-курс
		/// </summary>
		public float? RateCross
		{
			get => _rateCross;
			set
			{
				_rateCross = value;
				OnPropertyChanged(nameof(RateCross));
			}
		}

		/// <summary>
		/// Визначає яку валюту ми продаємо
		/// </summary>
		public bool IsFirstCurrencySell
		{
			get => _isFirstCurrencySell;
			set
			{
				_isFirstCurrencySell = value;
				OnPropertyChanged(nameof(IsFirstCurrencySell));
			}
		}

		/// <summary>
		/// Кількість одиниць валюти (тільки для валідації даних)
		/// </summary>
		public string Count
		{
			get => _count;
			set
			{
				_count = value;
				OnPropertyChanged(nameof(Count));
			}
		}

		/// <summary>
		/// Результат конвертації валюти
		/// </summary>
		public string Result
		{
			get => _result;
			set
			{
				_result = value;
				OnPropertyChanged(nameof(Result));
			}
		}

		#endregion

		#region Command

		public ICommand CurrencyCalculateCommand { get; set; }

		#endregion

		public CurrencyConverterViewModel()
		{
			_isFirstCurrencySell = true;
			var regex = new Regex(@"^\d+[\.\,]?\d*$");

			CurrencyCalculateCommand = new DelegateCommand(o =>
			{
				if (string.IsNullOrEmpty(o.ToString()) is false ||
					string.IsNullOrWhiteSpace(o.ToString()) is false)
				{
					if (regex.Match(o.ToString()!).Success)
					{
						if (o.ToString()!.Contains("."))
							o = o.ToString()!.Replace('.', ',');

						Calculate(float.Parse(o.ToString()!));
					}
				}
				else
					Calculate(0);
			});
		}

		/// <summary>
		/// Встановлює значення для конвертера валюти
		/// </summary>
		/// <param name="currencyName">Найменування валюти</param>
		/// <param name="currencyTag">Код валюти</param>
		/// <param name="rateBuy">Курс купівлі</param>
		/// <param name="rateSell">Курс продажу</param>
		public void SetValue(string currencyName, string currencyTag, float? rateBuy, float? rateSell)
		{
			ClearValue();

			CurrencyName = currencyName;
			CurrencyTag = currencyTag;
			RateBuy = rateBuy;
			RateSell = rateSell;

			Count = "1";

			Calculate(1);
		}

		/// <summary>
		/// Встановлює значення для конвертера валюти
		/// </summary>
		/// <param name="currencyName">Найменування валюти</param>
		/// <param name="currencyTag">Код валюти</param>
		/// <param name="rateCross">Крос-курс</param>
		public void SetValue(string currencyName, string currencyTag, float? rateCross)
		{
			ClearValue();

			CurrencyName = currencyName;
			CurrencyTag = currencyTag;
			RateCross = rateCross;

			Count = "1";

			Calculate(1);
		}

		/// <summary>
		/// Конвертація однієї валюти до іншої з урахуванням курсу
		/// </summary>
		/// <param name="count">Кількість одиниць валюти</param>
		private void Calculate(float count)
		{
			if (RateSell.HasValue && RateBuy.HasValue)
			{
				if (IsFirstCurrencySell)
				{
					if (count != 0)
					{
						if (RateCross.HasValue)
							Result = (count * RateBuy.Value).ToString("0.####", App.Language);
					}
					else if (RateCross.HasValue) Result = "0";
				}
				else
				{
					if (count != 0)
					{
						if (RateCross.HasValue)
							Result = (count / RateSell.Value).ToString("0.####", App.Language);
					}
					else if (RateCross.HasValue)
						Result = "0";
				}
			}
			else
			{
				if (IsFirstCurrencySell)
				{
					if (count != 0)
					{
						if (RateCross.HasValue)
							Result = (count * RateCross.Value).ToString("0.####", App.Language);
					}
					else if (RateCross.HasValue) Result = "0";
				}
				else
				{
					if (count != 0)
					{
						if (RateCross.HasValue)
							Result = (count / RateCross.Value).ToString("0.####", App.Language);
					}
					else if (RateCross.HasValue)
						Result = "0";
				}
			}
		}

		/// <summary>
		/// Встановлює значення за замовчуванням (очищення)
		/// </summary>
		private void ClearValue()
		{
			CurrencyName = string.Empty;
			CurrencyTag = string.Empty;
			RateBuy = null;
			RateSell = null;
			RateCross = 0;
			IsFirstCurrencySell = true;
			Count = string.Empty;
			Result = string.Empty;
		}
	}
}
