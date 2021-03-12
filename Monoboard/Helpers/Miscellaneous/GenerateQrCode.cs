using Monoboard.Properties;
using QRCoder;
using System;
using System.Web;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Monoboard.Helpers.Miscellaneous
{
	public static class GenerateQrCode
	{
		/// <summary>
		/// Генерує QR код та посилання на поповнення рахунку
		/// </summary>
		/// <param name="amount">Сума для переказу коштів (не обов'язково)</param>
		/// <param name="comment">Коментар (не обов'язково)</param>
		/// <returns>1 - Посилання, 2 - Qr-код</returns>
		public static (string link, ImageSource imageSource) Generate(string amount = "", string comment = "")
		{
			var url = "https://send.monobank.ua/" + Settings.Default.ClientId;

			var parameters = "";

			if (string.IsNullOrEmpty(amount) is false
				|| string.IsNullOrWhiteSpace(amount) is false)
			{
				parameters += "?a=" + HttpUtility.UrlEncode(amount);

				if (string.IsNullOrEmpty(comment) is false
					|| string.IsNullOrWhiteSpace(comment) is false)
					parameters += "&t=" + HttpUtility.UrlEncode(comment);
			}
			else if (string.IsNullOrEmpty(comment) is false
					 || string.IsNullOrWhiteSpace(comment) is false)
				parameters += "?t=" + HttpUtility.UrlEncode(comment);

			url += parameters;

			PayloadGenerator.Url generator = new(url);
			string payload = generator.ToString();

			QRCodeGenerator qrGenerator = new();
			QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
			QRCode qrCode = new(qrCodeData);

			var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
				qrCode.GetGraphic(20).GetHbitmap(),
				IntPtr.Zero,
				Int32Rect.Empty,
				BitmapSizeOptions.FromWidthAndHeight(300, 300));

			return (url, bitmapSource);
		}
	}
}
