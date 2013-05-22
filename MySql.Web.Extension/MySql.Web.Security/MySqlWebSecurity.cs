// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

/**********************************************************************************************************************/
/*	Domain		:	MySql.Web.Security.MySqlWebSecurity
/*	Creator		:	KIM-KIWON\xyz37(Kim Ki Won)
/*	Create		:	Friday, April 12, 2013 10:57 AM
/*	Purpose		:	Provides security and authentication features for ASP.NET Web Pages applications with MySql, including the ability to create user accounts, 
 *					log users in and out, reset or change passwords, and perform related tasks.
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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.WebPages;
using MySql.Web.Extension.Resources;
using WebMatrix.WebData.Resources;

namespace MySql.Web.Security
{
	/// <summary>
	/// Provides security and authentication features for ASP.NET Web Pages applications with MySql, including the ability to create user accounts,
	/// log users in and out, reset or change passwords, and perform related tasks.
	/// </summary>
	public static class MySqlWebSecurity
	{
		/// <summary>
		/// Represents the key to the mySqlSecurityInheritedContextType value in the <seealso cref="System.Configuration.ConfigurationManager.AppSettings"/> property.
		/// </summary>
		public static readonly string MySqlSecurityInheritedContextType = "mySqlSecurityInheritedContextType";
		/// <summary>
		/// Represents the key to the enableMySqlSimpleMembership value in the <seealso cref="System.Configuration.ConfigurationManager.AppSettings"/> property.
		/// </summary>
		public static readonly string EnableMySqlSimpleMembershipKey = "enableMySqlSimpleMembership";

		/// <summary>
		/// Gets a value indicating whether the <see cref="M:InitializeDatabaseConnection"/> method has been initialized.
		/// </summary>
		/// <value>
		/// <c>true</c> if the initialization method has been called; otherwise, <c>false</c>.
		/// </value>
		public static bool Initialized
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the ID for the current user.
		/// </summary>
		/// <value>The ID for the current user.</value>
		public static int CurrentUserId
		{
			get
			{
				return GetUserId(CurrentUserName);
			}
		}

		/// <summary>
		/// Gets the user name for the current user.
		/// </summary>
		/// <value>The name of the current user.</value>
		public static string CurrentUserName
		{
			get
			{
				return Context.User.Identity.Name;
			}
		}

		/// <summary>
		/// Gets a value that indicates whether the current user has a user ID.
		/// </summary>
		/// <value><c>true</c> if the user has a user ID; otherwise, <c>false</c>.</value>
		public static bool HasUserId
		{
			get
			{
				return CurrentUserId != -1;
			}
		}

		/// <summary>
		/// Gets the authentication status of the current user.
		/// </summary>
		/// <value><c>true</c> if the current user is authenticated; otherwise, false. The default is <c>false</c>.</value>
		public static bool IsAuthenticated
		{
			get
			{
				return Request.IsAuthenticated;
			}
		}

		internal static HttpContextBase Context
		{
			get
			{
				return new HttpContextWrapper(HttpContext.Current);
			}
		}

		internal static HttpRequestBase Request
		{
			get
			{
				return Context.Request;
			}
		}

		internal static HttpResponseBase Response
		{
			get
			{
				return Context.Response;
			}
		}

		internal static void PreAppStartInit()
		{
			// Allow use of <add key="EnableSimpleMembershipKey" value="false" /> to disable registration of membership/role providers as default.
			if (ConfigUtil.MySqlSimpleMembershipEnabled)
			{
				// called during PreAppStart, should also hook up the config for MembershipProviders?
				// Replace the AspNetSqlMembershipProvider (which is the default that is registered in root web.config)
				const string BuiltInMembershipProviderName = "MySqlSimpleMembershipProvider";
				var builtInMembership = System.Web.Security.Membership.Providers[BuiltInMembershipProviderName] as MembershipProvider;
				if (builtInMembership != null)
				{
					var mySqlSimpleMembership = CreateDefaultExtendedMembershipProvider(BuiltInMembershipProviderName, currentDefault: builtInMembership);
					System.Web.Security.Membership.Providers.Remove(BuiltInMembershipProviderName);
					System.Web.Security.Membership.Providers.Add(mySqlSimpleMembership);
				}

				System.Web.Security.Roles.Enabled = true;
				const string BuiltInRolesProviderName = "MySqlSimpleRoleProvider";
				var builtInRoles = System.Web.Security.Roles.Providers[BuiltInRolesProviderName] as RoleProvider;
				if (builtInRoles != null)
				{
					var MySqlSimpleRoles = CreateDefaultExtendedRoleProvider(BuiltInRolesProviderName, currentDefault: builtInRoles);
					System.Web.Security.Roles.Providers.Remove(BuiltInRolesProviderName);
					System.Web.Security.Roles.Providers.Add(MySqlSimpleRoles);
				}
			}
		}

		private static MySqlSimpleMembershipProvider VerifyProvider()
		{
			MySqlSimpleMembershipProvider provider = System.Web.Security.Membership.Provider as MySqlSimpleMembershipProvider;
			if (provider == null)
			{
				throw new InvalidOperationException(Resources.Security_NoExtendedMembershipProvider);
			}

			provider.VerifyInitialized(); // Have the provider verify that it's initialized (only our SimpleMembershipProvider does anything here)
			return provider;
		}

		/// <summary>
		/// Initializes the membership system by connecting to a database that contains user information.
		/// </summary>
		/// <param name="connectionStringName">Name of the connection string.</param>
		public static void InitializeDatabaseConnection(string connectionStringName)
		{
			DatabaseConnectionInfo connect = new DatabaseConnectionInfo();
			connect.ConnectionStringName = connectionStringName;
			InitializeProviders(connect);
		}

		/// <summary>
		/// Initializes the membership system by connecting to a database that contains user information.
		/// </summary>
		/// <param name="connectionString">The connection string.</param>
		/// <param name="providerName">Name of the provider.</param>
		public static void InitializeDatabaseConnection(string connectionString, string providerName)
		{
			DatabaseConnectionInfo connect = new DatabaseConnectionInfo();
			connect.ConnectionString = connectionString;
			connect.ProviderName = providerName;
			InitializeProviders(connect);
		}

		private static void InitializeProviders(DatabaseConnectionInfo connect)
		{
			MySqlSimpleMembershipProvider extendedMembership = System.Web.Security.Membership.Provider as MySqlSimpleMembershipProvider;
			if (extendedMembership != null)
			{
				InitializeMembershipProvider(extendedMembership, connect);
			}

			MySqlSimpleRoleProvider extendedRoles = Roles.Provider as MySqlSimpleRoleProvider;
			if (extendedRoles != null)
			{
				InitializeRoleProvider(extendedRoles, connect);
			}

			Initialized = true;
		}

		internal static void InitializeMembershipProvider(MySqlSimpleMembershipProvider extendedMembership, DatabaseConnectionInfo connect)
		{
			if (extendedMembership.InitializeCalled)
			{
				throw new InvalidOperationException(Resources.Security_InitializeAlreadyCalled);
			}
			extendedMembership.ConnectionInfo = connect;
			// We want to validate the user table if we aren't creating them
			extendedMembership.ValidateTable();
			extendedMembership.InitializeCalled = true;
		}

		internal static void InitializeRoleProvider(MySqlSimpleRoleProvider extendedRoles, DatabaseConnectionInfo connect)
		{
			if (extendedRoles.InitializeCalled)
			{
				throw new InvalidOperationException(Resources.Security_InitializeAlreadyCalled);
			}
			extendedRoles.ConnectionInfo = connect;
			extendedRoles.InitializeCalled = true;
		}

		private static MySqlSimpleMembershipProvider CreateDefaultExtendedMembershipProvider(string name, MembershipProvider currentDefault)
		{
			var membership = new MySqlSimpleMembershipProvider(previousProvider: currentDefault);
			NameValueCollection config = new NameValueCollection();
			membership.Initialize(name, config);
			return membership;
		}

		private static MySqlSimpleRoleProvider CreateDefaultExtendedRoleProvider(string name, RoleProvider currentDefault)
		{
			var roleProvider = new MySqlSimpleRoleProvider(previousProvider: currentDefault);
			NameValueCollection config = new NameValueCollection();
			roleProvider.Initialize(name, config);
			return roleProvider;
		}

		/// <summary>
		/// Logins the specified user name.
		/// </summary>
		/// <param name="userName">The user name.</param>
		/// <param name="password">The password for the user.</param>
		/// <param name="persistCookie">(Optional) true to specify that the authentication token in the cookie should be persisted beyond the current session; otherwise false. The default is false.</param>
		/// <returns><c>true</c> if the user was logged in; otherwise, <c>false</c>.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Login is used more consistently in ASP.Net")]
		[SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "This is a helper class, and we are not removing optional parameters from methods in helper classes")]
		public static bool Login(string userName, string password, bool persistCookie = false)
		{
			MySqlSimpleMembershipProvider provider = VerifyProvider();
			bool success = provider.ValidateUser(userName, password);
			if (success)
			{
				FormsAuthentication.SetAuthCookie(userName, persistCookie);
			}
			return success;
		}

		/// <summary>
		/// Logs the user out.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Logout", Justification = "Login is used more consistently in ASP.Net")]
		public static void Logout()
		{
			VerifyProvider();
			FormsAuthentication.SignOut();
		}

		/// <summary>
		/// Changes the password for the specified user.
		/// </summary>
		/// <param name="userName">The user name.</param>
		/// <param name="currentPassword">The current password for the user.</param>
		/// <param name="newPassword">The new password.</param>
		/// <returns><c>true</c> if the password is successfully changed; otherwise, <c>false</c>.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static bool ChangePassword(string userName, string currentPassword, string newPassword)
		{
			VerifyProvider();
			bool success = false;
			try
			{
				var currentUser = System.Web.Security.Membership.GetUser(userName, true /* userIsOnline */);
				success = currentUser.ChangePassword(currentPassword, newPassword);
			}
			catch (ArgumentException)
			{
				// An argument exception is thrown if the new password does not meet the provider's requirements
			}

			return success;
		}

		/// <summary>
		/// Confirms that an account is valid and activates the account.
		/// </summary>
		/// <param name="accountConfirmationToken">A confirmation token to pass to the authentication provider.</param>
		/// <returns><c>true</c> if the account is confirmed; otherwise, <c>false</c>.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static bool ConfirmAccount(string accountConfirmationToken)
		{
			MySqlSimpleMembershipProvider provider = VerifyProvider();
			Debug.Assert(provider != null); // VerifyProvider checks this
			return provider.ConfirmAccount(accountConfirmationToken);
		}

		/// <summary>
		/// Confirms that an account for the specified user name is valid and activates the account.
		/// </summary>
		/// <param name="userName">The user name.</param>
		/// <param name="accountConfirmationToken">A confirmation token to pass to the authentication provider.</param>
		/// <returns><c>true</c> if the account is confirmed; otherwise, <c>false</c>.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static bool ConfirmAccount(string userName, string accountConfirmationToken)
		{
			MySqlSimpleMembershipProvider provider = VerifyProvider();
			Debug.Assert(provider != null); // VerifyProvider checks this
			return provider.ConfirmAccount(userName, accountConfirmationToken);
		}

		/// <summary>
		/// Creates a new membership account using the specified user name and password and optionally lets you specify that the user must explicitly confirm the account.
		/// </summary>
		/// <param name="userName">The user name.</param>
		/// <param name="password">The password.</param>
		/// <param name="requireConfirmationToken">(Optional) true to specify that the account must be confirmed by using the token return value; otherwise, false. The default is false.</param>
		/// <returns>A token that can be sent to the user to confirm the account.</returns>
		/// <exception cref="System.Web.Security.MembershipCreateUserException">username is empty.-or-username already has a membership account.-or-password is empty.-or-password is too long.-or-The database operation failed.</exception>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		[SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "This is a helper class, and we are not removing optional parameters from methods in helper classes")]
		public static string CreateAccount(string userName, string password, bool requireConfirmationToken = false)
		{
			MySqlSimpleMembershipProvider provider = VerifyProvider();
			Debug.Assert(provider != null); // VerifyProvider checks this

			return provider.CreateAccount(userName, password, requireConfirmationToken);
		}

		/// <summary>
		/// Creates a new user profile entry and a new membership account.
		/// </summary>
		/// <param name="userName">The user name.</param>
		/// <param name="password">The password.</param>
		/// <param name="propertyValues">(Optional) A dictionary that contains additional user attributes. The default is null.</param>
		/// <param name="requireConfirmationToken">(Optional) true to specify that the user account must be confirmed; otherwise, false. The default is false.</param>
		/// <returns>A token that can be sent to the user to confirm the user account.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		[SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "This is a helper class, and we are not removing optional parameters from methods in helper classes")]
		public static string CreateUserAndAccount(string userName, string password, object propertyValues = null, bool requireConfirmationToken = false)
		{
			MySqlSimpleMembershipProvider provider = VerifyProvider();
			Debug.Assert(provider != null); // VerifyProvider checks this

			IDictionary<string, object> values = null;

			if (propertyValues != null)
				values = new Dictionary<string, object>(propertyValues as IDictionary<string, object>);

			return provider.CreateUserAndAccount(userName, password, requireConfirmationToken, values);
		}

		/// <summary>
		/// Generates a password reset token that can be sent to a user in email.
		/// </summary>
		/// <param name="userName">The user name.</param>
		/// <param name="tokenExpirationInMinutesFromNow">(Optional) The time in minutes until the password reset token expires. The default is 1440 (24 hours).</param>
		/// <returns>A token to send to the user.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		[SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "This is a helper class, and we are not removing optional parameters from methods in helper classes")]
		public static string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow = 1440)
		{
			MySqlSimpleMembershipProvider provider = VerifyProvider();
			Debug.Assert(provider != null); // VerifyProvider checks this

			return provider.GeneratePasswordResetToken(userName, tokenExpirationInMinutesFromNow);
		}

		/// <summary>
		/// Returns a value that indicates whether the specified user exists in the membership database.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <returns><c>true</c> if the username exists in the user profile table; otherwise, <c>false</c>.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static bool UserExists(string userName)
		{
			VerifyProvider();
			return System.Web.Security.Membership.GetUser(userName) != null;
		}

		/// <summary>
		/// Returns the ID for a user based on the specified user name.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <returns>The user ID.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static int GetUserId(string userName)
		{
			VerifyProvider();
			MembershipUser user = System.Web.Security.Membership.GetUser(userName);
			if (user == null)
			{
				return -1;
			}

			// REVIEW: This cast is breaking the abstraction for the membershipprovider, we basically assume that userids are ints
			return (int)user.ProviderUserKey;
		}

		/// <summary>
		/// Returns a user ID from a password reset token.
		/// </summary>
		/// <param name="token">The password reset token.</param>
		/// <returns>The user ID.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static int GetUserIdFromPasswordResetToken(string token)
		{
			MySqlSimpleMembershipProvider provider = VerifyProvider();
			Debug.Assert(provider != null); // VerifyProvider checks this

			return provider.GetUserIdFromPasswordResetToken(token);
		}

		/// <summary>
		/// Returns a value that indicates whether the user name of the logged-in user matches the specified user name.
		/// </summary>
		/// <param name="userName">The user name to compare the logged-in user name to.</param>
		/// <returns><c>true</c> if the logged-in user name matches userName; otherwise, <c>false</c>.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static bool IsCurrentUser(string userName)
		{
			VerifyProvider();
			return String.Equals(CurrentUserName, userName, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Returns a value that indicates whether the user has been confirmed.
		/// </summary>
		/// <param name="userName">The user name.</param>
		/// <returns><c>true</c> if the user is confirmed; otherwise, <c>false</c>.</returns>
		public static bool IsConfirmed(string userName)
		{
			MySqlSimpleMembershipProvider provider = VerifyProvider();
			Debug.Assert(provider != null); // VerifyProvider checks this

			return provider.IsConfirmed(userName);
		}

		// Make sure the logged on user is same as the one specified by the id
		private static bool IsUserLoggedOn(int userId)
		{
			VerifyProvider();
			return CurrentUserId == userId;
		}

		/// <summary>
		/// If the user is not authenticated, sets the HTTP status to 401 (Unauthorized).
		/// </summary>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static void RequireAuthenticatedUser()
		{
			VerifyProvider();
			var user = Context.User;
			if (user == null || !user.Identity.IsAuthenticated)
			{
				Response.SetStatus(HttpStatusCode.Unauthorized);
			}
		}

		/// <summary>
		/// If the specified user is not logged on, sets the HTTP status to 401 (Unauthorized).
		/// </summary>
		/// <param name="userId">The ID of the user to compare.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static void RequireUser(int userId)
		{
			VerifyProvider();
			if (!IsUserLoggedOn(userId))
			{
				Response.SetStatus(HttpStatusCode.Unauthorized);
			}
		}

		/// <summary>
		/// If the current user does not match the specified user name, sets the HTTP status to 401 (Unauthorized).
		/// </summary>
		/// <param name="userName">The name of the user to compare.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static void RequireUser(string userName)
		{
			VerifyProvider();
			if (!String.Equals(CurrentUserName, userName, StringComparison.OrdinalIgnoreCase))
			{
				Response.SetStatus(HttpStatusCode.Unauthorized);
			}
		}

		/// <summary>
		/// If the current user is not in all of the specified roles, sets the HTTP status code to 401 (Unauthorized).
		/// </summary>
		/// <param name="roles">The roles to check. The current user must be in all of the roles that are passed in this parameter.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static void RequireRoles(params string[] roles)
		{
			VerifyProvider();
			foreach (string role in roles)
			{
				if (!Roles.IsUserInRole(CurrentUserName, role))
				{
					Response.SetStatus(HttpStatusCode.Unauthorized);
					return;
				}
			}
		}

		/// <summary>
		/// Resets the password.
		/// </summary>
		/// <param name="passwordResetToken">The password reset token.</param>
		/// <param name="newPassword">The new password.</param>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static bool ResetPassword(string passwordResetToken, string newPassword)
		{
			MySqlSimpleMembershipProvider provider = VerifyProvider();
			Debug.Assert(provider != null); // VerifyProvider checks this
			return provider.ResetPasswordWithToken(passwordResetToken, newPassword);
		}

		/// <summary>
		/// Returns a value that indicates whether the specified membership account is temporarily locked because of too many failed password attempts in the specified number of seconds.
		/// </summary>
		/// <param name="userName">The user name of the membership account.</param>
		/// <param name="allowedPasswordAttempts">The number of password attempts the user is permitted before the membership account is locked.</param>
		/// <param name="intervalInSeconds">The number of seconds to lock a user account after the number of password attempts exceeds the value in the allowedPasswordAttempts parameter.</param>
		/// <returns><c>true</c> if the membership account is locked; otherwise, <c>false</c>.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static bool IsAccountLockedOut(string userName, int allowedPasswordAttempts, int intervalInSeconds)
		{
			VerifyProvider();
			return IsAccountLockedOut(userName, allowedPasswordAttempts, TimeSpan.FromSeconds(intervalInSeconds));
		}

		/// <summary>
		/// Returns a value that indicates whether the specified membership account is temporarily locked because of too many failed password attempts in the specified time span.
		/// </summary>
		/// <param name="userName">The user name of the membership account.</param>
		/// <param name="allowedPasswordAttempts">The number of password attempts the user is permitted before the membership account is locked.</param>
		/// <param name="interval">The number of seconds to lock out a user account after the number of password attempts exceeds the value in the allowedPasswordAttempts parameter.</param>
		/// <returns><c>true</c> if the membership account is locked; otherwise, <c>false</c>.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static bool IsAccountLockedOut(string userName, int allowedPasswordAttempts, TimeSpan interval)
		{
			MySqlSimpleMembershipProvider provider = VerifyProvider();
			Debug.Assert(provider != null); // VerifyProvider checks this

			return IsAccountLockedOutInternal(provider, userName, allowedPasswordAttempts, interval);
		}

		internal static bool IsAccountLockedOutInternal(MySqlSimpleMembershipProvider provider, string userName, int allowedPasswordAttempts, TimeSpan interval)
		{
			return (provider.GetUser(userName, false) != null &&
					provider.GetPasswordFailuresSinceLastSuccess(userName) > allowedPasswordAttempts &&
					provider.GetLastPasswordFailureDate(userName).Add(interval) > DateTime.Now);
		}

		/// <summary>
		/// Returns the number of times that the password for the specified account was incorrectly entered since the last successful login or since the membership account was created.
		/// </summary>
		/// <param name="userName">The user name of the account.</param>
		/// <returns>The count of failed password attempts for the specified account.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static int GetPasswordFailuresSinceLastSuccess(string userName)
		{
			MySqlSimpleMembershipProvider provider = VerifyProvider();
			Debug.Assert(provider != null); // VerifyProvider checks this

			return provider.GetPasswordFailuresSinceLastSuccess(userName);
		}

		/// <summary>
		/// Returns the date and time when the specified membership account was created.
		/// </summary>
		/// <param name="userName">The user name for the membership account.</param>
		/// <returns>The date and time that the membership account was created, or <see cref="System.DateTime.MinValue"/> if the account creation date is not available.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static DateTime GetCreateDate(string userName)
		{
			MySqlSimpleMembershipProvider provider = VerifyProvider();
			Debug.Assert(provider != null); // VerifyProvider checks this

			return provider.GetCreateDate(userName);
		}

		/// <summary>
		/// Gets the password changed date.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <returns>The date and time when the password was most recently changed, or <see cref="System.DateTime.MinValue"/> if the password has not been changed for this account.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static DateTime GetPasswordChangedDate(string userName)
		{
			MySqlSimpleMembershipProvider provider = VerifyProvider();
			Debug.Assert(provider != null); // VerifyProvider checks this

			return provider.GetPasswordChangedDate(userName);
		}

		/// <summary>
		/// Returns the date and time when an incorrect password was most recently entered for the specified account.
		/// </summary>
		/// <param name="userName">The user name of the membership account.</param>
		/// <returns>The date and time when an incorrect password was most recently entered for this account, or <see cref="System.DateTime.MinValue"/> if an incorrect password has not been entered for this account.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <seealso cref="Initialized"/> method was not called.-or-
		/// The <seealso cref="InitializeDatabaseConnection(string)"/> method was not called.-or-
		/// The <seealso cref="MySqlSimpleMembershipProvider"/> membership provider is not registered in the configuration of your site. 
		/// For more information, contact your site's system administrator.
		/// </exception>
		public static DateTime GetLastPasswordFailureDate(string userName)
		{
			MySqlSimpleMembershipProvider provider = VerifyProvider();
			Debug.Assert(provider != null); // VerifyProvider checks this

			return provider.GetLastPasswordFailureDate(userName);
		}
	}
}
