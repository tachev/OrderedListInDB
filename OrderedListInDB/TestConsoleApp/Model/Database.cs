using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Geo.Data.Tests
{
	public class Database : IDatabase<Item, string, string>
	{
		public const string LastId = "LAST";
		public int SimulateDatabaseDelay = 0;

		ConcurrentDictionary<string, Item> store = new ConcurrentDictionary<string, Item>();

		ConcurrentDictionary<string, Item> valuesIndex = new ConcurrentDictionary<string, Item>();
		ConcurrentDictionary<string, Item> nextIdIndex = new ConcurrentDictionary<string, Item>();

		public string GetLastId()
		{
			return LastId;
		}

		public Item FindItemByValue(string value)
		{
			Delay();
			return valuesIndex[value];
		}

		public Task CreateAsync(Item indexedItem)
		{
			return Task.Run(() =>
			{
				store.AddOrUpdate(indexedItem.Id, indexedItem, (key, value) => value = indexedItem);
				nextIdIndex.AddOrUpdate(indexedItem.NextId, indexedItem, (key, value) => value = indexedItem);
				valuesIndex.AddOrUpdate(indexedItem.Value, indexedItem, (key, value) => value = indexedItem);
				Delay();
			});
		}

		public Task<Item> ReadByIdAsync(string id)
		{
			return Task.Run(() =>
			{
				Delay();
				return store[id];
			});
		}

		private void Delay()
		{
			if (SimulateDatabaseDelay > 0)
			{
				Thread.Sleep(SimulateDatabaseDelay);
			}
		}

		public Task<Item> ReadByNextIdAsync(string nextId)
		{
			return Task.Run(() =>
			{
				Delay();
				nextIdIndex.TryGetValue(nextId, out Item item);
				return item;
			});
		}

		public Task UpdateAsync(Item indexedItem)
		{
			return Task.Run(() =>
			{
				store.AddOrUpdate(indexedItem.Id, indexedItem, (key, value) => value = indexedItem);
				nextIdIndex.AddOrUpdate(indexedItem.NextId, indexedItem, (key, value) => value = indexedItem);
				valuesIndex.AddOrUpdate(indexedItem.Value, indexedItem, (key, value) => value = indexedItem);
				Delay();
			});
		}

		public Task DeleteAsync(string id)
		{
			return Task.Run(() =>
			{
				store.TryRemove(id, out Item item);
				nextIdIndex.TryRemove(item.NextId, out item);
				valuesIndex.TryRemove(item.Value, out item);

				Delay();
			});
		}

		public Task<IEnumerable<Item>> Read(string query)
		{
			return Task.Run(() =>
			{
				if (SimulateDatabaseDelay > 0)
				{
					Thread.Sleep(SimulateDatabaseDelay * store.Count);
				}
				return store.Values.AsEnumerable();
			});
		}

		public int CountAll()
		{
			return store.Count;
		}
	}
}
