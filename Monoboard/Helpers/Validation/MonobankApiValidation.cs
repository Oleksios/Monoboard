using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Monoboard.Helpers.Validation
{
	internal class MonobankApiValidation : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo) =>
			string.IsNullOrWhiteSpace(value.ToString())
				? new ValidationResult(false, App.GetResourceValue("MbRequiredField"))
				: new Regex(@"^[\d\w-]{44}$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
					.Match((value as string)!).Success
					? ValidationResult.ValidResult
					: new ValidationResult(false, App.GetResourceValue("MbInvalidToken"));
	}
}
