using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geo.Data.Tests
{
	public class TestOrderedList : OrderedList<Item, string, string>
	{
		private int reindexCount;
		public TestOrderedList(): base(new Database())
		{
		}

		public TestOrderedList(Database database):base(database)
		{
		}

		protected override async Task ReindexItemsAsync(Item item)
		{
			var index = item.Index;

			var startTime = DateTime.Now;

			await base.ReindexItemsAsync(item);

			var itemsCount = ((Database)Database).CountAll();
			reindexCount++;

			System.Diagnostics.Trace.WriteLine($"Reindex # {reindexCount}. Items in the database {itemsCount}. Time for Reindex {DateTime.Now - startTime}");
		}
	}
}
