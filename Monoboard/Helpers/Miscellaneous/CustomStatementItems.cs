using MonoboardCore.Model;
using System.Collections.Generic;
using System.Windows.Media;

namespace Monoboard.Helpers.Miscellaneous
{
	public class CustomStatement
	{
		public string? Date { get; set; }
		public List<CustomView>? StatementItemsView { get; set; }

	}

	public class CustomView
	{
		public Brush Foreground { get; set; }
		public StatementItem StatementItem { get; set; }
	}
}
