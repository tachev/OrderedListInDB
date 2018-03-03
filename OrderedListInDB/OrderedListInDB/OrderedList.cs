using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geo.Data
{
    public class OrderedList<TItem, TId, TQuery> where TItem : IIndexedItem<TId> where TId : class
	{
		// Fine tuned based on the current density of the items. If we see a lot of reindexing happening we can change that
		protected decimal IndexGapSize = 64;
		protected decimal IndexInitiateReindex = 0.0000001M;
		protected decimal IndexReindexStep = 0.00001M;
		protected decimal IndexGapDivider = 2;

		public IDatabase<TItem, TId, TQuery> Database { get; }

		public OrderedList(IDatabase<TItem, TId, TQuery> database)
		{
			Database = database;
		}

		public async Task InsertAsync(TItem item)
		{
			if (item.NextId == Database.GetLastId())
			{
				await InsertAtTheEndAsync(item);
			}
			else
			{
				await InsertItemAsync(item);
			}

			await Database.CreateAsync(item);
		}

		public async Task UpdateAsync(TItem item)
		{
			var oldItem = await Database.ReadByIdAsync(item.Id);

			if (oldItem.NextId != item.NextId)
			{
				await DetachItemAsync(oldItem);
				if (item.NextId == Database.GetLastId())
				{
					await InsertAtTheEndAsync(item);
				}
				else
				{
					await InsertItemAsync(item);
				}
			}

			await Database.UpdateAsync(item);
		}

		public async Task DeleteAsync(TItem item)
		{
			await UpdateTheLinkForThePreviousItem(item);

			await Database.DeleteAsync(item.Id);
		}

		public async Task<IEnumerable<TItem>> ReadAllAsync(TQuery query)
		{
			var items = await Database.Read(query);

			return OrderItems(items.ToList());
		}

		private async Task UpdateTheLinkForThePreviousItem(TItem item)
		{
			//TODO: [Improvement] Can be done with one request
			var previousItem = await GetPreviousItemAsync(item.Id);
			if (previousItem != null)
			{
				previousItem.NextId = item.NextId;
				await Database.UpdateAsync(previousItem);
			}
		}

		private async Task<TItem> GetPreviousItemAsync(TId nextId)
		{
			return await Database.ReadByNextIdAsync(nextId);
		}

		private async Task<TItem> GetNextItemAsync(TItem item)
		{
			return await Database.ReadByIdAsync(item.NextId);
		}

		private async Task<TItem> GetLastItemAsync(TItem item)
		{
			return await Database.ReadByNextIdAsync(Database.GetLastId());
		}

		private List<TItem> OrderItems(List<TItem> unorderedList)
		{
			var orderedList = unorderedList.OrderBy(o => o.Index).ToList();
			return orderedList;
		}

		private async Task DetachItemAsync(TItem item)
		{
			var previousItem = await GetPreviousItemAsync(item.Id);

			if (previousItem != null)
			{
				if (item.NextId != Database.GetLastId())
				{
					var nextItem = await GetNextItemAsync(item);
					previousItem.NextId = nextItem.Id;
				}
				else
				{
					previousItem.NextId = Database.GetLastId();
				}

				await Database.UpdateAsync(previousItem);
			}
		}

		private async Task InsertItemAsync(TItem item)
		{
			var previousItem = await GetPreviousItemAsync(item.NextId);
			var nextItem = await GetNextItemAsync(item);

			if (previousItem == null)
			{
				InsertAtTheBeggining(item, nextItem);
			}
			else
			{
				await InsertInTheMiddle(item, previousItem, nextItem);
			}
		}

		private async Task InsertInTheMiddle(TItem item, TItem previousItem, TItem nextItem)
		{
			decimal increase = (nextItem.Index - previousItem.Index) / IndexGapDivider;

			if (increase < IndexInitiateReindex)
			{
				await ReindexItemsAsync(previousItem);
				increase = (nextItem.Index - previousItem.Index) / IndexGapDivider;
				item.Index = previousItem.Index + increase;
			}
			else
			{
				item.Index = previousItem.Index + increase;
			}
			previousItem.NextId = item.Id;
			await Database.UpdateAsync(previousItem);
		}		

		protected virtual async Task ReindexItemsAsync(TItem previousItem)
		{
			var item = previousItem;
			var index = item.Index;

			var asyncTasks = new List<Task>();
			while (item != null && item.Index >= index - IndexReindexStep)
			{
				index -= IndexReindexStep;
				item.Index = index;
				asyncTasks.Add(Database.UpdateAsync(item));
				item = await GetPreviousItemAsync(item.Id);
			}
			Task.WaitAll(asyncTasks.ToArray());
		}

		private void InsertAtTheBeggining(TItem item, TItem nextItem)
		{
			if (nextItem == null)
			{
				item.Index = 0;
			}
			else
			{
				item.Index = nextItem.Index - IndexGapSize;
			}
		}

		private async Task InsertAtTheEndAsync(TItem item)
		{
			var lastItem = await GetLastItemAsync(item);
			if (lastItem != null)
			{
				lastItem.NextId = item.Id;
				item.Index = lastItem.Index + IndexGapSize;
				await Database.UpdateAsync(lastItem);
			}
		}
	}
}
