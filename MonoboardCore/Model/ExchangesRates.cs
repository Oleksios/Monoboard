using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestEase;

namespace MonoboardCore.Model
{
	public interface IExchangesApi
	{
		[Get("bank/currency")]
		[AllowAnyStatusCode]
		Task<Response<List<ExchangeRates>>> GetExchangeRatesAsync();
	}

	public class ExchangeRates
	{
		public int ExchangeRatesId { get; set; }

		[JsonProperty("currencyCodeA")]
		public int CurrencyCodeA { get; set; }

		[JsonProperty("currencyCodeB")]
		public int CurrencyCodeB { get; set; }

		[JsonProperty("date")]
		public long Date { get; set; }

		[JsonProperty("rateBuy")]
		public float RateBuy { get; set; }

		[JsonProperty("rateSell")]
		public float RateSell { get; set; }

		[JsonProperty("rateCross")]
		public float? RateCross { get; set; }
	}
}
