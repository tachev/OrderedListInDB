using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geo.Data.Tests
{
	public class TestOrderedListWorstCase : TestOrderedList
	{
		public TestOrderedListWorstCase(): base(new Database())
		{
			IndexGapDivider = 64;
		}
	}
}
