using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace MySql.Web.Security
{
	public class UserProfileExt : UserProfile
	{
		/// <summary>
		/// Age를 구하거나 설정합니다.
		/// </summary>
		public int Age
		{
			get;
			set;
		}

		/// <summary>
		/// HP를 구하거나 설정합니다.
		/// </summary>
		public string HP
		{
			get;
			set;
		}

		/// <summary>
		/// Addr를 구하거나 설정합니다.
		/// </summary>
		public string Addr
		{
			get;
			set;
		}
		
	}
}
