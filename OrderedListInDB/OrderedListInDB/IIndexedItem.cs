using System;

namespace Geo.Data
{
    public interface IIndexedItem<TId>
    {
		TId Id { get; set; } 

		decimal Index { get; set; }

		TId NextId { get; set; }
    }
}
