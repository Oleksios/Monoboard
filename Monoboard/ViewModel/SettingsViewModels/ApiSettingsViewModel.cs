using Monoboard.Helpers;
using Monoboard.Properties;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Monoboard.Helpers.Formatter;
using MonoboardCore.Hepler;

namespace Monoboard.ViewModel.SettingsViewModels
{
	public partial class SettingsViewModel
	{
		public ICommand SaveMonobankTokenCommand { get; }

		public async Task SaveAccessTokenAsync(string token)
		{
			if (Internet.IsConnectedToInternet() is false)
			{
				Messages.Clear();
				Messages.Enqueue(App.GetResourceValue("MbNoInternet")!);

				return;
			}

			Messages.Clear();

			if (Settings.Default.MonobankToken == token)
			{
				Messages.Enqueue(App.GetResourceValue("MbSameToken")!);
				return;
			}

			if (TimeWait() <= TimeSpan.Zero) App.InfoDelay = DateTime.Parse("01.01.21");
			
			if (App.InfoDelay == DateTime.Parse("01.01.21"))
				if (await MonoboardCore.Set.SetMonoboardUserData
				.ChangeAccessTokenAsync(Settings.Default.ClientId, token))
				{
					var (userInfo, message) = await MonoboardCore.Get.GetUserInfo.GetClientInfoAsync(token);

					StartDelay();

					if (userInfo != null)
					{
						var user = await MonoboardCore.Get.GetUserInfo.GetUserAsync(
							Settings.Default.ClientId,
							true,
							true);

						user.Accounts = AccountsDecorator.Merge(user.Accounts, userInfo.Accounts);

						user.MonoboardUser.ClientId = userInfo.ClientId;

						if (await MonoboardCore.Set.SetUserInfo.UpdateUserAsync(user))
							Messages.Enqueue(App.GetResourceValue("MbTokenChanged")!);
						else
							Messages.Enqueue(App.GetResourceValue("MbSaveDataError")!);
					}
					else
						Messages.Enqueue(
							$"{App.GetResourceValue("MbSaveDataError")}" +
							$"\n{App.GetResourceValue("MbCauseInfo")} {message}");

					Settings.Default.MonobankToken = token;
					Settings.Default.Save();

					Messages.Enqueue(App.GetResourceValue("MbTokenChanged")!);
				}
				else
					Messages.Enqueue(App.GetResourceValue("MbSaveDataError")!);
			else
			{
				var startMessage = App.GetResourceValue("MbGetDataWaitStart");

				if (App.Language.Name == "uk-UA" || App.Language.Name == "ru-RU")
					if (TimeWait().Seconds.ToString().Contains("11") ||
					    TimeWait().Seconds.ToString().Contains("12") ||
					    TimeWait().Seconds.ToString().Contains("13") ||
					    TimeWait().Seconds.ToString().Contains("14"))
						Messages.Enqueue(
							$"{startMessage} {TimeWait().Seconds} {App.GetResourceValue("MbGetDataWaitEnd")}");
					else if (TimeWait().Seconds.ToString().EndsWith("1"))
						Messages.Enqueue(
							$"{startMessage} {TimeWait().Seconds} {App.GetResourceValue("MbGetDataWaitEndAlt")}");
					else if (TimeWait().Seconds.ToString().EndsWith("2") ||
					         TimeWait().Seconds.ToString().EndsWith("3") ||
					         TimeWait().Seconds.ToString().EndsWith("4"))
						Messages.Enqueue(
							$"{startMessage} {TimeWait().Seconds} {App.GetResourceValue("MbGetDataWaitEndAlt2")}");
					else
						Messages.Enqueue(
							$"{startMessage} {TimeWait().Seconds} {App.GetResourceValue("MbGetDataWaitEnd")}");
				else
					Messages.Enqueue(
						$"{startMessage} {TimeWait().Seconds} {App.GetResourceValue("MbGetDataWaitEnd")}");
			}
		}
	}
}
