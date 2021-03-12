using System.Security.Cryptography;
using System.Text;

namespace MonoboardCore.Hepler
{
	public static class Sha256Hash
	{
		public static string Compute(string? rawData)
		{
			// Створюємо SHA256   
			using var sha256Hash = SHA256.Create();
			var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData ?? ""));

			// Конвертуємо масив байтів в текст  
			var builder = new StringBuilder();
			foreach (var t in bytes) builder.Append(t.ToString("x2"));
			return builder.ToString();
		}
	}
}
