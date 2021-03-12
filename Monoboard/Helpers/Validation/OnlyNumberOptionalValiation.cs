using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Monoboard.Helpers.Validation
{
	internal class OnlyNumberOptionalValiation : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo) =>
			value != null
				? string.IsNullOrEmpty(value.ToString()) is false || string.IsNullOrWhiteSpace(value.ToString()) is false
					? new Regex(@"^\d+[\.]?\d*$").Match(value.ToString()!).Success is false
						? new ValidationResult(false, App.GetResourceValue("MbFieldOnlyNumber"))
						: int.Parse(value.ToString()!) >= 100
							? ValidationResult.ValidResult
							: new ValidationResult(false, App.GetResourceValue("MbMinAmount100"))
					: ValidationResult.ValidResult
				: ValidationResult.ValidResult;
	}
}
