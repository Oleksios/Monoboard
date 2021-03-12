using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Monoboard.Helpers.Validation
{
	internal class OnlyNumberValidation : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			return value != null
				? string.IsNullOrWhiteSpace(value.ToString()) || string.IsNullOrEmpty(value.ToString())
					? new ValidationResult(false, App.GetResourceValue("MbRequiredField"))
					: new Regex(@"^\d+[\.\,]?\d*$").Match(value.ToString()!).Success is false
						? new ValidationResult(false, App.GetResourceValue("MbFieldOnlyNumber"))
						: ValidationResult.ValidResult
				: new ValidationResult(false, App.GetResourceValue("MbRequiredField"));
		}
	}
}
