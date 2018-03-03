using Geo.Data.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geo.TestConsoleApp
{
	public class Program
	{
		static void Main(string[] args)
		{
			Helper.CreateItems(30000).Wait();
		}
	}
}
