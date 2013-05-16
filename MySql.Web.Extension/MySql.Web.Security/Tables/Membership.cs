/**********************************************************************************************************************/
/*	Domain		:	MySql.Web.Security.Membership
/*	Creator		:	KIM-KIWON\xyz37(Kim Ki Won)
/*	Create		:	Thursday, April 11, 2013 10:36 AM
/*	Purpose		:	webpages_Membership Table Entity class
/*--------------------------------------------------------------------------------------------------------------------*/
/*	Modifier	:	
/*	Update		:	
/*	Changes		:	
/*--------------------------------------------------------------------------------------------------------------------*/
/*	Comment		:	
CREATE TABLE [dbo.[webpages_Membership (
	[UserId                                  INT            NOT NULL,
	[CreateDate                              DATETIME       NULL,
	[ConfirmationToken                       NVARCHAR (128) NULL,
	[IsConfirmed                             BIT            DEFAULT ((0)) NULL,
	[LastPasswordFailureDate                 DATETIME       NULL,
	[PasswordFailuresSinceLastSuccess        INT            DEFAULT ((0)) NOT NULL,
	[Password                                NVARCHAR (128) NOT NULL,
	[PasswordChangedDate                     DATETIME       NULL,
	[PasswordSalt                            NVARCHAR (128) NOT NULL,
	[PasswordVerificationToken               NVARCHAR (128) NULL,
	[PasswordVerificationTokenExpirationDate DATETIME       NULL,
	PRIMARY KEY CLUSTERED ([UserId ASC)
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
	///  Table Entity class
	/// </summary>
	[Table("webpages_Membership")]
	public class Membership
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Membership"/> class.
		/// </summary>
		public Membership()
		{
		}

		/// <summary>
		/// Gets or sets the user id.
		/// </summary>
		/// <value>The user id.</value>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Display(Name = "Membership_UserId", ResourceType = typeof(MetadataResources))]
		public int UserId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the create date.
		/// </summary>
		/// <value>The create date.</value>
		[DataType(DataType.DateTime)]
		[Display(Name = "Membership_CreateDate", ResourceType = typeof(MetadataResources))]
		public DateTime? CreateDate
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the confirmation token.
		/// </summary>
		/// <value>The confirmation token.</value>
		[Column(TypeName = "nvarchar"), StringLength(128)]
		[Display(Name = "Membership_ConfirmationToken", ResourceType = typeof(MetadataResources))]
		public string ConfirmationToken
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is confirmed.
		/// </summary>
		/// <value><c>null</c> if [is confirmed] contains no value, <c>true</c> if [is confirmed]; otherwise, <c>false</c>.</value>
		[DefaultValue(false)]
		[Display(Name = "Membership_IsConfirmed", ResourceType = typeof(MetadataResources))]
		public bool? IsConfirmed
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the last password failure date.
		/// </summary>
		/// <value>The last password failure date.</value>
		[DataType(DataType.DateTime)]
		[Display(Name = "Membership_LastPasswordFailureDate", ResourceType = typeof(MetadataResources))]
		public DateTime? LastPasswordFailureDate
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the password failures since last success.
		/// </summary>
		/// <value>The password failures since last success.</value>
		[DefaultValue(0)]
		[Display(Name = "Membership_PasswordFailuresSinceLastSuccess", ResourceType = typeof(MetadataResources))]
		public int PasswordFailuresSinceLastSuccess
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		/// <value>The password.</value>
		[Column(TypeName = "nvarchar"), StringLength(128)]
		[Display(Name = "Membership_Password", ResourceType = typeof(MetadataResources))]
		public string Password
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the password changed date.
		/// </summary>
		/// <value>The password changed date.</value>
		[DataType(DataType.DateTime)]
		[Display(Name = "Membership_PasswordChangedDate", ResourceType = typeof(MetadataResources))]
		public DateTime? PasswordChangedDate
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the password salt.
		/// </summary>
		/// <value>The password salt.</value>
		[Column(TypeName = "nvarchar"), StringLength(128)]
		[Display(Name = "Membership_PasswordSalt", ResourceType = typeof(MetadataResources))]
		public string PasswordSalt
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the password verification token.
		/// </summary>
		/// <value>The password verification token.</value>
		[Column(TypeName = "nvarchar"), StringLength(128)]
		[Display(Name = "Membership_PasswordVerificationToken", ResourceType = typeof(MetadataResources))]
		public string PasswordVerificationToken
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the password verification token expiration date.
		/// </summary>
		/// <value>The password verification token expiration date.</value>
		[DataType(DataType.DateTime)]
		[Display(Name = "Membership_PasswordVerificationTokenExpirationDate", ResourceType = typeof(MetadataResources))]
		public DateTime? PasswordVerificationTokenExpirationDate
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
	}
}
