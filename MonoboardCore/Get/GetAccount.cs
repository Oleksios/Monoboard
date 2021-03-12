using Microsoft.EntityFrameworkCore;
using MonoboardCore.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonoboardCore.Get
{
	public static class GetAccount
	{
		/// <summary>
		/// Отримуємо список банківських карт користувача
		/// </summary>
		/// <param name="clientId">Id клієнта</param>
		/// <param name="isGetDeleted">Вказує, чи брати інформацію щодо карт/рахунків, що вже видалені</param>
		/// <returns>Список банківських карт</returns>
		public static async Task<ICollection<Account>> GetListAsync(string clientId, bool isGetDeleted = false)
		{
			var accounts = await new MonoboardDbContext().Accounts
				.Where(account => account.ClientId == clientId && account.IsDeleted == isGetDeleted)
				.ToListAsync();

			foreach (var account in accounts.Where(account => account.MaskedPan.Contains('|')))
				account.MaskedPanList = account.MaskedPan.Split('|').ToList();

			return accounts;
		}
	}
}
