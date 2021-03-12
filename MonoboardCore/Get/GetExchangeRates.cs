using System;
using MonoboardCore.Model;
using Newtonsoft.Json;
using RestEase;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonoboardCore.Hepler;

namespace MonoboardCore.Get
{
	public static class GetExchangeRates
	{
		/// <summary>
		/// Завантажує дані про курси валют з Монобанку
		/// </summary>
		/// <returns>Список валют з їхніми курсами</returns>
		public static async Task<(List<ExchangeRates>? exchangeRates, string? message)> Downdload()
		{
			var exchangesApi = RestClient.For<IExchangesApi>("https://api.monobank.ua");

			try
			{
				using var response = await exchangesApi.GetExchangeRatesAsync();

				return response.ResponseMessage.StatusCode switch
				{
					HttpStatusCode.OK =>
					(response.GetContent(), ""),
					HttpStatusCode.TooManyRequests =>
					(null, JsonConvert.DeserializeObject<Error>(response.StringContent).ErrorDescription),
					_ =>
					(null, JsonConvert.DeserializeObject<Error>(response.StringContent).ErrorDescription)
				};
			}
			catch (Exception)
			{
				return (null, "MbNoInternet")!;
			}
		}

		/// <summary>
		/// Завантажує дані про курси валют з бази даних
		/// </summary>
		/// <returns>Список валют з їхніми курсами</returns>
		public static List<ExchangeRates> GetInfo() =>
			new MonoboardDbContext().ExchangeRates
				.OrderByDescending(exchangesData => exchangesData.Date)
				.DistinctBy(rates => new { rates.CurrencyCodeA, rates.CurrencyCodeB})
				.OrderByDescending(rates => rates.CurrencyCodeA)
				.ToList();
	}
}
