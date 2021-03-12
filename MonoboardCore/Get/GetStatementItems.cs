using System;
using Microsoft.EntityFrameworkCore;
using MonoboardCore.Model;
using Newtonsoft.Json;
using RestEase;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MonoboardCore.Get
{
	public static class GetStatementItems
	{
		/// <summary>
		/// Завантажує дані виписки за вказаний період з API Monobank
		/// </summary>
		/// <param name="token">Токен користувача</param>
		/// <param name="account">Код рахунку</param>
		/// <param name="from">Дата й час моменту початку "читання" виписки</param>
		/// <param name="to">Дата й час моменту закінчення "читання" виписки</param>
		/// <returns>Дані виписки або повідомлення про помилку</returns>
		public static async Task<(List<StatementItem> statementItems, string info)> DownloadAsync(string token, string account, string from, string to = "")
		{
			var clientApi = RestClient.For<IStatementItemApi>("https://api.monobank.ua");

			try
			{
				using (var response = await clientApi.GetAccountStatementAsync(token, account, from, to))
				{
					if (response != null)
					{
						if (response.ResponseMessage.StatusCode != HttpStatusCode.OK)
						{
							var errorDescription = JsonConvert.DeserializeObject<Error>(response.StringContent).ErrorDescription;

							if (errorDescription == "Value field 'to' out of bounds")
								return (null, "MbDataBeforeRegister")!;

							return (null, errorDescription)!;
						}
					}

					return (response.GetContent(), "");
				}
			}
			catch (Exception)
			{
				return (null, "MbNoInternet")!;
			}
		}

		/// <summary>
		/// Отримуємо дані виписки з бази даних
		/// </summary>
		/// <param name="account">Код рахунку</param>
		/// <returns>Дані виписки</returns>
		public static async Task<ICollection<StatementItem>> GetStatementItemsList(string account) =>
			await new MonoboardDbContext().Accounts.Where(accountItem => accountItem.CardCode == account)
				.SelectMany(accountItem => accountItem.StatementItems).ToListAsync();

		/// <summary>
		/// Отримуємо дані виписки з бази даних
		/// </summary>
		/// <param name="account">Код рахунку</param>
		/// <param name="from">З якого проміжку часу отримувати дані</param>
		/// <param name="to">До якого проміжку часу отримувати дані</param>
		/// <returns>Дані виписки</returns>
		public static async Task<ICollection<StatementItem>> GetStatementItemsList(string account, long from, long to) =>
			await new MonoboardDbContext().Accounts.Where(accountItem => accountItem.CardCode == account)
				.SelectMany(accountItem => accountItem.StatementItems)
				.Where(item => item.Time >= @from && item.Time <= to)
				.ToListAsync();
	}
}
