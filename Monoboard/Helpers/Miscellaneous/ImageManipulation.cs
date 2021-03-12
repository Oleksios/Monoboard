using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MonoboardCore.Hepler;

namespace Monoboard.Helpers.Miscellaneous
{
	public static class ImageManipulation
	{
		public static async Task<string> GetImageAsBase64Url(string url)
		{
			if (Internet.IsConnectedToInternet() is false)
				return "";

			using var handler = new HttpClientHandler();
			using var client = new HttpClient(handler);
			var bytes = await client.GetByteArrayAsync(url);
			return Convert.ToBase64String(bytes);
		}

		public static BitmapImage GetBitmapImage(byte[] binaryData)
		{
			var bitmapImage = new BitmapImage();
			bitmapImage.BeginInit();
			bitmapImage.StreamSource = new MemoryStream(binaryData);
			bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
			bitmapImage.EndInit();

			return bitmapImage;
		}

		public static string BitmapToBase64(BitmapImage bitmapImage)
		{
			var memoryStream = new MemoryStream();
			var encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
			encoder.Save(memoryStream);
			var bitmapdata = memoryStream.ToArray();

			return Convert.ToBase64String(bitmapdata);
		}
	}
}
