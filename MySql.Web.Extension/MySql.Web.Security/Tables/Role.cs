/**********************************************************************************************************************/
/*	Domain		:	MySql.Web.Security.Role
/*	Creator		:	KIM-KIWON\xyz37(Kim Ki Won)
/*	Create		:	Thursday, April 11, 2013 10:36 AM
/*	Purpose		:	webpages_Roles Table Entity class
/*--------------------------------------------------------------------------------------------------------------------*/
/*	Modifier	:	
/*	Update		:	
/*	Changes		:	
/*--------------------------------------------------------------------------------------------------------------------*/
/*	Comment		:	
CREATE TABLE [dbo].[webpages_Roles] (
	[RoleId]   INT            IDENTITY (1, 1) NOT NULL,
	[RoleName] NVARCHAR (256) NOT NULL,
	PRIMARY KEY CLUSTERED ([RoleId] ASC),
	UNIQUE NONCLUSTERED ([RoleName] ASC)
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
	/// webpages_Roles Table Entity class
	/// </summary>
	[Table("webpages_Roles")]
	public class Role
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Role"/> class.
		/// </summary>
		public Role()
		{
		}

		/// <summary>
		/// Gets or sets the role id.
		/// </summary>
		/// <value>The role id.</value>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Display(Name = "Role_RoleId", ResourceType = typeof(MetadataResources))]
		public int RoleId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the name of the role.
		/// </summary>
		/// <value>The name of the role.</value>
		[Column(TypeName = "nvarchar"), StringLength(256)]
		[Display(Name = "Role_RoleName", ResourceType = typeof(MetadataResources))]
		public string RoleName
		{
			get;
			set;
		}
	}
}
