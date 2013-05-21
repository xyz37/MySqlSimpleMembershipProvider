/**********************************************************************************************************************/
/*	Domain		:	MySql.Web.Security.UserProfile
/*	Creator		:	KIM-KIWON\xyz37(Kim Ki Won)
/*	Create		:	Thursday, April 11, 2013 10:44 AM
/*	Purpose		:	UserProfile Table Entity class
/*--------------------------------------------------------------------------------------------------------------------*/
/*	Modifier	:	
/*	Update		:	
/*	Changes		:	
/*--------------------------------------------------------------------------------------------------------------------*/
/*	Comment		:	
CREATE TABLE [dbo].[UserProfile] (
	[UserId]   INT            IDENTITY (1, 1) NOT NULL,
	[UserName] NVARCHAR (MAX) NULL,
	PRIMARY KEY CLUSTERED ([UserId] ASC)
);
/*--------------------------------------------------------------------------------------------------------------------*/
/*	Reviewer	:	Kim Ki Won
/*	Rev. Date	:	
/**********************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MySql.Web.Extension.Resources;

namespace MySql.Web.Security
{
	/// <summary>
	/// UserProfile Table Entity class
	/// </summary>
	[Table("UserProfile")]
	public partial class UserProfile
	{
		/// <summary>
		/// Gets or sets the user id.
		/// </summary>
		/// <value>The user id.</value>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Display(Name = "UserProfile_UserId", ResourceType = typeof(MetadataResources))]
		public int UserId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the name of the user.
		/// </summary>
		/// <value>The name of the user.</value>
		[Display(Name = "UserProfile_UserName", ResourceType = typeof(MetadataResources))]
		[Required]
		public string UserName
		{
			get;
			set;
		}
	}
}
