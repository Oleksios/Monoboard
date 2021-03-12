using Microsoft.EntityFrameworkCore;
using MonoboardCore.Hepler;
using System.Threading.Tasks;

namespace MonoboardCore.Set
{
	public class SetMonoboardUserData
	{
		/// <summary>
		/// Зберігає мову користувача
		/// </summary>
		/// <param name="clientId">Id клієнта Monobank</param>
		/// <param name="language">Вибрана мова</param>
		/// <returns>[Стан] Мову змінено / не змінено</returns>
		public static async Task<bool> UpdateLanguage(string clientId, string language)
		{
			await using var db = new MonoboardDbContext();

			var user = await db.MonoBoardUsers.FirstAsync(monoboardUser => monoboardUser.ClientId == clientId);

			user.Language = language;

			db.MonoBoardUsers.Update(user);

			return await db.SaveChangesAsync() != 0;
		}

		/// <summary>
		/// Визначає тему додатку за бажанням користувача
		/// </summary>
		/// <param name="clientId">Id клієнта Monobank</param>
		/// <param name="isDarkTheme">[Стан] Світла тема / Темна тема</param>
		/// <returns>[Стан] Тему змінено / не змінено</returns>
		public static async Task<bool> UpdateTheme(string clientId, bool isDarkTheme)
		{
			await using var db = new MonoboardDbContext();

			var user = await db.MonoBoardUsers.FirstAsync(monoboardUser => monoboardUser.ClientId == clientId);

			user.IsDarkTheme = isDarkTheme;

			db.MonoBoardUsers.Update(user);

			return await db.SaveChangesAsync() != 0;
		}

		/// <summary>
		/// Визначає первинний колір додатку за бажанням користувача
		/// </summary>
		/// <param name="clientId">Id клієнта Monobank</param>
		/// <param name="primaryColor">Основний колір додатку</param>
		/// <returns>[Стан] Основний колір змінено / не змінено</returns>
		public static async Task<bool> UpdatePrimaryColor(string clientId, string primaryColor)
		{
			await using var db = new MonoboardDbContext();

			var user = await db.MonoBoardUsers.FirstAsync(monoboardUser => monoboardUser.ClientId == clientId);

			user.PrimaryColor = primaryColor.Contains("#") ?
				primaryColor :
				$"#{primaryColor}";

			db.MonoBoardUsers.Update(user);

			return await db.SaveChangesAsync() != 0;
		}

		/// <summary>
		/// Визначає другорядний (акцентний) колір додатку за бажанням користувача
		/// </summary>
		/// <param name="clientId">Id клієнта Monobank</param>
		/// <param name="secondaryColor">Акцентний колір додатку</param>
		/// <returns>[Стан] Акцентний колір змінено / не змінено</returns>
		public static async Task<bool> UpdateSecondaryColor(string clientId, string secondaryColor)
		{
			await using var db = new MonoboardDbContext();

			var user = await db.MonoBoardUsers.FirstAsync(monoboardUser => monoboardUser.ClientId == clientId);

			user.SecondaryColor = secondaryColor.Contains("#") ?
				secondaryColor :
				$"#{secondaryColor}";


			db.MonoBoardUsers.Update(user);

			return await db.SaveChangesAsync() != 0;
		}

		/// <summary>
		/// Встановлює значення теми і кольорів за замовчуванням
		/// </summary>
		/// <param name="clientId">Id клієнта Monobank</param>
		/// <returns>[Стан] Значення за замовчуванням встановлено / не встановлено</returns>
		public static async Task<bool> ResetColorTheme(string clientId)
		{
			await using var db = new MonoboardDbContext();

			var user = await db.MonoBoardUsers.FirstAsync(monoboardUser => monoboardUser.ClientId == clientId);

			user.IsDarkTheme = false;
			user.PrimaryColor = "fff43b3f";
			user.SecondaryColor = "ff06b206";

			db.MonoBoardUsers.Update(user);

			return await db.SaveChangesAsync() != 0;
		}

		/// <summary>
		/// Записує захешований пароль користувача в базу даних
		/// </summary>
		/// <param name="clientId">Id клієнта Monobank</param>
		/// <param name="password">Захешований пароль користувача</param>
		/// <returns>[Стан] Пароль встановлено / не встановлено</returns>
		public static async Task<bool> SetNewPassword(string clientId, string password)
		{
			await using var db = new MonoboardDbContext();

			var user = await db.MonoBoardUsers.FirstAsync(monoboardUser => monoboardUser.ClientId == clientId);

			user.Password = password;
			user.IsPasswordReset = false;

			db.MonoBoardUsers.Update(user);

			return await db.SaveChangesAsync() > 0;
		}

		/// <summary>
		/// Скидання паролю користувача
		/// </summary>
		/// <param name="clientId">Id клієнта Monobank</param>
		/// <returns>[Стан] Пароль скинуто / не скинуто та згенерований пароль для користувача</returns>
		public static async Task<(bool isResetCompleted, string newPassword)> ResetPasswordAsync(string clientId)
		{
			await using var db = new MonoboardDbContext();

			var user = await db.MonoBoardUsers.FirstAsync(monoboardUser => monoboardUser.ClientId == clientId);

			var newPassword = Sha256Hash.Compute($"user{user.ClientId}!");
			
			bool isComplete;

			if (user.Password != newPassword)
			{
				user.Password = newPassword;
				user.IsPasswordReset = true;

				isComplete = await db.SaveChangesAsync() > 0;
			}
			else
				isComplete = true;

			return (isComplete, $"user{user.MonoboardUserKey}{user.ClientId}!");
		}

		/// <summary>
		/// Змінюємо токен доступу користувача
		/// </summary>
		/// <param name="clientId">Id клієнта Monobank</param>
		/// <param name="token">Токен користувача</param>
		/// <returns>[Стан] Токен змінено / не змінено</returns>
		public static async Task<bool> ChangeAccessTokenAsync(string clientId, string token)
		{
			await using var db = new MonoboardDbContext();

			var user = await db.MonoBoardUsers.FirstAsync(monoboardUser => monoboardUser.ClientId == clientId);

			user.AccessToken = token;

			db.MonoBoardUsers.Update(user);

			return await db.SaveChangesAsync() > 0;
		}
	}
}
