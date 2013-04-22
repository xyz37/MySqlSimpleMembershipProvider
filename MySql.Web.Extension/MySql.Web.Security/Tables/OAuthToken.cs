/**********************************************************************************************************************/
/*	Domain		:	MySql.Web.Security.OAuthToken
/*	Creator		:	KIM-KIWON\xyz37(Kim Ki Won)
/*	Create		:	Friday, April 12, 2013 12:11:20 AM
/*	Purpose		:	webpages_OAuthToken Table Entity class
/*--------------------------------------------------------------------------------------------------------------------*/
/*	Modifier	:	
/*	Update		:	
/*	Changes		:	
/*--------------------------------------------------------------------------------------------------------------------*/
/*	Comment		:	
CREATE TABLE [dbo].[webpages_OAuthToken] (
	[Token]		nvarchar(100) NOT NULL, 
	[Secret]	nvarchar(100) NOT NULL, 
	PRIMARY KEY (Token)
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
	/// webpages_OAuthToken Table Entity class
	/// </summary>
	[Table("webpages_OAuthToken")]
	public class OAuthToken
	{
		/// <summary>
		/// Gets or sets the token.
		/// </summary>
		/// <value>The token.</value>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Column(TypeName = "nvarchar"), StringLength(100)]
		[Display(Name = "OAuthToken_Token", ResourceType = typeof(MetadataResources))]
		public string Token
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the secret.
		/// </summary>
		/// <value>The secret.</value>
		[Column(TypeName = "nvarchar"), StringLength(100)]
		[Display(Name = "OAuthToken_Secret", ResourceType = typeof(MetadataResources))]
		public string Secret
		{
			get;
			set;
		}
	}
}
