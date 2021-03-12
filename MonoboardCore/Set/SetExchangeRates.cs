using Microsoft.EntityFrameworkCore;
using MonoboardCore.Hepler;
using MonoboardCore.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonoboardCore.Set
{
	public static class SetExchangeRates
	{
		/// <summary>
		/// Зберігає дані курсів валют в базі даних
		/// </summary>
		/// <param name="exchangeRates">Дані курсу валют</param>
		/// <returns>[Стан] Дані збережено / не збережено</returns>
		public static async Task<bool> Save(List<ExchangeRates> exchangeRates)
		{
			var dbContext = new MonoboardDbContext();
			if (await dbContext.ExchangeRates.AnyAsync())
			{
				var dbExchangeRates = await dbContext.ExchangeRates
					.Select(exchangesData => exchangesData)
					.OrderByDescending(exchangesData => exchangesData.ExchangeRatesId)
					.Take(114)
					.OrderByDescending(exchangesData => exchangesData.CurrencyCodeA)
					.ToListAsync();

				for (var index = 0; index < exchangeRates.Count; index++)
				{
					var dbRateCross = dbExchangeRates[index];

					var rateCross = exchangeRates.Single(rates =>
						rates.CurrencyCodeA == dbRateCross.CurrencyCodeA &&
						rates.CurrencyCodeB == dbRateCross.CurrencyCodeB);

					if (UnixConverter.UnixTimestampToDateTime(dbRateCross.Date)
							.ToShortDateString() ==
						UnixConverter.UnixTimestampToDateTime(rateCross.Date)
							.ToShortDateString())
					{
						if (dbRateCross.RateBuy != rateCross.RateBuy)
							dbRateCross.RateBuy = float.Parse(rateCross.RateBuy.ToString("F4"));

						if (dbRateCross.RateSell != rateCross.RateSell)
							dbRateCross.RateSell = float.Parse(rateCross.RateSell.ToString("F4"));

						if (dbRateCross.RateCross.HasValue && rateCross.RateCross.HasValue)
							if (dbRateCross.RateCross != rateCross.RateCross)
								dbRateCross.RateCross = float.Parse(rateCross.RateCross!.Value.ToString("F4"));

						dbRateCross.Date = rateCross.Date;

						dbContext.ExchangeRates.Update(dbRateCross);
					}
					else
					{
						rateCross.RateBuy = float.Parse(rateCross.RateBuy.ToString("F4"));
						rateCross.RateSell = float.Parse(rateCross.RateSell.ToString("F4"));

						if (dbRateCross.RateCross.HasValue && rateCross.RateCross.HasValue)
							rateCross.RateCross = float.Parse(rateCross.RateCross.Value.ToString("F4"));

						await dbContext.ExchangeRates.AddAsync(rateCross);
					}
				}

				return await dbContext.SaveChangesAsync() > 0;
			}

			foreach (var rates in exchangeRates)
			{
				rates.RateBuy = float.Parse(rates.RateBuy.ToString("F4"));
				rates.RateSell = float.Parse(rates.RateSell.ToString("F4"));

				if (rates.RateCross.HasValue)
					rates.RateCross = float.Parse(rates.RateCross.Value.ToString("F4"));
			}

			await dbContext.ExchangeRates.AddRangeAsync(exchangeRates);

			return await dbContext.SaveChangesAsync() > 0;
		}
	}
}
