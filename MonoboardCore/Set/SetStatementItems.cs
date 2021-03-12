using MonoboardCore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonoboardCore.Set
{
	public static class SetStatementItems
	{
		/// <summary>
		/// Зберігає дані виписки в базі даних
		/// </summary>
		/// <param name="account">Рахунок користувача для якої взята виписка</param>
		/// <param name="statementItems">Дані виписки</param>
		/// <returns>[Дані збережено / не збережено] [Повідомлення]</returns>
		public static async Task<(bool isSaved, string message)> SaveAsync(string account, List<StatementItem> statementItems)
		{
			try
			{
				var dbContext = new MonoboardDbContext();

				var databaseData = dbContext.StatementItems.Select(item => item.Id).ToList();

				if (statementItems.Count == 0)
					return (true, "MbDataMonthNotExist");

				foreach (var item in statementItems.Where(item => !databaseData.Exists(s => s == item.Id)))
				{
					item.CardKey = account;
					await dbContext.StatementItems.AddAsync(item);
				}

				return await dbContext.SaveChangesAsync() > 0
					? (true, "MbSaveDataSuccess")
					: (false, "MbDataAlreadyDownloaded");
			}
			catch (Exception exception)
			{
				return (false, exception.Message);
			}
		}
	}
}
