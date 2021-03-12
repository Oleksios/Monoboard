using MonoboardCore.Model;
using System;
using System.Threading.Tasks;

namespace MonoboardCore.Set
{
	public static class SetStatementItemsChecker
	{
		/// <summary>
		/// Зберігає інформацію про те, що дані за вказаний місяць завантажені
		/// </summary>
		/// <param name="cardCode">Код картки рахунку користувача</param>
		/// <param name="month">Вибрана дата (місяць)</param>
		/// <param name="isEnd">Вказує, чи це є кінцевою точкою виписки</param>
		/// <returns>[Стан] Дані збережено / не збережено</returns>
		public static async Task<bool> CheckSaveAsync(string cardCode, DateTime month, bool isEnd)
		{
			await using var db = new MonoboardDbContext();

			await db.StatementItemsCheckers.AddAsync(new StatementItemsChecker
			{
				CardCode = cardCode,
				Month = month,
				IsEnd = isEnd
			});

			return await db.SaveChangesAsync() != 0;
		}
	}
}
