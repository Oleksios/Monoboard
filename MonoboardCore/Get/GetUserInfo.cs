using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonoboardCore.Model;
using Newtonsoft.Json;
using RestEase;

namespace MonoboardCore.Get
{
	public static class GetUserInfo
	{
		/// <summary>
		/// Завантажує дані користувача з Monobank
		/// </summary>
		/// <param name="token">Токен користувача</param>
		/// <returns>Завантажена з API Monobank інформація про користувача</returns>
		public static async Task<(UserInfo userInfo, string message)> GetClientInfoAsync(string? token)
		{
			var clientApi = RestClient.For<IUserInfoApi>("https://api.monobank.ua");

			try
			{
				using (var response = await clientApi.GetUserInfoAsync(token))
				{
					if (response.ResponseMessage.StatusCode == HttpStatusCode.Forbidden)
						return (null, JsonConvert.DeserializeObject<Error>(response.StringContent).ErrorDescription)!;

					var userInfo = response.GetContent();

					foreach (var userAccount in userInfo.Accounts)
					{
						var maskedPan = "";

						for (var i = 0; i < userAccount.MaskedPanList.Count; i++)
							maskedPan += i == userAccount.MaskedPanList.Count - 1
								? userAccount.MaskedPanList[i]
								: $"{userAccount.MaskedPanList[i]}|";

						userAccount.MaskedPan = maskedPan;

						userAccount.ClientId = userInfo.ClientId;
					}

					if (userInfo.Name.Contains(" "))
					{
						var parts = userInfo.Name.Split(" ");

						for (var i = 0; i < 2; i++)
							userInfo.NameShort += parts[i].First().ToString().ToUpperInvariant();
					}
					else
						userInfo.NameShort = userInfo.Name.First().ToString().ToUpperInvariant();

					return (userInfo, "");
				}
			}
			catch (Exception)
			{
				return (null, "MbNoInternet")!;
			}
		}

		/// <summary>
		/// Завантажує ID користувача з Monobank (для перевірки достовірності при скиданні паролю)
		/// </summary>
		/// <param name="token">Токен користувача</param>
		/// <returns>Завантажена з API Monobank інформація про ID користувача</returns>
		public static async Task<(bool isSuccess, string clientIdOrMessage)> GetClientIdAsync(string? token)
		{
			var clientApi = RestClient.For<IUserInfoApi>("https://api.monobank.ua");

			try
			{
				using (var response = await clientApi.GetUserInfoAsync(token))
				{
					if (response.ResponseMessage.StatusCode == HttpStatusCode.Forbidden)
						return (false, JsonConvert.DeserializeObject<Error>(response.StringContent).ErrorDescription)!;

					var user = response.GetContent();

					return (true, user.ClientId);
				}
			}
			catch (Exception)
			{
				return (false, "MbNoInternet")!;
			}
		}

		/// <summary>
		/// Отримуємо список користувачів
		/// </summary>
		/// <param name="isCustomDataNeeds">
		/// [Стан] Вказує чи додавати / не додавати
		/// специфічні дані користувача до списку</param>
		/// <param name="isAccountNeeds">
		/// [Стан] Вказує чи додавати / не додавати
		/// перелік рахунків користувача
		/// </param>
		/// <returns>Список користувачів Monoboard</returns>
		public static ObservableCollection<UserInfo> GetUserList(
			bool isCustomDataNeeds = false,
			bool isAccountNeeds = false)
		{
			var dbContext = new MonoboardDbContext();
			var userInfo = dbContext.UserInfo.AsParallel();

			if (isAccountNeeds)
				foreach (var user in userInfo)
					user.Accounts = dbContext.Accounts.AsParallel()
						.Where(account => account.AccountKey == user.Id).ToList();

			if (isCustomDataNeeds)
				foreach (var user in userInfo)
					user.MonoboardUser = dbContext.MonoBoardUsers
						.First(userdata => userdata.ClientId == user.ClientId);

			return new ObservableCollection<UserInfo>(userInfo);
		}

		/// <summary>
		/// Отримуємо дані конкретного користувача
		/// </summary>
		/// <param name="clientId">ID користувача</param>
		/// <param name="isGetAccounts">
		/// [Стан] Завантажувати / не завантажувати дані
		/// про картки/рахунки клієнта</param>
		/// <param name="isGetUserSettings">
		/// [Стан] Завантажувати / не завантажувати дані
		/// про особисті налаштування клієнта</param>
		/// <returns>Дані конкретного користувача</returns>
		public static async Task<UserInfo> GetUserAsync(
			string clientId,
			bool isGetAccounts = false,
			bool isGetUserSettings = false)
		{
			var db = new MonoboardDbContext();

			var user = await db.UserInfo.Where(info => info.ClientId == clientId)
				.Select(user => user).SingleAsync();

			if (isGetAccounts)
			{
				var accounts = await db.Accounts.Where(account => account.ClientId == clientId).ToListAsync();
				user.Accounts = accounts;
			}

			if (isGetUserSettings)
			{
				var monoboardUsers = await db.MonoBoardUsers.Where(account => account.ClientId == clientId).SingleAsync();
				user.MonoboardUser = monoboardUsers;
			}

			return user;
		}
	}
}
