using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace MonoboardCore.Set
{
	public static class SetAccount
	{
		/// <summary>
		/// Встановлює введені дані картки/рахунку користувача
		/// </summary>
		/// <param name="clientId">Id клієнта Monobank</param>
		/// <param name="cardCode">Ідентифікатор рахунку</param>
		/// <param name="cardName">Змінена назва картки/рахунку користувача</param>
		/// <param name="cardDescription">Змінений опис картки/рахунку користувача</param>
		/// <returns></returns>
		public static async Task<bool> SetCardPersonalizationAsync(
			string clientId,
			string cardCode,
			string cardName,
			string cardDescription)
		{
			await using var db = new MonoboardDbContext();

			var account = await db.Accounts.FirstAsync(accounts => accounts.ClientId == clientId && accounts.CardCode == cardCode);

			if(string.IsNullOrWhiteSpace(cardName) is false)
				account.CustomCardName = cardName;

			if (string.IsNullOrWhiteSpace(cardDescription) is false)
				account.CustomCardDescription = cardDescription;

			db.Accounts.Update(account);

			return await db.SaveChangesAsync() != 0;
		}

		/// <summary>
		/// Вказує, що вся виписка картки/рахунку користувача вже завантажена
		/// [Поточний місяць не рахується]
		/// </summary>
		/// <param name="clientId">Id клієнта Monobank</param>
		/// <param name="cardCode">Ідентифікатор рахунку</param>
		/// <returns>[Стан] Позначує, що виписка завантажена / виникла помилка</returns>
		public static async Task<bool> SetStatementsDownloadedAsync(
			string clientId,
			string cardCode)
		{
			await using var db = new MonoboardDbContext();

			var account = await db.Accounts.FirstAsync(accounts => accounts.ClientId == clientId && accounts.CardCode == cardCode);

			account.IsDownloaded = true;

			db.Accounts.Update(account);

			return await db.SaveChangesAsync() != 0;
		}
	}
}
