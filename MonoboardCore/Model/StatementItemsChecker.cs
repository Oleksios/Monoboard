using System;

namespace MonoboardCore.Model
{
	public class StatementItemsChecker
	{
		public int StatementItemsCheckerId { get; set; }
		public string CardCode { get; set; }
		public bool IsEnd { get; set; }
		public DateTime Month { get; set; }
	}
}
