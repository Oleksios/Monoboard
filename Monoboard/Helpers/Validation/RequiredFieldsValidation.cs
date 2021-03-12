using System.Globalization;
using System.Windows.Controls;

namespace Monoboard.Helpers.Validation
{
	internal class RequiredFieldsValidation : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			if (value != null)
			{
				return string.IsNullOrWhiteSpace(value.ToString()) || string.IsNullOrEmpty(value.ToString())
					? new ValidationResult(false, App.GetResourceValue("MbRequiredField"))
					: value!.ToString()!.Length < 6
						? new ValidationResult(false, App.GetResourceValue("MbFieldMinSixChar"))
						: ValidationResult.ValidResult;
			}
			else
				return new ValidationResult(false, App.GetResourceValue("MbRequiredField"));
		}
	}
}
