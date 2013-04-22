/**********************************************************************************************************************/
/*	Domain		:	MySql.Data.MySqlClient.MySqlDatabaseInitializer`1
/*	Creator		:	KIM-KIWON\xyz37(Kim Ki Won)
/*	Create		:	Thursday, April 04, 2013 11:23 AM
/*	Purpose		:	
/*--------------------------------------------------------------------------------------------------------------------*/
/*	Modifier	:	
/*	Update		:	
/*	Changes		:	
/*--------------------------------------------------------------------------------------------------------------------*/
/*	Comment		:	http://www.nsilverbullet.net/2012/11/07/6-steps-to-get-entity-framework-5-working-with-mysql-5-5/
 *					http://brice-lambson.blogspot.kr/2012/05/using-entity-framework-code-first-with.html
 *					https://gist.github.com/3061139
 *					http://blog.oneunicorn.com/2012/02/27/code-first-migrations-making-__migrationhistory-not-a-system-table/
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
using System.Text.RegularExpressions;
using System.Transactions;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Defines a method for the database initializer.
	/// </summary>
	/// <typeparam name="TContext">The type of the T context.
	/// This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived. 
	/// For more information about covariance and contravariance, see http://msdn.microsoft.com/en-us/library/dd799517(v=vs.103).aspx.
	/// </typeparam>
	public abstract class MySqlDatabaseInitializer<TContext> : IDatabaseInitializer<TContext>
		where TContext : DbContext
	{
		/// <summary>
		/// Executes the strategy to initialize the database for the given context.
		/// </summary>
		/// <param name="context">The context.</param>
		public abstract void InitializeDatabase(TContext context);

		/// <summary>
		/// Create MySql database.
		/// </summary>
		/// <param name="context">The context.</param>
		protected void CreateMySqlDatabase(TContext context)
		{
			try
			{
				// Create as much of the database as we can             
				context.Database.Create();
				// No exception? Don't need a workaround
				Seed(context);
				context.SaveChanges();

				return;
			}
			catch (MySqlException ex)
			{
				/*
				if (ex.Number == 1044)
				{
					// Could not create database under message
					// Access denied for user 'User'@'%' to database 'database'
					using (var connection = ((MySqlConnection)context.Database.Connection).Clone())
					{
						using (var command = connection.CreateCommand())
						{
							string userId = string.Empty;
							string password = string.Empty;

							foreach (string item in connection.ConnectionString.SplitDelimiter(";"))
							{
								var parse = item.SplitDelimiter("=").ToArray();

								if (parse.Count() == 2)
								{
									if (String.Compare(parse[0], "User Id", true) == 0)
										userId = parse[1];

									if (String.Compare(parse[0], "password", true) == 0)
										password = parse[1];
								}
								if (userId != string.Empty && password != string.Empty)
									break;
							}
							
							//command.CommandText = string.Format("grant all privileges on {0}.* to '{1}'@'%' identified by '{2}' with grant option;",
							//	connection.Database, userId, password);
							command.CommandText = string.Format("create database {0} character set utf8;", connection.Database);

							try
							{
								connection.Open();
								command.ExecuteNonQuery();
							}
							catch (Exception exDb)
							{
								throw exDb;
							}
							finally
							{
								connection.Close();
							}
						}
					}
				}
				*/

				if (ex.Number != 1064)		// Ignore the parse exception
					throw;
			}

			// Manually create the metadata table         
			using (var connection = ((MySqlConnection)context.Database.Connection).Clone())
			{
				using (var command = connection.CreateCommand())
				{
					command.CommandText =
		@"
CREATE TABLE __MigrationHistory (
	MigrationId mediumtext,
	CreatedOn datetime,
	Model mediumblob,
	ProductVersion mediumtext);

ALTER TABLE __MigrationHistory
ADD PRIMARY KEY (MigrationId(255));

INSERT INTO __MigrationHistory (
	MigrationId,
	CreatedOn,
	Model,
	ProductVersion)
VALUES (
	'InitialCreate',
	@CreatedOn,
	@Model,
	@ProductVersion);
";
					command.Parameters.AddWithValue("@Model", GetModel(context));
					command.Parameters.AddWithValue("@ProductVersion", GetProductVersion());
					command.Parameters.AddWithValue("@CreatedOn", DateTime.Now);
					connection.Open();
					command.ExecuteNonQuery();
					connection.Close();
				}
			}

			Seed(context);

			using (TransactionScope scope = new TransactionScope())
			{
				context.SaveChanges();
				scope.Complete();
			}
		}

		private byte[] GetModel(TContext context)
		{
			using (var memoryStream = new MemoryStream())
			{
				using (var gzipStream = new System.IO.Compression.GZipStream(memoryStream, System.IO.Compression.CompressionMode.Compress))
				using (var xmlWriter = System.Xml.XmlWriter.Create(gzipStream, new System.Xml.XmlWriterSettings
				{
					Indent = true
				}))
				{
					EdmxWriter.WriteEdmx(context, xmlWriter);
				}
				return memoryStream.ToArray();
			}
		}

		private string GetProductVersion()
		{
			return typeof(DbContext).Assembly
				.GetCustomAttributes(false)
				.OfType<System.Reflection.AssemblyInformationalVersionAttribute>()
				.Single()
				.InformationalVersion;
		}

		/// <summary>
		/// When overridden adds data to the context for seeding. The default implementation does nothing.
		/// </summary>
		/// <param name="context">The context to seed.</param>
		protected virtual void Seed(TContext context)
		{
		}
	}
}

