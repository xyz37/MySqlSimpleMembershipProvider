/**********************************************************************************************************************/
/*	Domain		:	MySql.Data.MySqlClient.MySqlConnectionFactory
/*	Creator		:	KIM-KIWON\xyz37(Kim Ki Won)
/*	Create		:	Friday, December 21, 2012 2:43 PM
/*	Purpose		:	Instances of this class are used to create DbConnection objects for SQL Server based on a given database name or connection string
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
using System.Linq;
using System.Text;
using System.Data.Entity.Infrastructure;
using System.Data.Common;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Instances of this class are used to create DbConnection objects for SQL Server based on a given database name or connection string
	/// </summary>
	public sealed class MySqlConnectionFactory : IDbConnectionFactory
	{
		private readonly string _providerInvariantName;
		private readonly string _baseConnectionString;

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlConnectionFactory"/> class.
		/// </summary>
		/// <param name="baseConnectionString">The base connection string.</param>
		/// <param name="providerInvariantName">Name of the provider invariant.</param>
		/// <exception cref="System.ArgumentNullException">
		/// providerInvariantName
		/// or
		/// baseConnectionString
		/// </exception>
		public MySqlConnectionFactory(string baseConnectionString, string providerInvariantName = "MySql.Data.MySqlClient")
		{
			if (providerInvariantName == null)
				throw new ArgumentNullException("providerInvariantName");

			if (baseConnectionString == null)
				throw new ArgumentNullException("baseConnectionString");

			_providerInvariantName = providerInvariantName;
			_baseConnectionString = baseConnectionString;
		}

		/// <summary>
		/// Creates a connection based on the given database name or connection string.
		/// </summary>
		/// <param name="nameOrConnectionString">The database name or connection string.</param>
		/// <returns>An initialized DbConnection.</returns>
		/// <exception cref="System.ArgumentNullException">nameOrConnectionString</exception>
		/// <exception cref="System.InvalidOperationException">ProviderInvariantName is invalid.</exception>
		public DbConnection CreateConnection(string nameOrConnectionString)
		{
			if (nameOrConnectionString == null)
				throw new ArgumentNullException("nameOrConnectionString");

			if (nameOrConnectionString.Contains("="))
				return new MySqlConnection(nameOrConnectionString);

			DbConnection connection = DbProviderFactories.GetFactory(ProviderInvariantName).CreateConnection();

			if (connection == null)
				throw new InvalidOperationException("ProviderInvariantName is invalid.");

			var databaseName = nameOrConnectionString.Replace(".", "_").Replace("+", "_");
			var regEx = new System.Text.RegularExpressions.Regex(@"database=(?<db>[\w_-]*);");

			if (regEx.IsMatch(nameOrConnectionString) == true)
				databaseName = regEx.Match(nameOrConnectionString).Groups["db"].Value;

			MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(BaseConnectionString)
			{
				Database = databaseName,
			};

			connection.ConnectionString = builder.ConnectionString;

			return connection;
		}

		/// <summary>
		/// Gets the name of the provider invariant.
		/// </summary>
		/// <value>The name of the provider invariant.</value>
		public string ProviderInvariantName
		{
			get
			{
				return _providerInvariantName;
			}
		}

		/// <summary>
		/// Gets the base connection string.
		/// </summary>
		/// <value>The base connection string.</value>
		public string BaseConnectionString
		{
			get
			{
				return _baseConnectionString;
			}
		}
	}
}
