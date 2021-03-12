using Microsoft.EntityFrameworkCore;
using MonoboardCore.Model;
using System.Threading.Tasks;

namespace MonoboardCore.Set
{
	public static class SetUserInfo
	{
		/// <summary>
		/// Створює нового користувача в базі даних застосунку
		/// </summary>
		/// <param name="userInfo">Інформація про користувача</param>
		/// <returns>[Стан] Створення нового користувача пройшло успішно / не успішно</returns>
		public static async Task<bool> SetNewUserAsync(UserInfo userInfo)
		{
			await using var db = new MonoboardDbContext();

			await db.UserInfo.AddAsync(userInfo);

			return await db.SaveChangesAsync() > 0;
		}

		/// <summary>
		/// Оновлює дані користувача в базі даних застосунку
		/// </summary>
		/// <param name="userInfo">Інформація про користувача</param>
		/// <returns>[Стан] Створення нового користувача пройшло успішно / не успішно</returns>
		public static async Task<bool> UpdateUserAsync(UserInfo userInfo)
		{
			await using var db = new MonoboardDbContext();

			db.UserInfo.Update(userInfo);

			return await db.SaveChangesAsync() > 0;
		}

		/// <summary>
		/// Видаляє користувача з бази даних і всю інформацію пов'язану з ним
		/// </summary>
		/// <param name="clientId">Id клієнта Monobank</param>
		/// <returns>[Стан] Видалення користувача пройшло успішно / не успішно</returns>
		public static async Task<bool> DeleteUserDataAsync(string clientId)
		{
			await using var db = new MonoboardDbContext();

			db.UserInfo.Remove(await db.UserInfo.SingleAsync(user => user.ClientId == clientId));

			return await db.SaveChangesAsync() > 0;
		}
	}
}
