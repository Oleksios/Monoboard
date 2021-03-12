using System.Runtime.InteropServices;

namespace MonoboardCore.Hepler
{
	public static class Internet
	{
		[DllImport("wininet.dll")]
		private static extern bool InternetGetConnectedState(out int description, int reservedValue);
		public static bool IsConnectedToInternet() => InternetGetConnectedState(out _, 0);
	}
}
