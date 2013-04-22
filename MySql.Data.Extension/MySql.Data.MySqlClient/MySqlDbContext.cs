/**********************************************************************************************************************/
/*	Domain		:	MySql.Data.MySqlClient.MySqlDbContext
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

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Provide Database context with MySql database for security.
	/// </summary>
	public class MySqlDbContext : DbContext
	{
		/// <summary>
		/// Constructs a new context instance using the given string as the name or connection string for the
		/// database to which a connection will be made.
		/// See the class remarks for how this is used to create a connection.
		/// </summary>
		/// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
		protected MySqlDbContext(string nameOrConnectionString)
			: base(nameOrConnectionString)
		{
			ConnectionString = nameOrConnectionString;
			Database.DefaultConnectionFactory = new MySqlConnectionFactory(ConnectionString);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlDbContext"/> class with database connection information.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="userId">The user id.</param>
		/// <param name="password">The password.</param>
		/// <param name="server">The server.</param>
		/// <param name="port">The port.</param>
		protected MySqlDbContext(
			string database,
			string userId,
			string password,
			string server = "localhost",
			uint port = 3306)
			: this(GetMySqlConnectionString(database, userId, password, server, port))
		{
		}

		/// <summary>
		/// Gets MySql connection string.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="userId">The user id.</param>
		/// <param name="password">The password.</param>
		/// <param name="server">The server.</param>
		/// <param name="port">The port.</param>
		/// <returns>Generated MySql connection string.</returns>
		public static string GetMySqlConnectionString(
			string database,
			string userId,
			string password,
			string server = "localhost",
			uint port = 3306)
		{
			var connectionStringBuilder = new MySqlConnectionStringBuilder
			{
				Server = server,
				Port = port,
				Database = database,
				UserID = userId,
				Password = password,
				PersistSecurityInfo = true,
			};

			return connectionStringBuilder.ConnectionString;
		}

		/// <summary>
		/// Gets or sets the connection string.
		/// </summary>
		/// <value>The connection string.</value>
		public string ConnectionString
		{
			get;
			protected set;
		}

		/// <summary>
		/// Creates the <see cref="MySqlDbContext"/> context.
		/// </summary>
		/// <param name="nameOrConnectionString">The name or connection string.</param>
		/// <returns>MySqlDbContext.</returns>
		public static MySqlDbContext CreateContext(string nameOrConnectionString)
		{
			return new MySqlDbContext(nameOrConnectionString);
		}
	}
}
