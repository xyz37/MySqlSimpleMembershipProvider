using System.Web;
using System.Web.Mvc;
using SimpleMembershipTest.Filters;

namespace SimpleMembershipTest
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
			filters.Add(new InitializeSimpleMembershipAttribute());
		}
	}
}