using MonoboardCore.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonoboardCore.Set
{
	public static class SetPartners
	{
		/// <summary>
		/// Завантажує нові дані щодо партнерів базі даних застосунку
		/// </summary>
		/// <param name="partners">Інформація про партнерів</param>
		/// <returns>[Стан] Збереження виконано / Виникла помилка</returns>
		public static async Task<bool> SaveAsync(List<Partner> partners)
		{
			await using var db = new MonoboardDbContext();

			await db.Partners.AddRangeAsync(partners);

			return await db.SaveChangesAsync() > 0;
		}

		/// <summary>
		/// Видаляє всі дані про партнерів Монобанку
		/// </summary>
		/// <returns>[Стан] Видалено успішно / Виникла помилка</returns>
		public static async Task<bool> DeleteAsync()
		{
			await using var db = new MonoboardDbContext();

			if (db.Partners.Any() is false) return true;
			
			db.Partners.RemoveRange(db.Partners);
			return await db.SaveChangesAsync() > 0;

		}
	}
}
