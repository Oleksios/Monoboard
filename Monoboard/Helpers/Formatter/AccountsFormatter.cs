using MonoboardCore.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Monoboard.Helpers.Formatter
{
	/// <summary>
	/// Модифікація рахунків користувача
	/// </summary>
	public static class AccountsDecorator
	{
		/// <summary>
		/// Приводить сирі дані рахунку що надійшли від API Monobank до читабельного вигляду
		/// </summary>
		/// <param name="accounts">Рахунки користувача</param>
		/// <returns>Модифіковані рахунки в читабельному вигляді</returns>
		public static IList<Account> Decorate(IList<Account> accounts)
		{
			foreach (var account in accounts)
			{
				if (string.IsNullOrEmpty(account.CustomCardName) &&
					string.IsNullOrEmpty(account.CustomCardDescription))
				{
					try
					{
						if (account.CardType == nameof(CardType.Black).ToLowerInvariant())
						{
							account.CustomCardName += App.GetResourceValue("MbBlackCard");
							account.CustomCardDescription = App.GetResourceValue("MbBlackCardDescription");
						}
						else if (account.CardType == nameof(CardType.Iron).ToLowerInvariant())
						{
							account.CustomCardName += App.GetResourceValue("MbIronCard");
							account.CustomCardDescription = App.GetResourceValue("MbIronCardDescription");
						}
						else if (account.CardType == nameof(CardType.Platinum).ToLowerInvariant())
						{
							account.CustomCardName += App.GetResourceValue("MbPlatinumCard");
							account.CustomCardDescription = App.GetResourceValue("MbPlatinumCardDescription");
						}
						else if (account.CardType == nameof(CardType.White).ToLowerInvariant())
						{
							account.CustomCardName += App.GetResourceValue("MbWhiteCard");
							account.CustomCardDescription = App.GetResourceValue("MbWhiteCardDescription");
						}
						else if (account.CardType == nameof(CardType.Yellow).ToLowerInvariant())
						{
							account.CustomCardName += App.GetResourceValue("MbYellowCard");
							account.CustomCardDescription = App.GetResourceValue("MbYellowCardDescription");
						}
						else if (account.CardType == nameof(CardType.Fop).ToLowerInvariant())
						{
							account.CustomCardName += App.GetResourceValue("MbFopCard");
							account.CustomCardDescription = App.GetResourceValue("MbFopCardDescription");
						}

						account.CustomCardName += " ";

						account.CustomCardName += account.CurrencyCode switch
						{
							980 => App.GetResourceValue("MbUahCard"),
							840 => App.GetResourceValue("MbUsdCard"),
							978 => App.GetResourceValue("MbEurCard"),
							985 => $"{App.GetResourceValue("MbCard")} {App.GetResourceValue("MbPlnCard")}",
							_ => ""
						};
					}
					finally
					{
						if (account.CurrencyCode != 985)
						{
							account.CustomCardName += " ";
							account.CustomCardName += App.GetResourceValue("MbCard")!;
						}
					}
				}

				var currencySymbol = account.CurrencyCode switch
				{
					980 => "uk-UA",
					840 => "en-US",
					978 => "nl-BE",
					985 => "pl-PL"
				};

				account.BalanceFormat = decimal
					.Parse((double.Parse(account.Balance.ToString("F2")) / 100)
						.ToString(new CultureInfo(currencySymbol)))
					.ToString("C2", new CultureInfo(currencySymbol));

				account.CreditLimitFormat = decimal
					.Parse((double.Parse(account.CreditLimit.ToString("F2")) / 100)
						.ToString(new CultureInfo(currencySymbol)))
					.ToString("C2", new CultureInfo(currencySymbol));
			}

			return accounts;
		}

		/// <summary>
		/// Приводить сирі дані рахунку що надійшли від API Monobank до читабельного вигляду
		/// </summary>
		/// <param name="account">Рахунок користувача</param>
		/// <returns>Модифікований рахунок в читабельному вигляді</returns>
		public static Account Decorate(Account account)
		{
			if ((string.IsNullOrEmpty(account.CustomCardName) || string.IsNullOrWhiteSpace(account.CustomCardName)) &&
				string.IsNullOrEmpty(account.CustomCardDescription) || string.IsNullOrWhiteSpace(account.CustomCardName))
			{
				try
				{
					if (account.CardType == nameof(CardType.Black).ToLowerInvariant())
					{
						account.CustomCardName += App.GetResourceValue("MbBlackCard");
						account.CustomCardDescription = App.GetResourceValue("MbBlackCardDescription");
					}
					else if (account.CardType == nameof(CardType.Iron).ToLowerInvariant())
					{
						account.CustomCardName += App.GetResourceValue("MbIronCard");
						account.CustomCardDescription = App.GetResourceValue("MbIronCardDescription");
					}
					else if (account.CardType == nameof(CardType.Platinum).ToLowerInvariant())
					{
						account.CustomCardName += App.GetResourceValue("MbPlatinumCard");
						account.CustomCardDescription = App.GetResourceValue("MbPlatinumCardDescription");
					}
					else if (account.CardType == nameof(CardType.White).ToLowerInvariant())
					{
						account.CustomCardName += App.GetResourceValue("MbWhiteCard");
						account.CustomCardDescription = App.GetResourceValue("MbWhiteCardDescription");
					}
					else if (account.CardType == nameof(CardType.Yellow).ToLowerInvariant())
					{
						account.CustomCardName += App.GetResourceValue("MbYellowCard");
						account.CustomCardDescription = App.GetResourceValue("MbYellowCardDescription");
					}
					else if (account.CardType == nameof(CardType.Fop).ToLowerInvariant())
					{
						account.CustomCardName += App.GetResourceValue("MbFopCard");
						account.CustomCardDescription = App.GetResourceValue("MbFopCardDescription");
					}

					account.CustomCardName += " ";

					account.CustomCardName += account.CurrencyCode switch
					{
						980 => App.GetResourceValue("MbUahCard"),
						840 => App.GetResourceValue("MbUsdCard"),
						978 => App.GetResourceValue("MbEurCard"),
						985 => $"{App.GetResourceValue("MbCard")} {App.GetResourceValue("MbPlnCard")}",
						_ => ""
					};
				}
				finally
				{
					if (account.CurrencyCode != 985)
					{
						account.CustomCardName += " ";
						account.CustomCardName += App.GetResourceValue("MbCard")!;
					}
				}
			}

			var currencySymbol = account.CurrencyCode switch
			{
				980 => "uk-UA",
				840 => "en-US",
				978 => "nl-BE",
				985 => "pl-PL"
			};

			account.BalanceFormat = decimal
				.Parse((double.Parse(account.Balance.ToString("F2")) / 100)
					.ToString(new CultureInfo(currencySymbol)))
				.ToString("C2", new CultureInfo(currencySymbol));

			account.CreditLimitFormat = decimal
				.Parse((double.Parse(account.CreditLimit.ToString("F2")) / 100)
					.ToString(new CultureInfo(currencySymbol)))
				.ToString("C2", new CultureInfo(currencySymbol));

			return account;
		}

		/// <summary>
		/// Об'єднує дані рахунку що надійшли від API Monobank з існуючими даними
		/// </summary>
		/// <param name="accounts">Теперішні дані рахунку користувача</param>
		/// <param name="newAccounts">Нові дані рахунку користувача</param>
		/// <returns>Модифіковані рахунки в читабельному вигляді</returns>
		public static IList<Account> Merge(IList<Account> accounts, IList<Account> newAccounts)
		{
			List<string> oldCardCodes = accounts.Select(t => t.CardCode).ToList();
			List<string> newCardCodes = newAccounts.Select(t => t.CardCode).ToList();

			var elementForAdded = newCardCodes.Except(oldCardCodes);
			var elementForDelete = oldCardCodes.Except(newCardCodes);

			if (elementForAdded != null && elementForAdded.Any())
				for (var i = 0; i < elementForAdded.Count(); i++)
					accounts.Add(Decorate(newAccounts.Single(account => account.CardCode == elementForAdded.ElementAt(i))));

			if (elementForDelete != null && elementForDelete.Any())
				for (var i = 0; i < elementForDelete.Count(); i++)
				foreach (var account in accounts)
					if (account.CardCode == elementForDelete.ElementAt(i))
						account.IsDeleted = true;

			foreach (var account in accounts)
			{
				if (account.IsDeleted is false)
				{
					var accountData = newAccounts.Single(userAccount => userAccount.CardCode == account.CardCode);

					account.ClientId = accountData.ClientId;

					account.Balance = accountData.Balance;

					account.CreditLimit = accountData.CreditLimit;

					var currencySymbol = account.CurrencyCode switch
					{
						980 => "uk-UA",
						840 => "en-US",
						978 => "nl-BE",
						985 => "pl-PL"
					};

					account.BalanceFormat = decimal
						.Parse((double.Parse(account.Balance.ToString("F2")) / 100)
							.ToString(new CultureInfo(currencySymbol)))
						.ToString("C2", new CultureInfo(currencySymbol));

					account.CreditLimitFormat = decimal
						.Parse((double.Parse(account.CreditLimit.ToString("F2")) / 100)
							.ToString(new CultureInfo(currencySymbol)))
						.ToString("C2", new CultureInfo(currencySymbol));
				}
			}

			return accounts;
		}
	}
}
