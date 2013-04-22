// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

/**********************************************************************************************************************/
/*	Domain		:	MySql.Web.Security.MySqlSimpleRoleProvider
/*	Creator		:	KIM-KIWON\xyz37(Kim Ki Won)
/*	Create		:	Friday, April 12, 2013 10:56 AM
/*	Purpose		:	Defines the contract that ASP.NET implements to provide role-management services using custom role providers for MySql database.
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
using System.Linq;
using System.Web.Security;
using System.Globalization;
using System.Collections.Generic;
using System.Configuration.Provider;
using WebMatrix.WebData.Resources;
using MySql.Web.Extension.Resources;

namespace MySql.Web.Security
{
	/// <summary>
	/// Defines the contract that ASP.NET implements to provide role-management services using custom role providers for MySql database.
	/// </summary>
	public class MySqlSimpleRoleProvider : RoleProvider
	{
		private RoleProvider _previousProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlSimpleRoleProvider"/> class.
		/// </summary>
		public MySqlSimpleRoleProvider()
			: this(null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlSimpleRoleProvider"/> class.
		/// </summary>
		/// <param name="previousProvider">The previous provider.</param>
		public MySqlSimpleRoleProvider(RoleProvider previousProvider)
		{
			_previousProvider = previousProvider;
		}

		private RoleProvider PreviousProvider
		{
			get
			{
				if (_previousProvider == null)
				{
					throw new InvalidOperationException(Resources.Security_InitializeMustBeCalledFirst);
				}
				else
				{
					return _previousProvider;
				}
			}
		}

		private MySqlSecurityDbContext NewMySqlSecurityDbContext
		{
			get
			{
				//if (string.IsNullOrEmpty(ConfigUtil.MySqlSecurityInheritedContextType) == true)
				//{
				//	string nameOrConnectionString = ConnectionInfo.ConnectionString;

				//	if (nameOrConnectionString.IsEmpty() == true)
				//		nameOrConnectionString = ConnectionInfo.ConnectionStringName;

				//	return new MySqlSecurityDbContext(nameOrConnectionString);
				//}
				//else
				{
					// NOTICE: In the above manner, the DatabaseInitializer interface method call had occurred.
					// by X10-MOBILE\xyz37(Kim Ki Won) in Friday, April 19, 2013 12:31 AM
					var contextInstance = Activator.CreateInstance(Type.GetType(ConfigUtil.MySqlSecurityInheritedContextType, false, true));

					return contextInstance as MySqlSecurityDbContext;
				}
			}
		}

		internal DatabaseConnectionInfo ConnectionInfo
		{
			get;
			set;
		}

		internal bool InitializeCalled
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the name of the application to store and retrieve role information for.
		/// </summary>
		/// <remarks>Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <value>The name of the application.</value>
		/// <exception cref="System.NotSupportedException">
		/// </exception>
		/// <returns>The name of the application to store and retrieve role information for.</returns>
		public override string ApplicationName
		{
			get
			{
				if (InitializeCalled)
				{
					throw new NotSupportedException();
				}
				else
				{
					return PreviousProvider.ApplicationName;
				}
			}
			set
			{
				if (InitializeCalled)
				{
					throw new NotSupportedException();
				}
				else
				{
					PreviousProvider.ApplicationName = value;
				}
			}
		}

		private void VerifyInitialized()
		{
			if (!InitializeCalled)
			{
				throw new InvalidOperationException(Resources.Security_InitializeMustBeCalledFirst);
			}
		}

		private List<int> GetUserIdsFromNames(MySqlSecurityDbContext db, string[] usernames)
		{
			List<int> userIds = new List<int>(usernames.Length);
			foreach (string username in usernames)
			{
				int id = MySqlSimpleMembershipProvider.GetUserId(db, username);
				if (id == -1)
				{
					throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Security_NoUserFound, username));
				}
				userIds.Add(id);
			}
			return userIds;
		}

		private static List<int> GetRoleIdsFromNames(MySqlSecurityDbContext db, string[] roleNames)
		{
			List<int> roleIds = new List<int>(roleNames.Length);
			foreach (string role in roleNames)
			{
				int id = FindRoleId(db, role);
				if (id == -1)
				{
					throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.SimpleRoleProvider_NoRoleFound, role));
				}
				roleIds.Add(id);
			}
			return roleIds;
		}

		/// <summary>
		/// Adds the specified user names to the specified roles for the configured applicationName.
		/// </summary>
		/// <remarks>Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="usernames">A string array of user names to be added to the specified roles.</param>
		/// <param name="roleNames">A string array of the role names to add the specified user names to.</param>
		/// <exception cref="System.InvalidOperationException"></exception>
		/// <exception cref="System.Configuration.Provider.ProviderException"></exception>
		public override void AddUsersToRoles(string[] usernames, string[] roleNames)
		{
			if (!InitializeCalled)
			{
				PreviousProvider.AddUsersToRoles(usernames, roleNames);
			}
			else
			{
				using (var db = NewMySqlSecurityDbContext)
				{
					int userCount = usernames.Length;
					int roleCount = roleNames.Length;
					List<int> userIds = GetUserIdsFromNames(db, usernames);
					List<int> roleIds = GetRoleIdsFromNames(db, roleNames);
					var affectedRow = 0;

					// Generate a INSERT INTO for each userid/rowid combination, where userIds are the first params, and roleIds follow
					for (int uId = 0; uId < userCount; uId++)
					{
						for (int rId = 0; rId < roleCount; rId++)
						{
							if (IsUserInRole(usernames[uId], roleNames[rId]))
							{
								throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.SimpleRoleProvder_UserAlreadyInRole, usernames[uId], roleNames[rId]));
							}

							// REVIEW: is there a way to batch up these inserts?
							db.UsersInRoles.Add(new UsersInRoles
							{
								UserId = userIds[uId],
								RoleId = roleIds[rId],
							});
							affectedRow++;
						}
					}

					if (db.SaveChanges() != affectedRow)
					{
						throw new ProviderException(Resources.Security_DbFailure);
					}
				}
			}
		}

		/// <summary>
		/// Adds a new role to the data source for the configured applicationName.
		/// </summary>
		/// <remarks>Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="roleName">The name of the role to create.</param>
		/// <exception cref="System.InvalidOperationException"></exception>
		/// <exception cref="System.Configuration.Provider.ProviderException"></exception>
		public override void CreateRole(string roleName)
		{
			if (!InitializeCalled)
			{
				PreviousProvider.CreateRole(roleName);
			}
			else
			{
				using (var db = NewMySqlSecurityDbContext)
				{
					int roleId = FindRoleId(db, roleName);
					if (roleId != -1)
					{
						throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, Resources.SimpleRoleProvider_RoleExists, roleName));
					}

					db.Roles.Add(new Role
					{
						RoleName = roleName
					});

					int rows = db.SaveChanges();
					if (rows != 1)
					{
						throw new ProviderException(Resources.Security_DbFailure);
					}
				}
			}
		}

		/// <summary>
		/// Removes a role from the data source for the configured applicationName.
		/// </summary>
		/// <remarks>Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="roleName">The name of the role to delete.</param>
		/// <param name="throwOnPopulatedRole">If true, throw an exception if <paramref name="roleName" /> has one or more members and do not delete <paramref name="roleName" />.</param>
		/// <returns>true if the role was successfully deleted; otherwise, false.</returns>
		/// <exception cref="System.InvalidOperationException"></exception>
		public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.DeleteRole(roleName, throwOnPopulatedRole);
			}

			using (var db = NewMySqlSecurityDbContext)
			{
				int roleId = FindRoleId(db, roleName);
				if (roleId == -1)
				{
					return false;
				}

				var usersInRoles = db.UsersInRoles.Where(x => x.RoleId == roleId);

				if (throwOnPopulatedRole)
				{
					if (usersInRoles.Count() > 0)
					{
						throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, Resources.SimpleRoleProvder_RolePopulated, roleName));
					}
				}
				else
				{
					// Delete any users in this role first
					foreach (var usersInRole in usersInRoles)
						db.UsersInRoles.Remove(usersInRole);
				}

				var role = db.Roles.SingleOrDefault(x => x.RoleId == roleId);

				db.Roles.Remove(role);

				return (db.SaveChanges() > 0);
			}
		}

		/// <summary>
		/// Gets an array of user names in a role where the user name contains the specified user name to match.
		/// </summary>
		/// <remarks>Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="roleName">The role to search in.</param>
		/// <param name="usernameToMatch">The user name to search for.</param>
		/// <returns>A string array containing the names of all the users where the user name matches <paramref name="usernameToMatch" /> and the user is a member of the specified role.</returns>
		public override string[] FindUsersInRole(string roleName, string usernameToMatch)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.FindUsersInRole(roleName, usernameToMatch);
			}
			using (var db = NewMySqlSecurityDbContext)
			{
				// REVIEW: Is there any way to directly get out a string[]?
				var users = db.UsersInRoles.Where(x => x.Role.RoleName == roleName && x.UserProfile.UserName.Contains(usernameToMatch))
					.Select(x => x.UserProfile.UserName)
					.ToArray();

				return users;
			}
		}

		/// <summary>
		/// Gets a list of all the roles for the configured applicationName.
		/// </summary>
		/// <remarks>Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <returns>A string array containing the names of all the roles stored in the data source for the configured applicationName.</returns>
		public override string[] GetAllRoles()
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.GetAllRoles();
			}
			using (var db = NewMySqlSecurityDbContext)
			{
				var roles = db.Roles.Select(x => x.RoleName).ToArray();

				return roles;
			}
		}

		/// <summary>
		/// Gets a list of the roles that a specified user is in for the configured applicationName.
		/// </summary>
		/// <remarks>Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="username">The user to return a list of roles for.</param>
		/// <returns>A string array containing the names of all the roles that the specified user is in for the configured applicationName.</returns>
		/// <exception cref="System.InvalidOperationException"></exception>
		public override string[] GetRolesForUser(string username)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.GetRolesForUser(username);
			}
			using (var db = NewMySqlSecurityDbContext)
			{
				int userId = MySqlSimpleMembershipProvider.GetUserId(db, username);
				if (userId == -1)
				{
					throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Security_NoUserFound, username));
				}

				var roles = db.UsersInRoles.Where(x => x.UserId == userId)
					.Select(x => x.Role.RoleName)
					.ToArray();

				return roles;
			}
		}

		/// <summary>
		/// Gets a list of users in the specified role for the configured applicationName.
		/// </summary>
		/// <remarks>Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="roleName">The name of the role to get the list of users for.</param>
		/// <returns>A string array containing the names of all the users who are members of the specified role for the configured applicationName.</returns>
		public override string[] GetUsersInRole(string roleName)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.GetUsersInRole(roleName);
			}
			using (var db = NewMySqlSecurityDbContext)
			{
				var users = db.UsersInRoles.Where(x => x.Role.RoleName == roleName)
					.Select(x => x.UserProfile.UserName)
					.ToArray();

				return users;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
		/// </summary>
		/// <remarks>Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="username">The user name to search for.</param>
		/// <param name="roleName">The role to search in.</param>
		/// <returns>true if the specified user is in the specified role for the configured applicationName; otherwise, false.</returns>
		public override bool IsUserInRole(string username, string roleName)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.IsUserInRole(username, roleName);
			}
			using (var db = NewMySqlSecurityDbContext)
			{
				var count = db.UsersInRoles.Count(x => x.UserProfile.UserName == username && x.Role.RoleName == roleName);

				return (count == 1);
			}
		}

		/// <summary>
		/// Removes the specified user names from the specified roles for the configured applicationName.
		/// </summary>
		/// <remarks>Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="usernames">A string array of user names to be removed from the specified roles.</param>
		/// <param name="roleNames">A string array of role names to remove the specified user names from.</param>
		/// <exception cref="System.InvalidOperationException">
		/// </exception>
		/// <exception cref="System.Configuration.Provider.ProviderException"></exception>
		public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
		{
			if (!InitializeCalled)
			{
				PreviousProvider.RemoveUsersFromRoles(usernames, roleNames);
			}
			else
			{
				foreach (string rolename in roleNames)
				{
					if (!RoleExists(rolename))
					{
						throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.SimpleRoleProvider_NoRoleFound, rolename));
					}
				}

				foreach (string username in usernames)
				{
					foreach (string rolename in roleNames)
					{
						if (!IsUserInRole(username, rolename))
						{
							throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.SimpleRoleProvder_UserNotInRole, username, rolename));
						}
					}
				}

				using (var db = NewMySqlSecurityDbContext)
				{
					List<int> userIds = GetUserIdsFromNames(db, usernames);
					List<int> roleIds = GetRoleIdsFromNames(db, roleNames);
					var affectedRows = 0;

					foreach (int userId in userIds)
					{
						foreach (int roleId in roleIds)
						{
							// Review: Is there a way to do these all in one query?
							var usersInRole = db.UsersInRoles.SingleOrDefault(x => x.UserId == userId && x.RoleId == roleId);

							if (usersInRole != null)
							{
								db.UsersInRoles.Remove(usersInRole);
								affectedRows++;
							}
						}
					}

					if (db.SaveChanges() != affectedRows)
					{
						throw new ProviderException(Resources.Security_DbFailure);
					}
				}
			}
		}

		private static int FindRoleId(MySqlSecurityDbContext db, string roleName)
		{
			var role = db.Roles.SingleOrDefault(x => x.RoleName == roleName);

			if (role != null)
				return role.RoleId;
			else
				return -1;
		}

		/// <summary>
		/// Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
		/// </summary>
		/// <remarks>Inherited from RoleProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="roleName">The name of the role to search for in the data source.</param>
		/// <returns>true if the role name already exists in the data source for the configured applicationName; otherwise, false.</returns>
		public override bool RoleExists(string roleName)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.RoleExists(roleName);
			}
			using (var db = NewMySqlSecurityDbContext)
			{
				return (FindRoleId(db, roleName) != -1);
			}
		}
	}
}
