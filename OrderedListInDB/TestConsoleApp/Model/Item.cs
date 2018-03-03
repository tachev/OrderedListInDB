using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geo.Data.Tests
{
	public class Item : IIndexedItem<string>
	{
		public string Id { get; set; }
		public decimal Index { get; set; }
		public string NextId { get; set; }
		public string Value { get; set; }

		public override string ToString()
		{
			return Value;
		}
	}
}
