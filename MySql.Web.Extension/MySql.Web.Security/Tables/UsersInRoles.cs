/**********************************************************************************************************************/
/*	Domain		:	MySql.Web.Security.UsersInRoles
/*	Creator		:	KIM-KIWON\xyz37(Kim Ki Won)
/*	Create		:	Thursday, April 11, 2013 10:36 AM
/*	Purpose		:	webpages_UsersInRoles Table Entity class
/*--------------------------------------------------------------------------------------------------------------------*/
/*	Modifier	:	
/*	Update		:	
/*	Changes		:	
/*--------------------------------------------------------------------------------------------------------------------*/
/*	Comment		:	
CREATE TABLE [dbo].[webpages_UsersInRoles] (
	[UserId] INT NOT NULL,
	[RoleId] INT NOT NULL,
	PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
	CONSTRAINT [fk_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[UserProfile] ([UserId]),
	CONSTRAINT [fk_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[webpages_Roles] ([RoleId])
);
/*--------------------------------------------------------------------------------------------------------------------*/
/*	Reviewer	:	Kim Ki Won
/*	Rev. Date	:	
/**********************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Web.Extension.Resources;

namespace MySql.Web.Security
{
	/// <summary>
	/// webpages_UsersInRoles Table Entity class
	/// </summary>
	[Table("webpages_UsersInRoles")]
	public class UsersInRoles
	{
		/// <summary>
		/// Gets or sets the user id.
		/// </summary>
		/// <value>The user id.</value>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Column(Order = 1)]
		[Display(Name = "UsersInRoles_UserId", ResourceType = typeof(MetadataResources))]
		public int UserId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the role id.
		/// </summary>
		/// <value>The role id.</value>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Column(Order = 2)]
		[Display(Name = "UsersInRoles_RoleId", ResourceType = typeof(MetadataResources))]
		public int RoleId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the user profile.
		/// </summary>
		/// <value>The user profile.</value>
		[ForeignKey("UserId")]
		public virtual UserProfile UserProfile
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the role.
		/// </summary>
		/// <value>The role.</value>
		[ForeignKey("RoleId")]
		public virtual Role Role
		{
			get;
			set;
		}
	}
}
