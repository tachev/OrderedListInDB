using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Geo.Data
{
    public interface IDatabase<TItem, TId, TQuery> where TItem: IIndexedItem<TId>
    {
		Task CreateAsync(TItem indexedItem);

		//Id should be a primary key/clustered index or whatever the database is using to make the search by this field fast
		Task<TItem> ReadByIdAsync(TId id);

		//Implementing this should use an index by nextId
		Task<TItem> ReadByNextIdAsync(TId id);

		Task UpdateAsync(TItem indexedItem);

		Task DeleteAsync(TId id);

		TId GetLastId();

		Task<IEnumerable<TItem>> Read(TQuery query);
	}
}
