using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Geo.TestConsoleApp;

namespace Geo.Data.Tests
{
	[TestClass]
	public class OrderedListsInDBTests
	{
		[ClassInitialize]
		public static void ClassInitialize(TestContext tc)
		{
		}

		[TestMethod]
		public async Task ReindexItemsWorstCase()
		{
			var items = new TestOrderedListWorstCase();
			await Helper.FillOrderedListAsync(items);
			await TestReindex(items);
		}

		[TestMethod]
		public async Task ReindexItems()
		{
			var items = await Helper.CreateItems(30000);
			await AssertOrderAsync(items, null);
		}

		private async Task TestReindex(TestOrderedList items)
		{
			var secondItem = FindItemByValue(items, "2");

			int[] indexes = new int[1000];

			indexes[0] = 1;
			indexes[996] = 2;
			indexes[997] = 3;
			indexes[998] = 4;
			indexes[999] = 5;

			//If we insert a lot of items between the first and the second we are going to initiate reindexing
			for (int i = 6; i <= 1000; i++)
			{
				var item = new Item
				{
					Id = Guid.NewGuid().ToString(),
					Value = i.ToString(),
					NextId = secondItem.Id
				};

				indexes[i - 5] = i;

				await items.InsertAsync(item);
			}

			await AssertOrderAsync(items, indexes);
		}

		[TestMethod]
		public async Task InsertItems()
		{
			//We are starting with 5 items with values 1,2,3,4,5
			var items = new TestOrderedList();
			await Helper.FillOrderedListAsync(items);

			await AssertOrderAsync(items, new[] { 1, 2, 3, 4, 5 });

			//Test Insert item at the begining
			await InsertItemBefore(items, "6", "1");
			await AssertOrderAsync(items, new[] { 6, 1, 2, 3, 4, 5 });

			//Test Insert item in the middle
			await InsertItemBefore(items, "7", "3");
			await AssertOrderAsync(items, new[] { 6, 1, 2, 7, 3, 4, 5 });

			//Test Insert item at the end
			await InsertItemBefore(items, "8", "5");
			await AssertOrderAsync(items, new[] { 6, 1, 2, 7, 3, 4, 8, 5 });

			//Test Insert after the end(null nextId)
			await InsertItemBefore(items, "9", Database.LastId);
			await AssertOrderAsync(items, new[] { 6, 1, 2, 7, 3, 4, 8, 5, 9 });

			//Test Insert after delete the first item 
			await DeleteItemWithValueAsync(items, "6");
			await InsertItemBefore(items, "10", "1");
			await AssertOrderAsync(items, new[] { 10, 1, 2, 7, 3, 4, 8, 5, 9 });
		}

		[TestMethod]
		public async Task ReorderItems()
		{
			//We are starting with 5 items with values 1,2,3,4,5
			var items = new TestOrderedList();
			await Helper.FillOrderedListAsync(items);

			await AssertOrderAsync(items, new[] { 1, 2, 3, 4, 5 });

			//Test Move task at the begining
			await MoveItemBefore(items, "5", "1");
			await AssertOrderAsync(items, new[] { 5, 1, 2, 3, 4 });

			await MoveItemBefore(items, "1", "3");
			await AssertOrderAsync(items, new[] { 5, 2, 1, 3, 4 });

			await MoveItemBefore(items, "4", "1");
			await AssertOrderAsync(items, new[] { 5, 2, 4, 1, 3 });

			await MoveItemBefore(items, "3", "1");
			await AssertOrderAsync(items, new[] { 5, 2, 4, 3, 1 });
		}

		private async Task DeleteItemWithValueAsync(TestOrderedList items, string value)
		{
			var task = FindItemByValue(items, value);
			await items.DeleteAsync(task);
		}

		private static Item FindItemByValue(TestOrderedList items, string value)
		{
			return ((Database)items.Database).FindItemByValue(value);
		}

		private async Task InsertItemBefore(TestOrderedList items, string newItemValue, string nextItemValue)
		{
			var item = new Item
			{
				Id = Guid.NewGuid().ToString(),
				Value = newItemValue
			};
			if (nextItemValue == Database.LastId)
			{
				item.NextId = Database.LastId;
			}
			else
			{
				item.NextId = FindItemByValue(items, nextItemValue).Id;
			}
			
			await items.InsertAsync(item);
		}

		private async Task MoveItemBefore(TestOrderedList items, string itemValue, string nextItemValue)
		{
			var oldItem = FindItemByValue(items, itemValue);
			var item = new Item
			{
				Id = oldItem.Id,
				Value = oldItem.Value
			};

			if (nextItemValue != null)
			{
				item.NextId = FindItemByValue(items, nextItemValue).Id;
			}

			await items.UpdateAsync(item);
		}

		private async Task AssertOrderAsync(TestOrderedList orderedList, int[] order)
		{
			var items = (await orderedList.ReadAllAsync(null)).ToList();
			if (order != null)
			{
				Assert.AreEqual(order.Length, items.Count);
			}
			for (int i = 0; i < items.Count; i++)
			{
				var expectedNextTaskId = (i == (items.Count - 1)) ? Database.LastId : items[i + 1].Id;
				Assert.AreEqual(expectedNextTaskId, items[i].NextId);
				if (order != null)
				{
					Assert.AreEqual(order[i].ToString(), items[i].Value);
				}
			}
		}
	}
}
