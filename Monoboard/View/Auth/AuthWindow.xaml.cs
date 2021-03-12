using System;
using Monoboard.ViewModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using Monoboard.ViewModel.AuthViewModels;

namespace Monoboard.View.Auth
{
	public partial class AuthWindow
	{
		public AuthWindow() => InitializeComponent();

		/// <summary>
		/// Відкриває посилання на проект в GitHub
		/// </summary>
		private void GoToGitHubButtonClickHandler(object sender,
			RoutedEventArgs args) =>
			Process.Start(new ProcessStartInfo
			{
				FileName = "https://github.com/Oleksios/Monoboard",
				UseShellExecute = true
			});

		private void GoToAuthHelperClickHandler(object sender,
			RoutedEventArgs args) =>
			AuthTransitioner.SelectedIndex = 1;

		private void LanguageChangeClickHandler(object sender, RoutedEventArgs e)
		{
			var oldContinueAs = App.GetResourceValue("MbContinueAs");
			var oldSelectUser = App.GetResourceValue("MbSelectUsersAccount");

			App.Language = App.Language.Name switch
			{
				"uk-UA" => new CultureInfo("ru-RU"),
				"ru-RU" => new CultureInfo("en-US"),
				_ => new CultureInfo("uk-UA")
			};

			Properties.Settings.Default.Language = App.Language;
			Properties.Settings.Default.Save();


			// for change text property in AuthViewModel after language operation
			var text = ((AuthViewModel)AuthControl.DataContext).ContinueAsUser;
			if (text != null && text.Contains(oldSelectUser!))
			{
				if (((AuthViewModel)AuthControl.DataContext).TransitionIndex == 1)
					((AuthViewModel) AuthControl.DataContext).ContinueAsUser =
						App.GetResourceValue("MbSelectUsersAccount");
			}
			else
			{
				if (((AuthViewModel)AuthControl.DataContext).TransitionIndex == 1)
					((AuthViewModel) AuthControl.DataContext).ContinueAsUser = text?.Replace(oldContinueAs!,
						App.GetResourceValue("MbContinueAs"),
						StringComparison.CurrentCulture);
			}
		}
	}
}
