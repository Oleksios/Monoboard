using System;
using System.Globalization;

namespace MonoboardCore.Hepler
{
	/// <summary>
	/// Реалізує можливості для конвертації DateTime в TimeStamp і навпаки
	/// </summary>
	public static class UnixConverter
	{
		/// <summary>
		/// Переводить Timestamp в формат DateTime
		/// </summary>
		/// <param name="unixTime">Значення Timestamp</param>
		/// <returns>Значення DateTime</returns>
		public static DateTime UnixTimestampToDateTime(long unixTime) =>
			new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
				.AddSeconds(unixTime);

		/// <summary>
		/// Переводить DateTime в формат Timestamp (у вигляді тексту)
		/// </summary>
		/// <param name="dateTime">Значення DateTime</param>
		/// <returns>Значення Timestamp</returns>
		public static string DateTimeToUnixTimestampString(DateTime dateTime)
		{
			var elapsedTime = dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return elapsedTime.TotalSeconds.ToString(CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Переводить DateTime в формат Timestamp
		/// </summary>
		/// <param name="dateTime">Значення DateTime</param>
		/// <returns>Значення UnixTimestamp</returns>
		public static long DateTimeToUnixTimestamp(DateTime dateTime)
		{
			var elapsedTime = dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return (long) elapsedTime.TotalSeconds;
		}
	}
}
