using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Web.Security;

namespace SimpleMembershipTest.Dac
{
	class Program
	{
		private static void Main()
		{
			var db = SimpleMembershipTestDbContext.CreateContext();
			var count = db.UserProfiles.Count();

			Console.WriteLine("user count: {0}", count);

			MySqlWebSecurity.InitializeDatabaseConnection("SimpleMembershipTestDbContext");
			int userId = MySqlWebSecurity.GetUserId("test");

			Console.WriteLine("user ID: {0}", userId);			
		}
	}
}
