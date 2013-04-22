using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace MySql.Web.Security
{
	public class UserProfileExt : UserProfile
	{
		[DataType(DataType.EmailAddress)]
		public string Email
		{
			get;
			set;
		}

		public string Facebook
		{
			get;
			set;
		}

		public int Age
		{
			get;
			set;
		}

		public double? Rate
		{
			get;
			set;
		}
	}
}
