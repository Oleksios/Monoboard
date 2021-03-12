#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.RegularExpressions;

namespace Monoboard.Helpers.Validation
{
	public static class PasswordValidation
	{
		/// <summary>
		/// Порівнює два поля типу "конфіденційний текст" на рівність
		/// </summary>
		/// <param name="s1">Конфіденційний текст №1</param>
		/// <param name="s2">Конфіденційний текст №2</param>
		/// <returns>[Стан] Рівність досягнена / не досягнена</returns>
		public static bool Compare(SecureString? s1, SecureString? s2)
		{
			if (s1 == null)
				return false;

			if (s2 == null)
				return false;

			if (s1.Length != s2.Length)
				return false;

			var bstr1 = IntPtr.Zero;
			var bstr2 = IntPtr.Zero;

			RuntimeHelpers.PrepareConstrainedRegions();

			try
			{
				bstr1 = Marshal.SecureStringToBSTR(s1);
				bstr2 = Marshal.SecureStringToBSTR(s2);

				unsafe
				{
					for (char* ptr1 = (char*)bstr1.ToPointer(), ptr2 = (char*)bstr2.ToPointer();
						*ptr1 != 0 && *ptr2 != 0;
						++ptr1, ++ptr2)
						if (*ptr1 != *ptr2)
							return false;
				}

				return true;
			}
			finally
			{
				if (bstr1 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr1);

				if (bstr2 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr2);
			}
		}

		/// <summary>
		/// Перевіряє правильність вводу паролю
		/// </summary>
		/// <param name="password">Пароль</param>
		/// <param name="isEquals">Паролі співпадають / не співпадають</param>
		/// <returns>Повідомлення щодо правильності вводу паролю</returns>
		public static (string? resourceValue, bool validate) Validate(string? password, bool isEquals = true)
		{
			var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\-_\.])[A-Za-z\d@$!%*?&\-_\.]{8,}$");

			var resourceValue = string.IsNullOrWhiteSpace(password)
				? App.GetResourceValue("MbPasswordEmpty")
				: regex.Match(password).Success is false
					? App.GetResourceValue("MbPasswordNeed")
					: !isEquals
						? App.GetResourceValue("MbPasswordError")
						: App.GetResourceValue("MbPasswordSuccess");

			return resourceValue == App.GetResourceValue("MbPasswordSuccess")
				? (resourceValue, true)
				: (resourceValue, false);

		}
	}
}
