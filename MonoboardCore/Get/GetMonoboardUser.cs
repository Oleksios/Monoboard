using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MonoboardCore.Get
{
	public static class GetMonoboardUser
	{
		/// <summary>
		/// Отримуємо кольорову тему користувача для первісного налаштування
		/// </summary>
		/// <param name="clientId">Id клієнта</param>
		/// <returns>Кольорова тема користувача</returns>
		public static async Task<(bool isDark, string primaryColor, string secondaryColor)> GetColorThemeAsync(string clientId)
		{
			var user = await new MonoboardDbContext().MonoBoardUsers
				.FirstAsync(info => info.ClientId == clientId);

			return (user.IsDarkTheme, user.PrimaryColor, user.SecondaryColor);
		}

		/// <summary>
		/// Перевіряємо наявність користувача в базі даних по токену
		/// </summary>
		/// <param name="token">Токен користувача</param>
		/// <returns>[Стан] Користувач в базі даних наявний / відсутній</returns>
		public static async Task<bool> GetUserExistByToken(string token)
		{
			var users = new MonoboardDbContext().MonoBoardUsers;

			return await users.AnyAsync() && await users.AnyAsync(user => user.AccessToken == token);
		}

		/// <summary>
		/// Перевіряємо наявність паролю користувача в базі даних по його Id
		/// </summary>
		/// <param name="clientId">Id клієнта</param>
		/// <returns>[Стан] Пароль користувача в базі даних наявний / відсутній</returns>
		public static async Task<bool> GetPasswordExistAsync(string clientId) =>
			!string.IsNullOrEmpty(await new MonoboardDbContext().MonoBoardUsers
				.Where(user => user.ClientId == clientId)
				.Select(user => user.Password).SingleAsync());

		/// <summary>
		/// Звіряє паролі, що ведені користувачем з тим, що знаходяться в базі даних
		/// </summary>
		/// <param name="clientId">Id клієнта</param>
		/// <param name="password">Захешований пароль</param>
		/// <returns>[Стан] Паролі співпадають / Паролі не співпадають</returns>
		public static async Task<bool> CheckPassword(string clientId, string password) =>
			string.Equals(await new MonoboardDbContext().MonoBoardUsers.Where(user => user.ClientId == clientId)
					.Select(user => user.Password)
					.SingleAsync(),
				password);

		/// <summary>
		/// Перевіряє, чи знаходиться пароль користувача в стані "скинутого"
		/// </summary>
		/// <param name="clientId">Id клієнта</param>
		/// <returns>[Стан] Пароль користувача скинуто / не скинуто</returns>
		public static async Task<bool> CheckIsResetPasswordAsync(string clientId) =>
			await new MonoboardDbContext().MonoBoardUsers.Where(user => user.ClientId == clientId)
				.Select(user => user.IsPasswordReset)
				.SingleAsync();
	}
}
