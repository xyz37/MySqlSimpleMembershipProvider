using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySql.Web.Security
{
	internal static class StringExtensions
	{
		public static bool IsEmpty(this string value)
		{
			return string.IsNullOrEmpty(value);
		}
	}
}
