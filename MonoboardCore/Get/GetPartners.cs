using Microsoft.EntityFrameworkCore;
using MonoboardCore.Model;
using Newtonsoft.Json;
using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MonoboardCore.Get
{
	public static class GetPartners
	{
		/// <summary>
		/// Завантажує дані про партнерів Monobank (Покупка частинами)
		/// </summary>
		/// <returns>Завантажена з API Monobank інформація про користувача</returns>
		public static async Task<(List<Partner> partners, string message)> GetInfoAsync()
		{
			var clientApi = RestClient.For<IPartnersApi>("https://goose.ml");

			try
			{
				using (var response = await clientApi.GetPartnersInfoAsync())
				{
					if (response.ResponseMessage.StatusCode != HttpStatusCode.OK)
						return (null, JsonConvert.DeserializeObject<ErrorInformation>(response.StringContent).Error.Message)!;

					return (response.GetContent(), "");
				}
			}
			catch (Exception)
			{
				return (null, "MbNoInternet")!;
			}
		}

		/// <summary>
		/// Отримає список партнерів з інформацією про них
		/// </summary>
		/// <returns>Інформація про партнерів Monobank</returns>
		public static async Task<List<Partner>> GetListAsync()
		{
			var db = new MonoboardDbContext();

			var partners = await db.Partners
				.Select(partner => partner)
				.ToListAsync();

			foreach (var partner in partners)
				partner.Categories = await db.PartnersCategories
					.Where(category => category.PartnerKey == partner.Id)
					.ToListAsync();

			return partners;
		}
	}
}
