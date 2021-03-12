using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonoboardCore.Get
{
	public static class GetStatementItemsChecker
	{
		/// <summary>
		/// Визначає, чи були завантажені дані за цей місяць раніше
		/// </summary>
		/// <param name="cardCode">Код картки рахунку</param>
		/// <param name="currentDate">Вибрана дата</param>
		/// <returns>Дані за цей місяць присутні / відсутні</returns>
		public static async Task<bool> CheckAsync(string cardCode, DateTime currentDate)
		{
			await using var db = new MonoboardDbContext();

			var itemsChecker = db.StatementItemsCheckers
				.Where(checker => checker.CardCode == cardCode)
				.Select(checker => checker.Month);

			return itemsChecker.Contains(currentDate);
		}

		/// <summary>
		/// Знаходимо останню дату (місяць) дані якої не було завантажено
		/// </summary>
		/// <param name="cardCode">Код картки рахунку</param>
		/// <returns>Дата (місяць), яка не має жодних завантажених даних</returns>
		public static async Task<DateTime> GetLastDate(string cardCode)
		{
			await using var db = new MonoboardDbContext();

			var currentDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1, 0, 0, 0);

			var lastDate = await db.StatementItemsCheckers.SingleOrDefaultAsync(checker => checker.CardCode == cardCode && checker.IsEnd);

			var items = db.StatementItemsCheckers
				.Where(checker => checker.CardCode == cardCode)
				.Select(checker => checker.Month)
				.OrderByDescending(time => time.Date);

			var monthoffset = -1;

			foreach (var dateTime in items)
			{
				if (lastDate?.Month.Date == dateTime.Date)
					return lastDate.Month.Date;

				if (dateTime.Date == currentDate.Date.AddMonths(monthoffset))
					monthoffset--;
				else
					return currentDate.Date.AddMonths(monthoffset);
			}

			return currentDate.Date.AddMonths(monthoffset);
		}
	}
}
