#nullable enable
using MaterialDesignThemes.Wpf;
using Monoboard.Properties;
using MonoboardCore;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace Monoboard
{
	public partial class App
	{
		/// <summary>
		/// Ресурс для локалізації
		/// </summary>
		private static ResourceDictionary _resourceDictionaryValue = Current.Resources.MergedDictionaries.First();

		private static DateTime _infoDelay = DelaySettings.Default.InfoDelay;
		private static DateTime _otherDelay = DelaySettings.Default.OtherDelay;

		/// <summary>
		/// Час затримки для отримання виписки / інформацію про користувача
		/// </summary>
		public static DateTime InfoDelay
		{
			get => _infoDelay;
			set
			{
				DelaySettings.Default.InfoDelay = _infoDelay = value;
				DelaySettings.Default.Save();
			}
		}

		/// <summary>
		/// Час затримки для отримання даних валют
		/// </summary>
		public static DateTime OtherDelay
		{
			get => _otherDelay;
			set
			{
				DelaySettings.Default.OtherDelay = _otherDelay = value;
				DelaySettings.Default.Save();
			}
		}

		private static bool IsAppLoaded { get; set; }

		public App()
		{
			InitializeComponent();

			FirstLoad();

			IsAppLoaded = true;
		}

		/// <summary>
		/// Отримує локалізований текст за ключем локалізації
		/// </summary>
		/// <param name="resourceKey">Ключ локалізації</param>
		/// <returns>Локалізований текст</returns>
		public static string? GetResourceValue(string resourceKey) => _resourceDictionaryValue[resourceKey] as string;

		/// <summary>
		/// Налаштування додатку при запуску
		/// </summary>
		private static void FirstLoad()
		{
			if (string.IsNullOrEmpty(Settings.Default.MonobankToken))
			{
				var webviewData = new DirectoryInfo("Monoboard.exe.WebView2");

				if (webviewData.Exists) webviewData.Delete(true);
			}

			if (!File.Exists("Monoboard.db"))
			{
				using var dbContext = new MonoboardDbContext();

				dbContext.CreateDatabase();

				Settings.Default.Reset();
				PaletteSettings.Default.Reset();
			}

			var userAvailable = !string.IsNullOrEmpty(Settings.Default.MonobankToken) &&
								!string.IsNullOrEmpty(Settings.Default.UserName) &&
								!string.IsNullOrEmpty(Settings.Default.ClientId);

			Current.StartupUri = userAvailable
				? new Uri("View/MainWindow.xaml",
					UriKind.Relative)
				: new Uri("View/Auth/AuthWindow.xaml",
					UriKind.Relative);

			#region Theme

			if (!PaletteSettings.Default.Loaded)
			{
				if (userAvailable)
				{
					var (isDark, primaryColor, secondaryColor) = MonoboardCore.Get.GetMonoboardUser
						.GetColorThemeAsync(Settings.Default.ClientId)
						.Result;

					ApplyTheme(PaletteSettings.Default.IsDark = isDark);

					if (!primaryColor.Contains("#"))
						primaryColor = $"#{primaryColor}";


					PaletteSettings.Default.PrimaryColor = System.Drawing.Color.FromName(primaryColor);

					if (!string.Equals(primaryColor, "#fff43b3f", StringComparison.InvariantCultureIgnoreCase))
					{
						if (PaletteSettings.Default.PrimaryColor.Name.Contains("#"))
							ModifyTheme(theme =>
								theme.SetPrimaryColor(
									(Color)ColorConverter.ConvertFromString(PaletteSettings.Default.PrimaryColor.Name)!));
						else
							ModifyTheme(theme =>
								theme.SetPrimaryColor(
									(Color)ColorConverter.ConvertFromString($"#{PaletteSettings.Default.PrimaryColor.Name}")!));
					}

					if (!secondaryColor.Contains("#"))
						secondaryColor = $"#{secondaryColor}";

					//PaletteSettings.Default.SecondaryColor = (System.Drawing.Color)ColorConverter.ConvertFromString(secondaryColor)!;
					PaletteSettings.Default.SecondaryColor = System.Drawing.Color.FromName(secondaryColor);

					if (!string.Equals(secondaryColor, "#ff06b206", StringComparison.InvariantCultureIgnoreCase))
					{
						if (PaletteSettings.Default.SecondaryColor.Name.Contains("#"))
							ModifyTheme(theme =>
								theme.SetSecondaryColor(
									(Color)ColorConverter.ConvertFromString(PaletteSettings.Default.SecondaryColor.Name)!));
						else
							ModifyTheme(theme =>
								theme.SetSecondaryColor(
									(Color)ColorConverter.ConvertFromString($"#{PaletteSettings.Default.SecondaryColor.Name}")!));
					}

					PaletteSettings.Default.Loaded = true;

					PaletteSettings.Default.Save();
				}
			}
			else
			{
				ApplyTheme(PaletteSettings.Default.IsDark);

				if (!string.Equals(PaletteSettings.Default.PrimaryColor.Name, "#fff43b3f", StringComparison.InvariantCultureIgnoreCase))
				{
					if (PaletteSettings.Default.PrimaryColor.Name.Contains("#"))
						ModifyTheme(theme =>
							theme.SetPrimaryColor(
								(Color)ColorConverter.ConvertFromString(PaletteSettings.Default.PrimaryColor.Name)!));
					else
						ModifyTheme(theme =>
							theme.SetPrimaryColor(
								(Color)ColorConverter.ConvertFromString($"#{PaletteSettings.Default.PrimaryColor.Name}")!));
				}

				if (!string.Equals(PaletteSettings.Default.SecondaryColor.Name, "#ff06b206", StringComparison.InvariantCultureIgnoreCase))
				{
					if (PaletteSettings.Default.SecondaryColor.Name.Contains("#"))
						ModifyTheme(theme =>
							theme.SetSecondaryColor(
								(Color)ColorConverter.ConvertFromString(PaletteSettings.Default.SecondaryColor.Name)!));
					else
						ModifyTheme(theme =>
							theme.SetSecondaryColor(
								(Color)ColorConverter.ConvertFromString($"#{PaletteSettings.Default.SecondaryColor.Name}")!));
				}
			}

			#endregion
		}

		/// <summary>
		/// Перемикає світлу тему на темну і навпаки
		/// </summary>
		/// <param name="isDark">[Стан] Темна тема: Ввімкнена/Вимкнена</param>
		public static void ApplyTheme(bool isDark)
		{
			var paletteHelper = new PaletteHelper();

			ITheme theme = paletteHelper.GetTheme();

			IBaseTheme baseTheme = isDark ? new MaterialDesignDarkTheme() : new MaterialDesignLightTheme() as IBaseTheme;

			theme.SetBaseTheme(baseTheme);

			paletteHelper.SetTheme(theme);
		}

		/// <summary>
		/// Модифікує кольори теми за бажанням користувача
		/// </summary>
		public static void ModifyTheme(Action<ITheme> modificationAction)
		{
			var paletteHelper = new PaletteHelper();

			var theme = paletteHelper.GetTheme();

			modificationAction(theme);

			paletteHelper.SetTheme(theme);

			if (!IsAppLoaded) return;

			try
			{
				if (!string.Equals(PaletteSettings.Default.PrimaryColor.Name,
					theme.PrimaryMid.Color.ToString().ToLowerInvariant(),
					StringComparison.InvariantCultureIgnoreCase))
				{
					PaletteSettings.Default.PrimaryColor = System.Drawing.Color.FromName(theme.PrimaryMid.Color.ToString());

					MonoboardCore.Set.SetMonoboardUserData.UpdatePrimaryColor(
						Settings.Default.ClientId,
						PaletteSettings.Default.PrimaryColor.Name);
				}

				if (!string.Equals(PaletteSettings.Default.SecondaryColor.Name,
					theme.SecondaryMid.Color.ToString().ToLowerInvariant(),
					StringComparison.InvariantCultureIgnoreCase))
				{
					PaletteSettings.Default.SecondaryColor = System.Drawing.Color.FromName(theme.SecondaryMid.Color.ToString());

					MonoboardCore.Set.SetMonoboardUserData.UpdateSecondaryColor(
						Settings.Default.ClientId,
						PaletteSettings.Default.SecondaryColor.Name);
				}
			}
			finally
			{
				PaletteSettings.Default.Save();
			}
		}

		/// <summary>
		/// Мова додатку
		/// </summary>
		public static CultureInfo Language
		{
			get
			{
				if (Thread.CurrentThread.CurrentUICulture == Settings.Default.Language)
					return Thread.CurrentThread.CurrentUICulture;
				
				Thread.CurrentThread.CurrentUICulture = Settings.Default.Language;

				ChangelanguageResource(Settings.Default.Language);

				return Thread.CurrentThread.CurrentUICulture;
			}
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));
				if (Equals(value, Settings.Default.Language)) return;

				ChangelanguageResource(value);
			}
		}

		/// <summary>
		/// Змінює моі застосунку та ресурси, що з ним пов'язані
		/// </summary>
		/// <param name="culture">Вибрана нова культура застосунку</param>
		private static void ChangelanguageResource(CultureInfo culture)
		{
			//1. Змінюємо мову застосунку:
			Thread.CurrentThread.CurrentUICulture = culture;
			//CultureInfo.CurrentUICulture = value;

			//2. Створюємо ResourceDictionary для нової культури
			var dict = new ResourceDictionary
			{
				Source = culture.Name switch
				{
					"ru-RU" => new Uri($"/Resources/Localization/lang.{culture.Name}.xaml", UriKind.Relative),
					"en-US" => new Uri($"/Resources/Localization/lang.{culture.Name}.xaml", UriKind.Relative),
					_ => new Uri("/Resources/Localization/lang.xaml", UriKind.Relative)
				}
			};

			var currencyDict = new ResourceDictionary
			{
				Source = culture.Name switch
				{
					"ru-RU" => new Uri($"/Resources/Localization/Currency/CurrencyResources.{culture.Name}.xaml", UriKind.Relative),
					"en-US" => new Uri($"/Resources/Localization/Currency/CurrencyResources.{culture.Name}.xaml", UriKind.Relative),
					_ => new Uri("/Resources/Localization/Currency/CurrencyResources.xaml", UriKind.Relative)
				}
			};

			//3. Знаходимо старий ResourceDictionary і видаляємо його, а також додаємо новий ResourceDictionary
			var oldDict = Current.Resources.MergedDictionaries.First(d =>
				d.Source != null && d.Source.OriginalString.StartsWith("/Resources/Localization/lang."));

			var oldCurrencyDict = Current.Resources.MergedDictionaries.First(d =>
				d.Source != null && d.Source.OriginalString.StartsWith("/Resources/Localization/Currency/CurrencyResources."));

			if (oldDict != null && oldCurrencyDict != null)
			{
				var ind = Current.Resources.MergedDictionaries.IndexOf(oldDict);
				var indCurrDict = Current.Resources.MergedDictionaries.IndexOf(oldCurrencyDict);

				Current.Resources.MergedDictionaries.Remove(oldDict);
				Current.Resources.MergedDictionaries.Insert(ind, dict);
				Current.Resources.MergedDictionaries.Insert(indCurrDict, currencyDict);
				_resourceDictionaryValue = Current.Resources.MergedDictionaries[ind];
			}
			else
			{
				Current.Resources.MergedDictionaries.Add(dict);
				_resourceDictionaryValue = dict;
				Current.Resources.MergedDictionaries.Add(currencyDict);
			}

			//4. Викликаємо івент для сповіщення всіх вікон.
			LanguageChanged(Current, new EventArgs());
		}

		/// <summary>
		/// Івент для сповіщення всіх вікон додатку
		/// </summary>
		public static event EventHandler LanguageChanged = LanguageValueChanged;

		/// <summary>
		/// Зберігає змінену мову
		/// </summary>
		private static void LanguageValueChanged(object? sender, EventArgs e)
		{
			Settings.Default.Language = Thread.CurrentThread.CurrentUICulture;

			Settings.Default.Save();
		}
	}
}
