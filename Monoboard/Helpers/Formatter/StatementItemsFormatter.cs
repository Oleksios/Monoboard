using Monoboard.Properties;
using MonoboardCore.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Monoboard.Helpers.Formatter
{
	public static class StatementItemsFormatter
	{
		/// <summary>
		/// Приводить сирі дані виписки що надійшла від API Monobank до читабельного вигляду
		/// </summary>
		/// <param name="currencyCode">Код валюти картки з якої здійснена операція</param>
		/// <param name="statementItems">Дані виписки</param>
		/// <returns>Модифіковані дані виписки в читабельному вигляді</returns>
		public static IList<StatementItem> Decorate(int currencyCode, IList<StatementItem> statementItems)
		{
			var (_, cultureName) = GetCultureByCyrrencyCode(currencyCode);
			foreach (var statementItem in statementItems)
			{
				var dateTime = MonoboardCore.Hepler.UnixConverter
					.UnixTimestampToDateTime(statementItem.Time).ToLocalTime();

				statementItem.FormatDate = dateTime.ToShortDateString();

				statementItem.FormatTime = dateTime.ToShortTimeString();

				statementItem.BalanceFormat = decimal
					.Parse((double.Parse(statementItem.Balance.ToString("F2")) / 100)
						.ToString(new CultureInfo(cultureName)))
					.ToString("C2", new CultureInfo(cultureName));

				statementItem.AmountFormat = decimal
					.Parse((double.Parse(statementItem.Amount.ToString("F2")) / 100)
						.ToString(new CultureInfo(cultureName)))
					.ToString("C2", new CultureInfo(cultureName));

				statementItem.CashbackAmountFormat = decimal
					.Parse((double.Parse(statementItem.CashbackAmount.ToString("F2")) / 100)
						.ToString(new CultureInfo(cultureName)))
					.ToString("C2", new CultureInfo(cultureName));

				statementItem.CommissionRateFormat = decimal
					.Parse((double.Parse(statementItem.CommissionRate.ToString("F2")) / 100)
						.ToString(new CultureInfo(cultureName)))
					.ToString("C2", new CultureInfo(cultureName));

				var (isDeafult, cultureTransaction) = GetCultureByCyrrencyCode(statementItem.CurrencyCode);

				statementItem.OperationAmountFormat = isDeafult
					? decimal
						.Parse((double.Parse(statementItem.OperationAmount.ToString("F2")) / 100).ToString("F2"))
						.ToString("C2", new CultureInfo(cultureTransaction))
					: string.IsNullOrEmpty(cultureTransaction) is false
						? statementItem.OperationAmountFormat =
							double.Parse(statementItem.OperationAmount.ToString("F2", CultureInfo.InvariantCulture)) /
							100 + " " + cultureTransaction
						: statementItem.OperationAmountFormat =
							(double.Parse(statementItem.OperationAmount.ToString("F2", CultureInfo.InvariantCulture)) /
							 100).ToString(CultureInfo
								.InvariantCulture);
			}

			return statementItems;
		}

		/// <summary>
		/// Отримуємо CultureInfo.Name по коду валюти
		/// </summary>
		/// <param name="currencyCode">Код валюти</param>
		/// <returns>Стандартний / нестандартний символ та CultureInfo.Name </returns>
		private static (bool isDefault, string currencyCultureOrSymbol) GetCultureByCyrrencyCode(int currencyCode)
		{
			var culture = currencyCode switch
			{
				980 => "uk-UA",
				840 => "en-US",
				968 => "en-US",
				516 => "en-US",
				124 => "en-CA",
				096 => "ms-BN",
				036 => "en-AU",
				032 => "es-AR",
				978 => "nl-BE",
				985 => "pl-PL",
				643 => "ru-RU",
				784 => "ar-AE",
				051 => "hy-AM",
				944 => "az-Cyrl-AZ",
				050 => "bn-BD",
				048 => "ar-BH",
				986 => "pt-BR",
				072 => "tn-BW",
				933 => "be-BY",
				977 => "hr-BA",
				975 => "bg-BG",
				068 => "es-BO",
				084 => "en-BZ",
				976 => "fr-CD",
				756 => "de-LI",
				152 => "es-CL",
				156 => "zh-CN",
				170 => "es-CO",
				188 => "es-CR",
				132 => "en-US",
				203 => "cs-CZ",
				208 => "da-DK",
				214 => "es-DO",
				012 => "ar-DZ",
				818 => "ar-EG",
				232 => "ti-ER",
				230 => "om-ET",
				826 => "en-GB",
				981 => "ka-GE",
				320 => "es-GT",
				344 => "zh-HK",
				340 => "es-HN",
				191 => "hr-HR",
				348 => "hu-HU",
				360 => "id-ID",
				376 => "he-IL",
				356 => "hi-IN",
				368 => "ar-IQ",
				364 => "fa-IR",
				400 => "ar-JO",
				392 => "ja-JP",
				404 => "sw-KE",
				116 => "km-KH",
				410 => "ko-KR",
				414 => "ar-KW",
				398 => "kk-KZ",
				422 => "ar-LB",
				144 => "si-LK",
				434 => "ar-LY",
				504 => "ar-MA",
				498 => "ro-MD",
				807 => "mk-MK",
				104 => "my-MM",
				446 => "zh-MO",
				484 => "es-MX",
				458 => "ms-MY",
				566 => "ig-NG",
				558 => "es-NI",
				578 => "nn-NO",
				524 => "ne-NP",
				554 => "en-NZ",
				512 => "ar-OM",
				590 => "es-PA",
				604 => "es-PE",
				608 => "en-PH",
				586 => "ur-PK",
				600 => "gn-PY",
				634 => "ar-QA",
				682 => "ar-SA",
				752 => "se-SE",
				702 => "zg-SG",
				706 => "so-SO",
				760 => "ar-SY",
				764 => "th-TH",
				788 => "ar-TN",
				949 => "tr-TR",
				780 => "en-TT",
				901 => "zh-TW",
				858 => "es-UY",
				860 => "uz-Cyrl-UZ",
				704 => "vi-VN",
				886 => "ar-YE",
				710 => "af-ZA",
				_ => ""
			};

			if (culture == "")
			{
				var altCulture = currencyCode switch
				{
					108 => "₣",
					262 => "₣",
					936 => "GH₵",
					324 => "₣",
					352 => "kr",
					388 => "J$",
					174 => "₣",
					969 => "Ar",
					480 => "¢",
					943 => "MT",
					946 => "L",
					941 => "дин",
					646 => "₣",
					938 => "£",
					776 => "T$",
					834 => "TSh",
					800 => "USh",
					937 => "B$",
					950 => "₣",
					894 => "K",
					932 => "$",
					929 => "UM",
					934 => "T",
					967 => "K",
					_ => ""
				};

				return (false, altCulture);
			}

			return (true, culture);
		}

		/// <summary>
		/// Надає повний і короткий опис по MCC для виписки
		/// </summary>
		/// <param name="statementItems">Дані виписки до яких застосовуватиметься опис</param>
		/// <returns>Модифіковані дані виписки які вже деталізовані</returns>
		public static IList<StatementItem> MccDescriptor(IEnumerable<StatementItem> statementItems)
		{
			var mccHelper = LoadMcc();

			foreach (var statementItem in statementItems)
			{
				var mcc = mccHelper.Single(helper => helper.Mcc == statementItem.Mcc.ToString("0000"));

				statementItem.MccDescription = Settings.Default.Language.Name switch
				{
					"uk-UA" => mcc.ShortDescription.Uk,
					"en-US" => mcc.ShortDescription.En,
					_ => mcc.ShortDescription.Ru
				};

				statementItem.MccFullDescription = Settings.Default.Language.Name switch
				{
					"uk-UA" => mcc.FullDescription.Uk,
					"en-US" => mcc.FullDescription.En,
					_ => mcc.FullDescription.Ru
				};
			}

			return statementItems.ToList();
		}

		/// <summary>
		/// Завантажує json з даними про MCC
		/// </summary>
		private static List<MccHelper> LoadMcc() =>
			JsonConvert.DeserializeObject<List<MccHelper>>(
				new StreamReader(@"Resources/mcc.json").ReadToEnd());
	}

	public class Description
	{
		[JsonProperty("uk")]
		public string Uk { get; set; } = null!;

		[JsonProperty("en")]
		public string En { get; set; } = null!;

		[JsonProperty("ru")]
		public string Ru { get; set; } = null!;
	}

	public class MccHelper
	{
		[JsonProperty("mcc")]
		public string Mcc { get; set; } = null!;

		[JsonProperty("fullDescription")]
		public Description FullDescription { get; set; } = null!;

		[JsonProperty("shortDescription")]
		public Description ShortDescription { get; set; } = null!;
	}


}
