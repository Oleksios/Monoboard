using Newtonsoft.Json;
using RestEase;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace MonoboardCore.Model
{
	public interface IUserInfoApi
	{
		[Get("personal/client-info")]
		[AllowAnyStatusCode]
		Task<Response<UserInfo>> GetUserInfoAsync([Header("X-Token")] string token);
	}

	/// <summary>
	/// Тип наявних карток в Monobank
	/// </summary>
	public enum CardType { Black, White, Platinum, Iron, Yellow, Fop }

	public enum CardSystem { MasterCard, Visa, Unknown}

	/// <summary>
	/// Тип наявних кешбеків в Monobank
	/// </summary>
	public enum CashbackType { None, Uah, Miles }

	/// <summary>
	/// Дані про рахунки користувача
	/// </summary>
	public class Account
	{
		public int Id { get; set; }

		/// <summary>
		/// [НЕ ЧІПАТИ!]
		/// Прив'язка до основних даних користувача
		/// </summary>
		public int AccountKey { get; set; }

		/// <summary>
		/// [НЕ ЧІПАТИ!]
		/// Id користувача Monobank (для порівняння)
		/// </summary>
		public string ClientId { get; set; }

		/// <summary>
		/// Ідентифікатор рахунку
		/// </summary>
		[JsonProperty("id")]
		public string CardCode { get; set; }

		/// <summary>
		/// Найменування картки/рахунку
		/// </summary>
		public string? CustomCardName { get; set; }

		/// <summary>
		/// Опис картки/рахунку
		/// </summary>
		public string? CustomCardDescription { get; set; }

		/// <summary>
		/// Замасковані номери карток
		/// </summary>
		[JsonProperty("maskedPan")]
		[NotMapped]
		public virtual IList<string> MaskedPanList { get; set; }
		public virtual string MaskedPan { get; set; }

		/// <summary>
		/// Міжнародний номер банківського рахунку
		/// </summary>
		[JsonProperty("Iban")]
		public string Iban { get; set; }

		/// <summary>
		/// Тип картки
		/// </summary>
		[JsonProperty("type")]
		public string CardType { get; set; }

		/// <summary>
		/// Баланс рахунку в мінімальних одиницях валюти (копійках, центах)
		/// </summary>
		[JsonProperty("balance")]
		public long Balance { get; set; }

		/// <summary>
		/// Форматований баланс для читабельного вигляду. Наприклад: "9720,35₴", "322,50$", тощо.
		/// </summary>
		public string? BalanceFormat { get; set; }

		/// <summary>
		/// Кредитний ліміт
		/// </summary>
		[JsonProperty("creditLimit")]
		public long CreditLimit { get; set; }

		/// <summary>
		/// Форматований кредитний ліміт для читабельного вигляду. Наприклад: "9720,35₴", "322,50$", тощо.
		/// </summary>
		public string? CreditLimitFormat { get; set; }

		/// <summary>
		/// Код валюти рахунку відповідно ISO 4217
		/// </summary>
		[JsonProperty("currencyCode")]
		public int CurrencyCode { get; set; }

		/// <summary>
		/// Тип кешбеку який нараховується на рахунок
		/// </summary>
		[JsonProperty("cashbackType")]
		public string CashbackType { get; set; }

		/// <summary>
		/// [Стан] Рахунок користувача видалено / не видалено
		/// </summary>
		public bool IsDeleted { get; set; }

		/// <summary>
		/// Вказує, чи завантажено всю виписку по даному рахунку користувача (не рахуючи поточний місяць)
		/// </summary>
		public bool IsDownloaded { get; set; }

		/// <summary>
		/// Список витрат по рахунку
		/// </summary>
		public virtual IList<StatementItem> StatementItems { get; set; }

		/// <summary>
		/// Прив'язка до основних (базових) даних користувача
		/// </summary>
		public virtual UserInfo UserInfo { get; set; }
	}

	/// <summary>
	/// Основна інформація про користувача
	/// </summary>
	public class UserInfo
	{
		/// <summary>
		/// Порядковий номер в базі даних
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Id користувача монобанку
		/// </summary>
		[JsonProperty("clientId")]
		public string ClientId { get; set; }

		/// <summary>
		/// Ім'я клієнта
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// Скорочене ім'я клієнта
		/// </summary>
		public string NameShort { get; set; }

		/// <summary>
		/// Дозволи для доступу до облікового запису користувача
		/// </summary>
		[JsonProperty("permissions")]
		public string Permissions { get; set; }

		/// <summary>
		/// Перелік доступних рахунків
		/// </summary>
		[JsonProperty("accounts")]
		public virtual IList<Account> Accounts { get; set; }

		public virtual MonoboardUser MonoboardUser { get; set; }
	}

	/// <summary>
	/// Специфічні дані користувача для використання у застосунку
	/// </summary>
	public class MonoboardUser
	{
		/// <summary>
		/// Порядковий номер в базі даних
		/// </summary>
		public int MonoboardUserId { get; set; }

		/// <summary>
		/// [НЕ ЧІПАТИ!]
		/// Прив'язка до основних даних користувача
		/// </summary>
		public int MonoboardUserKey { get; set; }

		/// <summary>
		/// [НЕ ЧІПАТИ!]
		/// Id користувача Monobank (для порівняння)
		/// </summary>
		public string ClientId { get; set; }

		/// <summary>
		/// Пароль користувача у вигляду хешу
		/// </summary>
		public string? Password { get; set; }

		/// <summary>
		/// [Стан] Пароль скинуто / Пароль не скинуто
		/// </summary>
		public bool IsPasswordReset { get; set; }


		/// <summary>
		/// Ключ доступу до API Монобанку
		/// </summary>
		public string AccessToken { get; set; }

		/// <summary>
		/// Мова додатку користувача
		/// </summary>
		public string Language { get; set; }

		/// <summary>
		/// [Стан] Темна тема / Світла тема
		/// </summary>
		public bool IsDarkTheme { get; set; }

		/// <summary>
		/// Основний колір додатку
		/// </summary>
		public string PrimaryColor { get; set; }

		/// <summary>
		/// Основний колір додатку
		/// </summary>
		public string SecondaryColor { get; set; }

		public virtual UserInfo UserInfo { get; set; }
	}
}
