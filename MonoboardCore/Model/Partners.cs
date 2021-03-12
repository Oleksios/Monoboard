using Newtonsoft.Json;
using RestEase;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonoboardCore.Model
{
	public interface IPartnersApi
	{
		[Get("api/credit/divided-payment/partners")]
		[AllowAnyStatusCode]
		Task<Response<List<Partner>>> GetPartnersInfoAsync();
	}

	/// <summary>
	/// Дані про категорію
	/// </summary>
	public class PartnersCategory
	{
		public int Id { get; set; }

		/// <summary>
		/// [НЕ ЧІПАТИ!]
		/// Прив'язка до основних даних партнерів
		/// </summary>
		public int PartnerKey { get; set; }

		/// <summary>
		/// Ідентифікатор категорії
		/// </summary>
		[JsonProperty("id")]
		public string CategoryId { get; set; }

		/// <summary>
		/// Найменування категорії
		/// </summary>
		[JsonProperty("title")]
		public string Title { get; set; }

		public virtual Partner Partner { get; set; }
	}

	/// <summary>
	/// Дані щодо партнера Monobamk (Покупка частинами)
	/// </summary>
	public class Partner
	{
		public int Id { get; set; }

		/// <summary>
		/// Найменування партнера
		/// </summary>
		[JsonProperty("title")]
		public string Title { get; set; }

		/// <summary>
		/// Сайт партнера
		/// </summary>
		[JsonProperty("url")]
		public string Url { get; set; }

		/// <summary>
		/// Логотип партнера (Потрібен доступ до інтернету)
		/// </summary>
		[JsonProperty("logo_url")]
		public string LogoUrl { get; set; }

		/// <summary>
		/// Зображення логотипу партнера Монобанку в форматі Base64
		/// </summary>
		public string? Logo { get; set; }

		/// <summary>
		/// Замовлення онлайн з доставною
		/// </summary>
		[JsonProperty("is_full_online")]
		public bool IsFullOnline { get; set; }

		/// <summary>
		/// Замовлення онлайн але потрібно відвідувати магазин
		/// </summary>
		[JsonProperty("is_partially_online")]
		public bool IsPartiallyOnline { get; set; }

		/// <summary>
		/// Примітка щодо партнера
		/// </summary>
		[JsonProperty("note")]
		public string? Note { get; set; }

		/// <summary>
		/// Категорія партнера
		/// </summary>
		[JsonProperty("categories")]
		public virtual IList<PartnersCategory> Categories { get; set; }
	}
}
