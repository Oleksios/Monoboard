using Newtonsoft.Json;

namespace MonoboardCore.Model
{
	public class Error
	{
		/// <summary>
		/// Офіційне повідомлення про помилку від API Monobank
		/// </summary>
		[JsonProperty("errorDescription")]
		public string? ErrorDescription { get; set; }

		/// <summary>
		/// Повідомлення про помилку від розробника "Покупка частинами"
		/// </summary>
		[JsonProperty("code")]
		public int? Code { get; set; }

		/// <summary>
		/// Повідомлення про помилку від розробника "Покупка частинами"
		/// </summary>
		[JsonProperty("message")]
		public string? Message { get; set; }
	}

	public class ErrorInformation
	{
		public ErrorInformation(Error error) => Error = error;

		/// <summary>
		/// Тіло основного блоку про повідомлення помилки від розробника "Покупка частинами"
		/// </summary>
		[JsonProperty("error")]
		public Error Error { get; set; }
	}
}
