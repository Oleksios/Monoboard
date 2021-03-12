using Newtonsoft.Json;
using RestEase;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonoboardCore.Model
{
	public interface IStatementItemApi
	{
		[Get("personal/statement/{account}/{from}/{to}")]
		[AllowAnyStatusCode]
		Task<Response<List<StatementItem>>> GetAccountStatementAsync([Header("X-Token")] string xToken,
			[Path("account")] string account,
			[Path("from")] string from,
			[Path("to")] string to);
	}

	public class StatementItem
	{
		/// <summary>
		/// Код картки
		/// </summary>
		public string CardKey { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		/// <summary>
		/// Час транзакції в секундах в форматі Unix time
		/// </summary>
		[JsonProperty("time")]
		public long Time { get; set; }

		/// <summary>
		/// Форматована дата з поля Time відповідно до часової зони користувача (пристрою)
		/// </summary>
		public string FormatDate { get; set; }

		/// <summary>
		/// Форматований час відповідно до часової зони користувача (пристрою)
		/// </summary>
		public string FormatTime { get; set; }

		/// <summary>
		/// Опис транзакцій
		/// </summary>
		[JsonProperty("description")]
		public string Description { get; set; }

		/// <summary>
		/// Коментар користувача
		/// </summary>
		[JsonProperty("comment")]
		public string? Comment { get; set; }

		/// <summary>
		/// Код типу транзакції (Merchant Category Code), відповідно ISO 18245
		/// </summary>
		[JsonProperty("mcc")]
		public int Mcc { get; set; }

		/// <summary>
		/// Локалізоване найменування категорії транзакції
		/// </summary>
		public string MccDescription { get; set; }

		/// <summary>
		/// Локалізований код MCC (скорочена форма)
		/// </summary>
		public string MccFullDescription { get; set; }
		
		/// <summary>
		/// Код квитанції
		/// </summary>
		[JsonProperty("receiptId")]
		public string? Receipt { get; set; }

		/// <summary>
		/// Код (ЄДРПОУ) Єдиного державного реєстру підприємств та організацій України
		/// </summary>
		[JsonProperty("counterEdrpou")]
		public string? Edrpou { get; set; }

		/// <summary>
		/// Міжнародний номер банківського рахунку
		/// </summary>
		[JsonProperty("counterIban")]
		public string? Iban { get; set; }

		/// <summary>
		/// Сума у валюті рахунку в мінімальних одиницях валюти (копійках, центах)
		/// </summary>
		[JsonProperty("amount")]
		public long Amount { get; set; }

		/// <summary>
		/// Форматована сума коштів у транзакції для читабельного вигляду. Наприклад: "-15,35₴", "322,50$", тощо.
		/// </summary>
		public string AmountFormat { get; set; }

		/// <summary>
		/// Сума у валюті транзакції в мінімальних одиницях валюти (копійках, центах)
		/// </summary>
		[JsonProperty("operationAmount")]
		public long OperationAmount { get; set; }

		/// <summary>
		/// Форматована сума коштів в іноземній валюті у транзакції для читабельного вигляду. Наприклад: "-15,35₴", "322,50$", тощо.
		/// </summary>
		public string OperationAmountFormat { get; set; }

		/// <summary>
		/// Код валюти рахунку відповідно ISO 4217
		/// </summary>
		[JsonProperty("currencyCode")]
		public int CurrencyCode { get; set; }

		/// <summary>
		/// Розмір комісії в мінімальних одиницях валюти (копійках, центах)
		/// </summary>
		[JsonProperty("commissionRate")]
		public long CommissionRate { get; set; }

		/// <summary>
		/// Форматований розмір комісії в мінімальних одиницях валюти (копійках, центах)
		/// </summary>
		public string CommissionRateFormat { get; set; }

		/// <summary>
		/// Розмір кешбеку в мінімальних одиницях валюти (копійках, центах)
		/// </summary>
		[JsonProperty("cashbackAmount")]
		public long CashbackAmount { get; set; }

		/// <summary>
		/// Форматований розмір кешбеку в мінімальних одиницях валюти (копійках, центах)
		/// </summary>
		public string CashbackAmountFormat { get; set; }

		/// <summary>
		/// Баланс рахунку в мінімальних одиницях валюти (копійках, центах)
		/// </summary>
		[JsonProperty("balance")]
		public int Balance { get; set; }

		/// <summary>
		/// Форматована сума балансу рахунку в мінімальних одиницях валюти (копійках, центах)
		/// </summary>
		public string BalanceFormat { get; set; }

		/// <summary>
		/// Статус блокування суми
		/// </summary>
		[JsonProperty("hold")]
		public bool Hold { get; set; }

		/// <summary>
		/// Прив'язка до картки рахунку
		/// </summary>
		public virtual Account Account { get; set; }
	}
}
