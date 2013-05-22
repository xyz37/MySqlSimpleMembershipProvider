/**********************************************************************************************************************/
/*	Domain		:	MySql.Data.MySqlClient.CreateMySqlDatabaseIfNotExists`1
/*	Creator		:	KIM-KIWON\xyz37(Kim Ki Won)
/*	Create		:	Wednesday, April 10, 2013 10:22 AM
/*	Purpose		:	An implementation of IDatabaseInitializer<TContext> that will recreate and optionally re-seed the database with data only if the database does not exist. 
 *					To seed the database, create a derived class and override the Seed method.
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
using System.IO;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.Entity;
using System.Configuration;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// An implementation of IDatabaseInitializer&lt;TContext&gt; that will recreate and optionally re-seed the database with data only if the database does not exist. 
	/// To seed the database, create a derived class and override the <seealso cref="MySql.Data.MySqlClient.MySqlDatabaseInitializer&lt;TContext&gt;.Seed"/> method.
	/// </summary>
	/// <typeparam name="TContext">The type of the T context.</typeparam>
	public class CreateMySqlDatabaseIfNotExists<TContext> : MySqlDatabaseInitializer<TContext>
		where TContext : DbContext
	{
		/// <summary>
		/// Initializes the database.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <exception cref="System.InvalidOperationException">The model has changed.</exception>
		public override void InitializeDatabase(TContext context)
		{
			if (context.Database.Exists() == true)
			{
				if (context.Database.CompatibleWithModel(false) == false)
					throw new InvalidOperationException("The model has changed!");
			}
			else
				CreateMySqlDatabase(context);
		}
	}
}
