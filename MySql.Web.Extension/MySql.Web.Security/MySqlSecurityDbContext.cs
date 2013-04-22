/**********************************************************************************************************************/
/*	Domain		:	MySql.Web.Security.MySqlSecurityDbContext
/*	Creator		:	KIM-KIWON\xyz37(Kim Ki Won)
/*	Create		:	Wednesday, April 10, 2013 10:28 AM
/*	Purpose		:	Provide Database context with MySql database for security.
/*--------------------------------------------------------------------------------------------------------------------*/
/*	Modifier	:	
/*	Update		:	
/*	Changes		:	
/*--------------------------------------------------------------------------------------------------------------------*/
/*	Comment		:	
/*--------------------------------------------------------------------------------------------------------------------*/
/*	Reviewer	:	Kim Ki Won
/*	Rev. Date	:	
/**********************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace MySql.Web.Security
{
	/// <summary>
	/// Provide Database context with MySql database for security.
	/// </summary>
	public class MySqlSecurityDbContext : MySqlDbContext
	{
		/// <summary>
		/// Constructs a new context instance using the given string as the name or connection string for the
		/// database to which a connection will be made.
		/// See the class remarks for how this is used to create a connection.
		/// </summary>
		/// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
		public MySqlSecurityDbContext(string nameOrConnectionString)
			: base(nameOrConnectionString)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlSecurityDbContext" /> class with database connection information.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="userId">The user id.</param>
		/// <param name="password">The password.</param>
		/// <param name="server">The server.</param>
		/// <param name="port">The port.</param>
		public MySqlSecurityDbContext(
			string database,
			string userId,
			string password,
			string server = "localhost",
			uint port = 3306)
			: base(database, userId, password, server, port)
		{
		}

		/// <summary>
		/// Get or set UserProfiles
		/// </summary>
		/// <value>The user profiles.</value>
		public DbSet<UserProfile> UserProfiles
		{
			get;
			set;
		}

		/// <summary>
		/// Get or set Memberships
		/// </summary>
		/// <value>The memberships.</value>
		public DbSet<Membership> Memberships
		{
			get;
			set;
		}

		/// <summary>
		/// Get or set OAuthMemberships
		/// </summary>
		/// <value>The OAuthMemberships.</value>
		public DbSet<OAuthMembership> OAuthMemberships
		{
			get;
			set;
		}

		/// <summary>
		/// Get or set OAuthTokens
		/// </summary>
		/// <value>The OAuthTokens.</value>
		public DbSet<OAuthToken> OAuthTokens
		{
			get;
			set;
		}

		/// <summary>
		/// Get or set Roless
		/// </summary>
		/// <value>The roles.</value>
		public DbSet<Role> Roles
		{
			get;
			set;
		}

		/// <summary>
		/// Get or set UsersInRoles
		/// </summary>
		/// <value>The users in roles.</value>
		public DbSet<UsersInRoles> UsersInRoles
		{
			get;
			set;
		}
	}
}
