﻿using Geo.Data.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geo.TestConsoleApp
{
	public static class Helper
	{
		public static async Task<TestOrderedList> CreateItems(int count)
		{
			Random random = new Random(0);
			var items = new TestOrderedList();
			await FillOrderedListAsync(items);

			for (int i = 0; i < count; i++)
			{
				var nextId = Database.LastId;
				if (i > 0)
				{
					var randomItem = ((Database)items.Database).FindItemByValue(random.Next(0, i).ToString());
					nextId = randomItem.Id;
				}

				var item = new Item
				{
					Id = Guid.NewGuid().ToString(),
					Value = i.ToString(),
					NextId = nextId
				};

				await items.InsertAsync(item);
			}

			return items;
		}
		public static async Task FillOrderedListAsync(TestOrderedList list)
		{
			for (int i = 1; i <= 5; i++)
			{
				var item = new Item
				{
					Id = Guid.NewGuid().ToString(),
					Value = i.ToString(),
					NextId = Database.LastId
				};

				await list.InsertAsync(item);
			}
		}
	}
}
