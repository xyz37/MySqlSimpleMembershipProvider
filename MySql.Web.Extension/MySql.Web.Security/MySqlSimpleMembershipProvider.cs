// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

/**********************************************************************************************************************/
/*	Domain		:	MySql.Web.Security.MySqlSimpleMembershipProvider
/*	Creator		:	KIM-KIWON\xyz37(Kim Ki Won)
/*	Create		:	Friday, April 12, 2013 10:56 AM
/*	Purpose		:	Provides support for website membership tasks, such as creating accounts, deleting accounts, 
 *					and managing passwords for MySql database.
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
using System.Configuration.Provider;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Helpers;
using System.Web.Security;
using Microsoft.Internal.Web.Utils;
using MySql.Data.MySqlClient;
using MySql.Web.Extension.Common;
using MySql.Web.Extension.Resources;
using WebMatrix.WebData;

namespace MySql.Web.Security
{
	/// <summary>
	/// Provides support for website membership tasks, such as creating accounts, deleting accounts, 
	/// and managing passwords for MySql database.
	/// </summary>
	public class MySqlSimpleMembershipProvider : ExtendedMembershipProvider
	{
		private const int TokenSizeInBytes = 16;
		private readonly MembershipProvider _previousProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlSimpleMembershipProvider"/> class.
		/// </summary>
		public MySqlSimpleMembershipProvider()
			: this(null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlSimpleMembershipProvider"/> class.
		/// </summary>
		/// <param name="previousProvider">The previous provider.</param>
		public MySqlSimpleMembershipProvider(MembershipProvider previousProvider)
		{
			_previousProvider = previousProvider;
			if (_previousProvider != null)
			{
				_previousProvider.ValidatingPassword += (sender, args) =>
				{
					if (!InitializeCalled)
					{
						OnValidatingPassword(args);
					}
				};
			}
		}

		private MembershipProvider PreviousProvider
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
					// by X10-MOBILE\xyz37(Kim Ki Won) in Friday, April 19, 2013 12:28 AM
					var contextInstance = Activator.CreateInstance(Type.GetType(ConfigUtil.MySqlSecurityInheritedContextType, false, true));

					return contextInstance as MySqlSecurityDbContext;
				}
			}
		}

		/// <summary>
		/// Gets a value that indicates whether the membership provider lets users retrieve their passwords.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <value><c>true</c> if [enable password retrieval]; otherwise, <c>false</c>.</value>
		/// <returns>true if the membership provider supports password retrieval; otherwise, false. The default is false.</returns>
		public override bool EnablePasswordRetrieval
		{
			get
			{
				return InitializeCalled ? false : PreviousProvider.EnablePasswordRetrieval;
			}
		}

		/// <summary>
		/// Indicates whether the membership provider is configured to allow users to reset their passwords.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <value><c>true</c> if [enable password reset]; otherwise, <c>false</c>.</value>
		/// <returns>true if the membership provider supports password reset; otherwise, false. The default is true.</returns>
		public override bool EnablePasswordReset
		{
			get
			{
				return InitializeCalled ? false : PreviousProvider.EnablePasswordReset;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <value><c>true</c> if [requires question and answer]; otherwise, <c>false</c>.</value>
		/// <returns>true if a password answer is required for password reset and retrieval; otherwise, false. The default is true.</returns>
		public override bool RequiresQuestionAndAnswer
		{
			get
			{
				return InitializeCalled ? false : PreviousProvider.RequiresQuestionAndAnswer;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <value><c>true</c> if [requires unique email]; otherwise, <c>false</c>.</value>
		/// <returns>true if the membership provider requires a unique e-mail address; otherwise, false. The default is true.</returns>
		public override bool RequiresUniqueEmail
		{
			get
			{
				return InitializeCalled ? false : PreviousProvider.RequiresUniqueEmail;
			}
		}

		/// <summary>
		/// Gets a value indicating the format for storing passwords in the membership data store.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <value>The password format.</value>
		/// <returns>One of the <see cref="T:System.Web.Security.MembershipPasswordFormat" /> values indicating the format for storing passwords in the data store.</returns>
		public override MembershipPasswordFormat PasswordFormat
		{
			get
			{
				return InitializeCalled ? MembershipPasswordFormat.Hashed : PreviousProvider.PasswordFormat;
			}
		}

		/// <summary>
		/// Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <value>The max invalid password attempts.</value>
		/// <returns>The number of invalid password or password-answer attempts allowed before the membership user is locked out.</returns>
		public override int MaxInvalidPasswordAttempts
		{
			get
			{
				return InitializeCalled ? Int32.MaxValue : PreviousProvider.MaxInvalidPasswordAttempts;
			}
		}

		/// <summary>
		/// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <value>The password attempt window.</value>
		/// <returns>The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.</returns>
		public override int PasswordAttemptWindow
		{
			get
			{
				return InitializeCalled ? Int32.MaxValue : PreviousProvider.PasswordAttemptWindow;
			}
		}

		/// <summary>
		/// Gets the minimum length required for a password.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <value>The length of the min required password.</value>
		/// <returns>The minimum length required for a password. </returns>
		public override int MinRequiredPasswordLength
		{
			get
			{
				return InitializeCalled ? 0 : PreviousProvider.MinRequiredPasswordLength;
			}
		}

		/// <summary>
		/// Gets the minimum number of special characters that must be present in a valid password.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <value>The min required non alphanumeric characters.</value>
		/// <returns>The minimum number of special characters that must be present in a valid password.</returns>
		public override int MinRequiredNonAlphanumericCharacters
		{
			get
			{
				return InitializeCalled ? 0 : PreviousProvider.MinRequiredNonAlphanumericCharacters;
			}
		}

		/// <summary>
		/// Gets the regular expression used to evaluate a password.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <value>The password strength regular expression.</value>
		/// <returns>A regular expression used to evaluate a password.</returns>
		public override string PasswordStrengthRegularExpression
		{
			get
			{
				return InitializeCalled ? String.Empty : PreviousProvider.PasswordStrengthRegularExpression;
			}
		}

		/// <summary>
		/// The name of the application using the custom membership provider.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <value>The name of the application.</value>
		/// <exception cref="System.NotSupportedException">Not initialized MySqlSimpleMembershipProvider. 
		/// This provider initialized call by <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string)"/> or <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string, string)"/> methods.
		/// </exception>
		/// <returns>The name of the application using the custom membership provider.</returns>
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

		internal void VerifyInitialized()
		{
			if (!InitializeCalled)
			{
				throw new InvalidOperationException(Resources.Security_InitializeMustBeCalledFirst);
			}
		}

		/// <summary>
		/// Initializes the provider.
		/// </summary>
		/// <remarks>Inherited from ProviderBase - The "previous provider" we get has already been initialized by the Config system, so we shouldn't forward this call</remarks>
		/// <param name="name">The friendly name of the provider.</param>
		/// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
		/// <exception cref="System.ArgumentNullException">The name of the provider is null.</exception>
		/// <exception cref="System.Configuration.Provider.ProviderException">The config of the provider is null.</exception>
		/// <exception cref="System.ArgumentNullException">The name of the provider is null.</exception>
		/// <exception cref="System.ArgumentException">The name of the provider has a length of zero.</exception>
		/// <exception cref="System.InvalidOperationException">An attempt is made to call Initialize on a provider after the provider has already been initialized.</exception>
		public override void Initialize(string name, NameValueCollection config)
		{
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}
			if (String.IsNullOrEmpty(name))
			{
				name = "MySqlSimpleMembershipProvider";
			}
			if (String.IsNullOrEmpty(config["description"]))
			{
				config.Remove("description");
				config.Add("description", "MySql Extended Membership Provider");
			}
			base.Initialize(name, config);

			config.Remove("connectionStringName");
			config.Remove("enablePasswordRetrieval");
			config.Remove("enablePasswordReset");
			config.Remove("requiresQuestionAndAnswer");
			config.Remove("applicationName");
			config.Remove("requiresUniqueEmail");
			config.Remove("maxInvalidPasswordAttempts");
			config.Remove("passwordAttemptWindow");
			config.Remove("passwordFormat");
			config.Remove("name");
			config.Remove("description");
			config.Remove("minRequiredPasswordLength");
			config.Remove("minRequiredNonalphanumericCharacters");
			config.Remove("passwordStrengthRegularExpression");
			config.Remove("hashAlgorithmType");

			if (config.Count > 0)
			{
				string attribUnrecognized = config.GetKey(0);
				if (!String.IsNullOrEmpty(attribUnrecognized))
				{
					throw new ProviderException(String.Format(CultureInfo.CurrentCulture, Resources.SimpleMembership_ProviderUnrecognizedAttribute, attribUnrecognized));
				}
			}
		}

		internal static bool CheckTableExists(MySqlSecurityDbContext db, string tableName)
		{
			// NOTICE: It does not needed in Entity Framework Code First
			// by Kim Ki Won in Saturday, April 13, 2013 11:59 PM
			return true;
		}

		private static void CreateOAuthTokenTableIfNeeded(MySqlSecurityDbContext db)
		{
			// NOTICE: It does not needed in Entity Framework Code First
			// by X10-MOBILE\xyz37(Kim Ki Won) in Sunday, April 14, 2013 12:03 AM
		}

		/// <summary>
		/// Gets the user id.
		/// </summary>
		/// <remarks>Not an override ==> Simple Membership MUST be enabled to use this method</remarks>
		/// <param name="userName">Name of the user.</param>
		/// <returns>System.Int32.</returns>
		public int GetUserId(string userName)
		{
			VerifyInitialized();

			return GetUserId(NewMySqlSecurityDbContext, userName);
		}

		internal static int GetUserId(MySqlSecurityDbContext db, string userName)
		{
			var result = db.UserProfiles.FirstOrDefault(x => x.UserName == userName);

			if (result != null)
				return result.UserId;
			else
				return -1;
		}

		/// <summary>
		/// When overridden in a derived class, returns an ID for a user based on a password reset token.
		/// </summary>
		/// <remarks>Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method</remarks>
		/// <param name="token">The password reset token.</param>
		/// <returns>The user ID.</returns>
		public override int GetUserIdFromPasswordResetToken(string token)
		{
			VerifyInitialized();

			using (var db = NewMySqlSecurityDbContext)
			{
				var result = db.Memberships.SingleOrDefault(x => x.PasswordVerificationToken == token);

				if (result != null)
					return result.UserId;
				else
					return -1;
			}
		}

		/// <summary>
		/// Processes a request to update the password question and answer for a membership user.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="username">The user to change the password question and answer for.</param>
		/// <param name="password">The password for the specified user.</param>
		/// <param name="newPasswordQuestion">The new password question for the specified user.</param>
		/// <param name="newPasswordAnswer">The new password answer for the specified user.</param>
		/// <returns>true if the password question and answer are updated successfully; otherwise, false.</returns>
		/// <exception cref="System.NotSupportedException">Not initialized MySqlSimpleMembershipProvider. 
		/// This provider initialized call by <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string)"/> or <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string, string)"/> methods.
		/// </exception>
		public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.ChangePasswordQuestionAndAnswer(username, password, newPasswordQuestion, newPasswordAnswer);
			}
			throw new NotSupportedException();
		}

		/// <summary>
		/// Sets the confirmed flag for the username if it is correct.
		/// </summary>
		/// <returns>True if the account could be successfully confirmed. False if the username was not found or the confirmation token is invalid.</returns>
		/// <remarks>Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method</remarks>
		public override bool ConfirmAccount(string userName, string accountConfirmationToken)
		{
			VerifyInitialized();

			using (var db = NewMySqlSecurityDbContext)
			{
				Membership membership = db.Memberships
					.SingleOrDefault(x => x.UserProfile.UserName == userName && x.ConfirmationToken == accountConfirmationToken);

				if (membership == null)
					return false;

				string expectedToken = membership.ConfirmationToken;

				if (String.Equals(accountConfirmationToken, expectedToken, StringComparison.Ordinal))
				{
					membership.IsConfirmed = true;

					return db.SaveChanges() > 0;
				}

				return false;
			}
		}

		/// <summary>
		/// Sets the confirmed flag for the username if it is correct.
		/// </summary>
		/// <returns>True if the account could be successfully confirmed. False if the username was not found or the confirmation token is invalid.</returns>
		/// <remarks>Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method.
		/// There is a tiny possibility where this method fails to work correctly. Two or more users could be assigned the same token but specified using different cases.
		/// A workaround for this would be to use the overload that accepts both the user name and confirmation token.
		/// </remarks>
		public override bool ConfirmAccount(string accountConfirmationToken)
		{
			VerifyInitialized();

			using (var db = NewMySqlSecurityDbContext)
			{
				// We need to compare the token using a case insensitive comparison however it seems tricky to do this uniformly across databases when representing the token as a string. 
				// Therefore verify the case on the client
				var membership = db.Memberships.SingleOrDefault(x => x.ConfirmationToken.Equals(accountConfirmationToken, StringComparison.Ordinal));

				if (membership == null)
					return false;

				membership.IsConfirmed = true;

				return db.SaveChanges() > 0;
			}
		}

		/// <summary>
		/// When overridden in a derived class, creates a new user account using the specified user name and password, optionally requiring that the new account must be confirmed before the account is available for use.
		/// </summary>
		/// <remarks>Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method</remarks>
		/// <param name="userName">The user name.</param>
		/// <param name="password">The password.</param>
		/// <param name="requireConfirmationToken">(Optional) true to specify that the account must be confirmed; otherwise, false. The default is false.</param>
		/// <returns>A token that can be sent to the user to confirm the account.</returns>
		/// <exception cref="System.Web.Security.MembershipCreateUserException">
		/// The user was not created. Check the <seealso cref="System.Web.Security.MembershipCreateUserException.StatusCode"/> property for a <seealso cref="System.Web.Security.MembershipCreateStatus"/> value.
		/// </exception>
		public override string CreateAccount(string userName, string password, bool requireConfirmationToken)
		{
			VerifyInitialized();

			if (password.IsEmpty())
			{
				throw new MembershipCreateUserException(MembershipCreateStatus.InvalidPassword);
			}

			string hashedPassword = Crypto.HashPassword(password);
			if (hashedPassword.Length > 128)
			{
				throw new MembershipCreateUserException(MembershipCreateStatus.InvalidPassword);
			}

			if (userName.IsEmpty())
			{
				throw new MembershipCreateUserException(MembershipCreateStatus.InvalidUserName);
			}

			using (var db = NewMySqlSecurityDbContext)
			{
				// Step 1: Check if the user exists in the Users table
				int uid = GetUserId(db, userName);
				if (uid == -1)
				{
					// User not found
					throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
				}

				// Step 2: Check if the user exists in the Membership table: Error if yes.
				var result = db.Memberships.Count(x => x.UserId == uid);
				if (result > 0)
				{
					throw new MembershipCreateUserException(MembershipCreateStatus.DuplicateUserName);
				}

				// Step 3: Create user in Membership table
				string token = null;
				object dbtoken = DBNull.Value;
				if (requireConfirmationToken)
				{
					token = GenerateToken();
					dbtoken = token;
				}
				int defaultNumPasswordFailures = 0;

				db.Memberships.Add(new Membership
				{
					UserId = uid,
					Password = hashedPassword,
					PasswordSalt = string.Empty,
					IsConfirmed = !requireConfirmationToken,
					ConfirmationToken = Convert.ToString(dbtoken),
					CreateDate = DateTime.Now,
					PasswordFailuresSinceLastSuccess = defaultNumPasswordFailures
				});
				int insert = db.SaveChanges();

				if (insert != 1)
				{
					throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
				}
				return token;
			}
		}

		/// <summary>
		/// Adds a new membership user to the data source.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="username">The user name for the new user.</param>
		/// <param name="password">The password for the new user.</param>
		/// <param name="email">The e-mail address for the new user.</param>
		/// <param name="passwordQuestion">The password question for the new user.</param>
		/// <param name="passwordAnswer">The password answer for the new user</param>
		/// <param name="isApproved">Whether or not the new user is approved to be validated.</param>
		/// <param name="providerUserKey">The unique identifier from the membership data source for the user.</param>
		/// <param name="status">A <see cref="T:System.Web.Security.MembershipCreateStatus" /> enumeration value indicating whether the user was created successfully.</param>
		/// <returns>A <see cref="T:System.Web.Security.MembershipUser" /> object populated with the information for the newly created user.</returns>
		/// <exception cref="System.NotSupportedException">Not initialized MySqlSimpleMembershipProvider. 
		/// This provider initialized call by <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string)"/> or <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string, string)"/> methods.
		/// </exception>
		public override MembershipUser CreateUser(
			string username,
			string password,
			string email,
			string passwordQuestion,
			string passwordAnswer,
			bool isApproved,
			object providerUserKey,
			out MembershipCreateStatus status)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.CreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);
			}
			throw new NotSupportedException();
		}

		private void CreateUserRow(MySqlSecurityDbContext db, string userName, IDictionary<string, object> values)
		{
			var newUserProfile = new UserProfile
			{
				UserName = userName,
			};

			if (values != null && values.Count > 0)
			{
				var type = Type.GetType(ConfigUtil.MySqlSecurityInheritedContextType, false, true);
				var contextAssembly = Assembly.GetAssembly(type);
				var userProfileExType = contextAssembly.GetTypes().FirstOrDefault(x => x.BaseType == typeof(MySql.Web.Security.UserProfile));

				if (userProfileExType != null)
				{
					object userProfileEx = Activator.CreateInstance(userProfileExType);
					var userNamePi = userProfileEx.GetType().GetProperty("UserName");

					userNamePi.SetValue(userProfileEx, userName);

					foreach (var key in values.Keys)
					{
						var pi = userProfileExType.GetProperty(key);

						if (pi != null && pi.CanWrite == true)
						{
							object value = values[key];

							if (value == null)
								value = DBNull.Value;

							pi.SetValue(userProfileEx, value);
						}
					}

					var userProfileExDbSet = EntryBy(db, userProfileExType.FullName);	// get DbSet<UserProfile inherited class>
					var addMethod = userProfileExDbSet.GetType().GetMethod("Add");		// get Add method info
					addMethod.Invoke(userProfileExDbSet, new object[] { userProfileEx });		// invoke add UserProfile inherited class object
				}
			}
			else
				db.UserProfiles.Add(newUserProfile);

			int rows = db.SaveChanges();

			if (rows != 1)
			{
				throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
			}
		}

		private object EntryBy(System.Data.Entity.DbContext dbContext, string typeFullName)
		{
			Type contextType = dbContext.GetType();
			Assembly assembly = Assembly.GetAssembly(contextType);
			Type dacType = assembly.GetType(contextType.FullName);
			var pi = dacType.GetProperties().Single(x => x.PropertyType.FullName.Contains(typeFullName));
			var dbSet = pi.GetValue(dbContext, null);

			return dbSet;
		}

		// Not used but CreateUser direct to database
		private void CreateUserRowByDatabase(MySqlSecurityDbContext db, string userName, IDictionary<string, object> values)
		{
			var newUserProfile = new UserProfile
			{
				UserName = userName,
			};
			int rows = -1;

			using (TransactionScope scope = new TransactionScope())
			{
				db.UserProfiles.Add(newUserProfile);
				rows = db.SaveChanges();

				if (values != null && values.Count > 0)
				{
					var user = db.UserProfiles
						.OrderByDescending(x => x.UserName)
						.First(x => x.UserName == userName);
					StringBuilder sql = new StringBuilder("UPDATE UserProfile SET ");

					foreach (var key in values.Keys)
					{
						object value = values[key];

						if (value == null)
							continue;

						if (value is String)
							sql.AppendFormat("{0} = '{1}' ,", key, value);
						else
							sql.AppendFormat("{0} = {1} ,", key, value);
					}

					string sqlCommand = string.Format("{0} WHERE UserId = {1}", sql.ToString(0, sql.Length - 1), user.UserId);
					rows += db.Database.ExecuteSqlCommand(sqlCommand);
				}

				scope.Complete();
			}

			if (rows == 0)
			{
				throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
			}
		}

		/// <summary>
		/// When overridden in a derived class, creates a new user profile and a new membership account.
		/// </summary>
		/// <remarks>Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method</remarks>
		/// <param name="userName">The user name.</param>
		/// <param name="password">The password.</param>
		/// <param name="requireConfirmation">(Optional) true to specify that the user account must be confirmed; otherwise, false. The default is false.</param>
		/// <param name="values">(Optional) A dictionary that contains additional user attributes to store in the user profile. The default is null.</param>
		/// <returns>A token that can be sent to the user to confirm the user account.</returns>
		public override string CreateUserAndAccount(string userName, string password, bool requireConfirmation, IDictionary<string, object> values)
		{
			VerifyInitialized();

			using (var db = NewMySqlSecurityDbContext)
			{
				CreateUserRow(db, userName, values);
				return CreateAccount(userName, password, requireConfirmation);
			}
		}

		/// <summary>
		/// Gets the password for the specified user name from the data source.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="username">The user to retrieve the password for.</param>
		/// <param name="answer">The password answer for the user.</param>
		/// <returns>The password for the specified user name.</returns>
		/// <exception cref="System.NotSupportedException">Not initialized MySqlSimpleMembershipProvider. 
		/// This provider initialized call by <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string)"/> or <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string, string)"/> methods.
		/// </exception>
		public override string GetPassword(string username, string answer)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.GetPassword(username, answer);
			}
			throw new NotSupportedException();
		}

		private static bool SetPassword(MySqlSecurityDbContext db, int userId, string newPassword)
		{
			string hashedPassword = Crypto.HashPassword(newPassword);
			if (hashedPassword.Length > 128)
			{
				throw new ArgumentException(Resources.SimpleMembership_PasswordTooLong);
			}

			// Update new password
			var membership = db.Memberships.SingleOrDefault(x => x.UserId == userId);

			if (membership == null)
				return false;

			membership.Password = hashedPassword;
			membership.PasswordSalt = string.Empty;
			membership.PasswordChangedDate = DateTime.Now;

			return (db.SaveChanges() > 0);
		}

		/// <summary>
		/// Processes a request to update the password for a membership user.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="username">The user to update the password for.</param>
		/// <param name="oldPassword">The current password for the specified user.</param>
		/// <param name="newPassword">The new password for the specified user.</param>
		/// <returns>true if the password was updated successfully; otherwise, false.</returns>
		/// <exception cref="System.ArgumentException">
		/// username
		/// or
		/// oldPassword
		/// or
		/// newPassword
		/// </exception>
		public override bool ChangePassword(string username, string oldPassword, string newPassword)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.ChangePassword(username, oldPassword, newPassword);
			}

			// REVIEW: are commas special in the password?
			if (username.IsEmpty())
			{
				throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "username");
			}
			if (oldPassword.IsEmpty())
			{
				throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "oldPassword");
			}
			if (newPassword.IsEmpty())
			{
				throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "newPassword");
			}

			using (var db = NewMySqlSecurityDbContext)
			{
				int userId = GetUserId(db, username);
				if (userId == -1)
				{
					return false; // User not found
				}

				// First check that the old credentials match
				if (!CheckPassword(db, userId, oldPassword))
				{
					return false;
				}

				return SetPassword(db, userId, newPassword);
			}
		}

		/// <summary>
		/// Resets a user's password to a new, automatically generated password.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="username">The user to reset the password for.</param>
		/// <param name="answer">The password answer for the specified user.</param>
		/// <returns>The new password for the specified user.</returns>
		/// <exception cref="System.NotSupportedException">Not initialized MySqlSimpleMembershipProvider. 
		/// This provider initialized call by <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string)"/> or <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string, string)"/> methods.
		/// </exception>
		public override string ResetPassword(string username, string answer)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.ResetPassword(username, answer);
			}
			throw new NotSupportedException();
		}

		/// <summary>
		/// Gets user information from the data source based on the unique identifier for the membership user. Provides an option to update the last-activity date/time stamp for the user.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="providerUserKey">The unique identifier for the membership user to get information for.</param>
		/// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
		/// <returns>A <see cref="T:System.Web.Security.MembershipUser" /> object populated with the specified user's information from the data source.</returns>
		/// <exception cref="System.NotSupportedException">Not initialized MySqlSimpleMembershipProvider. 
		/// This provider initialized call by <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string)"/> or <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string, string)"/> methods.
		/// </exception>
		public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.GetUser(providerUserKey, userIsOnline);
			}
			throw new NotSupportedException();
		}

		/// <summary>
		/// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="username">The name of the user to get information for.</param>
		/// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
		/// <returns>A <see cref="T:System.Web.Security.MembershipUser" /> object populated with the specified user's information from the data source.</returns>
		public override MembershipUser GetUser(string username, bool userIsOnline)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.GetUser(username, userIsOnline);
			}

			// Due to a bug in v1, GetUser allows passing null / empty values.
			using (var db = NewMySqlSecurityDbContext)
			{
				int userId = GetUserId(db, username);
				if (userId == -1)
				{
					return null; // User not found
				}

				return new MembershipUser(System.Web.Security.Membership.Provider.Name, username, userId, null, null, null, true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
			}
		}

		/// <summary>
		/// Gets the user name associated with the specified e-mail address.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="email">The e-mail address to search for.</param>
		/// <returns>The user name associated with the specified e-mail address. If no match is found, return null.</returns>
		/// <exception cref="System.NotSupportedException">Not initialized MySqlSimpleMembershipProvider. 
		/// This provider initialized call by <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string)"/> or <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string, string)"/> methods.
		/// </exception>
		public override string GetUserNameByEmail(string email)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.GetUserNameByEmail(email);
			}
			throw new NotSupportedException();
		}

		/// <summary>
		/// When overridden in a derived class, deletes the specified membership account.
		/// </summary>
		/// <remarks>Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method</remarks>
		/// <param name="userName">The user name.</param>
		/// <returns>true if the user account was deleted; otherwise, false.</returns>
		public override bool DeleteAccount(string userName)
		{
			VerifyInitialized();

			using (var db = NewMySqlSecurityDbContext)
			{
				int userId = GetUserId(db, userName);
				if (userId == -1)
				{
					return false; // User not found
				}

				var membership = db.Memberships.Single(x => x.UserId == userId);

				db.Memberships.Remove(membership);

				return (db.SaveChanges() == 1);
			}
		}

		/// <summary>
		/// Removes a user from the membership data source.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="username">The name of the user to delete.</param>
		/// <param name="deleteAllRelatedData">true to delete data related to the user from the database; false to leave data related to the user in the database.</param>
		/// <returns>true if the user was successfully deleted; otherwise, false.</returns>
		public override bool DeleteUser(string username, bool deleteAllRelatedData)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.DeleteUser(username, deleteAllRelatedData);
			}

			using (var db = NewMySqlSecurityDbContext)
			{
				int userId = GetUserId(db, username);
				if (userId == -1)
				{
					return false; // User not found
				}

				var user = db.UserProfiles.Single(x => x.UserId == userId);

				db.UserProfiles.Remove(user);

				bool returnValue = (db.SaveChanges() == 1);

				//if (deleteAllRelatedData) {
				// REVIEW: do we really want to delete from the user table?
				//}
				return returnValue;
			}
		}

		internal bool DeleteUserAndAccountInternal(string userName)
		{
			return (DeleteAccount(userName) && DeleteUser(userName, false));
		}

		/// <summary>
		/// Gets a collection of all the users in the data source in pages of data.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex" /> is zero-based.</param>
		/// <param name="pageSize">The size of the page of results to return.</param>
		/// <param name="totalRecords">The total number of matched users.</param>
		/// <returns>A <see cref="T:System.Web.Security.MembershipUserCollection" /> collection that contains a page of <paramref name="pageSize" /><see cref="T:System.Web.Security.MembershipUser" /> objects beginning at the page specified by <paramref name="pageIndex" />.</returns>
		/// <exception cref="System.NotSupportedException">Not initialized MySqlSimpleMembershipProvider. 
		/// This provider initialized call by <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string)"/> or <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string, string)"/> methods.
		/// </exception>
		public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.GetAllUsers(pageIndex, pageSize, out totalRecords);
			}
			throw new NotSupportedException();
		}

		/// <summary>
		/// Gets the number of users currently accessing the application.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <returns>The number of users currently accessing the application.</returns>
		/// <exception cref="System.NotSupportedException">Not initialized MySqlSimpleMembershipProvider. 
		/// This provider initialized call by <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string)"/> or <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string, string)"/> methods.
		/// </exception>
		public override int GetNumberOfUsersOnline()
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.GetNumberOfUsersOnline();
			}
			throw new NotSupportedException();
		}

		/// <summary>
		/// Gets a collection of membership users where the user name contains the specified user name to match.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="usernameToMatch">The user name to search for.</param>
		/// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex" /> is zero-based.</param>
		/// <param name="pageSize">The size of the page of results to return.</param>
		/// <param name="totalRecords">The total number of matched users.</param>
		/// <returns>A <see cref="T:System.Web.Security.MembershipUserCollection" /> collection that contains a page of <paramref name="pageSize" /><see cref="T:System.Web.Security.MembershipUser" /> objects beginning at the page specified by <paramref name="pageIndex" />.</returns>
		/// <exception cref="System.NotSupportedException">Not initialized MySqlSimpleMembershipProvider. 
		/// This provider initialized call by <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string)"/> or <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string, string)"/> methods.
		/// </exception>
		public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.FindUsersByName(usernameToMatch, pageIndex, pageSize, out totalRecords);
			}
			throw new NotSupportedException();
		}

		/// <summary>
		/// Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="emailToMatch">The e-mail address to search for.</param>
		/// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex" /> is zero-based.</param>
		/// <param name="pageSize">The size of the page of results to return.</param>
		/// <param name="totalRecords">The total number of matched users.</param>
		/// <returns>A <see cref="T:System.Web.Security.MembershipUserCollection" /> collection that contains a page of <paramref name="pageSize" /><see cref="T:System.Web.Security.MembershipUser" /> objects beginning at the page specified by <paramref name="pageIndex" />.</returns>
		/// <exception cref="System.NotSupportedException">Not initialized MySqlSimpleMembershipProvider. 
		/// This provider initialized call by <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string)"/> or <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string, string)"/> methods.
		/// </exception>
		public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);
			}
			throw new NotSupportedException();
		}

		private static int GetPasswordFailuresSinceLastSuccess(MySqlSecurityDbContext db, int userId)
		{
			var membership = db.Memberships.SingleOrDefault(x => x.UserId == userId);

			if (membership != null)
				return membership.PasswordFailuresSinceLastSuccess;
			else
				return -1;
		}

		/// <summary>
		/// When overridden in a derived class, returns the number of times that the password for the specified user account was incorrectly entered since the most recent successful login or since the user account was created.
		/// </summary>
		/// <remarks>Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method</remarks>
		/// <param name="userName">The user name of the account.</param>
		/// <returns>The count of failed password attempts for the specified user account.</returns>
		/// <exception cref="System.InvalidOperationException">No user found.</exception>
		public override int GetPasswordFailuresSinceLastSuccess(string userName)
		{
			using (var db = NewMySqlSecurityDbContext)
			{
				int userId = GetUserId(db, userName);
				if (userId == -1)
				{
					throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Security_NoUserFound, userName));
				}

				return GetPasswordFailuresSinceLastSuccess(db, userId);
			}
		}

		/// <summary>
		/// When overridden in a derived class, returns the date and time when the specified user account was created.
		/// </summary>
		/// <remarks>Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method</remarks>
		/// <param name="userName">The user name of the account.</param>
		/// <returns>The date and time the account was created, or <see cref="F:System.DateTime.MinValue" /> if the account creation date is not available.</returns>
		/// <exception cref="System.InvalidOperationException">No user found.</exception>
		public override DateTime GetCreateDate(string userName)
		{
			using (var db = NewMySqlSecurityDbContext)
			{
				int userId = GetUserId(db, userName);
				if (userId == -1)
				{
					throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Security_NoUserFound, userName));
				}

				var membership = db.Memberships.SingleOrDefault(x => x.UserId == userId);

				if (membership != null)
					return membership.CreateDate ?? DateTime.MinValue;
				else
					return DateTime.MinValue;
			}
		}

		/// <summary>
		/// When overridden in a derived class, returns the date and time when the password was most recently changed for the specified membership account.
		/// </summary>
		/// <remarks>Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method</remarks>
		/// <param name="userName">The user name of the account.</param>
		/// <returns>The date and time when the password was more recently changed for membership account, or <see cref="F:System.DateTime.MinValue" /> if the password has never been changed for this user account.</returns>
		/// <exception cref="System.InvalidOperationException">No user found.</exception>
		public override DateTime GetPasswordChangedDate(string userName)
		{
			using (var db = NewMySqlSecurityDbContext)
			{
				int userId = GetUserId(db, userName);
				if (userId == -1)
				{
					throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Security_NoUserFound, userName));
				}

				var membership = db.Memberships.SingleOrDefault(x => x.UserId == userId);

				if (membership != null)
					return membership.PasswordChangedDate ?? DateTime.MinValue;
				else
					return DateTime.MinValue;
			}
		}

		/// <summary>
		/// When overridden in a derived class, returns the date and time when an incorrect password was most recently entered for the specified user account.
		/// </summary>
		/// <remarks>Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method</remarks>
		/// <param name="userName">The user name of the account.</param>
		/// <returns>The date and time when an incorrect password was most recently entered for this user account, or <see cref="F:System.DateTime.MinValue" /> if an incorrect password has not been entered for this user account.</returns>
		/// <exception cref="System.InvalidOperationException">No user found.</exception>
		public override DateTime GetLastPasswordFailureDate(string userName)
		{
			using (var db = NewMySqlSecurityDbContext)
			{
				int userId = GetUserId(db, userName);
				if (userId == -1)
				{
					throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Security_NoUserFound, userName));
				}

				var membership = db.Memberships.SingleOrDefault(x => x.UserId == userId);

				if (membership != null)
					return membership.LastPasswordFailureDate ?? DateTime.MinValue;
				else
					return DateTime.MinValue;
			}
		}

		private bool CheckPassword(MySqlSecurityDbContext db, int userId, string password)
		{
			string hashedPassword = GetHashedPassword(db, userId);
			bool verificationSucceeded = (hashedPassword != null && Crypto.VerifyHashedPassword(hashedPassword, password));
			var membership = db.Memberships.SingleOrDefault(x => x.UserId == userId);

			if (verificationSucceeded)
			{
				if (membership != null)
					membership.PasswordFailuresSinceLastSuccess = 0;
			}
			else
			{
				int failures = GetPasswordFailuresSinceLastSuccess(db, userId);
				if (failures != -1)
				{
					if (membership != null)
					{
						membership.PasswordFailuresSinceLastSuccess = failures + 1;
						membership.LastPasswordFailureDate = DateTime.Now;
					}
				}
			}

			db.SaveChanges();

			return verificationSucceeded;
		}

		private string GetHashedPassword(MySqlSecurityDbContext db, int userId)
		{
			var membership = db.Memberships.SingleOrDefault(x => x.UserId == userId);

			if (membership != null)
				return membership.Password;
			else
				return null;
		}

		// Ensures the user exists in the accounts table
		private int VerifyUserNameHasConfirmedAccount(MySqlSecurityDbContext db, string userName, bool throwException)
		{
			int userId = GetUserId(db, userName);
			if (userId == -1)
			{
				if (throwException)
				{
					throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Security_NoUserFound, userName));
				}
				else
				{
					return -1;
				}
			}

			int result = db.Memberships.Count(x => x.UserId == userId && x.IsConfirmed == true);

			if (result == 0)
			{
				if (throwException)
				{
					throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Security_NoAccountFound, userName));
				}
				else
				{
					return -1;
				}
			}
			return userId;
		}

		private static string GenerateToken()
		{
			using (var prng = new RNGCryptoServiceProvider())
			{
				return GenerateToken(prng);
			}
		}

		internal static string GenerateToken(RandomNumberGenerator generator)
		{
			byte[] tokenBytes = new byte[TokenSizeInBytes];
			generator.GetBytes(tokenBytes);
			return HttpServerUtility.UrlTokenEncode(tokenBytes);
		}

		/// <summary>
		/// When overridden in a derived class, generates a password reset token that can be sent to a user in email.
		/// </summary>
		/// <remarks>Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method</remarks>
		/// <param name="userName">The user name.</param>
		/// <param name="tokenExpirationInMinutesFromNow">(Optional) The time, in minutes, until the password reset token expires. The default is 1440 (24 hours).</param>
		/// <returns>A token to send to the user.</returns>
		/// <exception cref="System.ArgumentException">userName</exception>
		/// <exception cref="System.Configuration.Provider.ProviderException">Uesr not found or database failure.</exception>
		public override string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow)
		{
			VerifyInitialized();
			if (userName.IsEmpty())
			{
				throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "userName");
			}
			using (var db = NewMySqlSecurityDbContext)
			{
				int userId = VerifyUserNameHasConfirmedAccount(db, userName, throwException: true);
				var membership = db.Memberships.SingleOrDefault(x => x.UserId == userId && x.PasswordVerificationTokenExpirationDate > DateTime.Now);

				if (membership != null)
				{
					string token = membership.PasswordVerificationToken;
					if (token.IsEmpty())
					{
						token = GenerateToken();

						var newMembership = db.Memberships.SingleOrDefault(x => x.UserId == userId);

						if (newMembership != null)
						{
							newMembership.PasswordVerificationToken = token;
							newMembership.PasswordVerificationTokenExpirationDate = DateTime.Now.AddMinutes(tokenExpirationInMinutesFromNow);
							db.SaveChanges();
						}
						else
							throw new ProviderException(Resources.Security_DbFailure);
					}
					else
					{
						// TODO: should we update expiry again?
					}
					return token;
				}
				else
					return string.Empty;
			}
		}

		/// <summary>
		/// When overridden in a derived class, returns a value that indicates whether the user account has been confirmed by the provider.
		/// </summary>
		/// <remarks>Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method</remarks>
		/// <param name="userName">The user name.</param>
		/// <returns>true if the user is confirmed; otherwise, false.</returns>
		/// <exception cref="System.ArgumentException">userName</exception>
		public override bool IsConfirmed(string userName)
		{
			VerifyInitialized();
			if (userName.IsEmpty())
			{
				throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "userName");
			}

			using (var db = NewMySqlSecurityDbContext)
			{
				int userId = VerifyUserNameHasConfirmedAccount(db, userName, throwException: false);
				return (userId != -1);
			}
		}

		/// <summary>
		/// When overridden in a derived class, resets a password after verifying that the specified password reset token is valid.
		/// </summary>
		/// <remarks>Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method</remarks>
		/// <param name="token">A password reset token.</param>
		/// <param name="newPassword">The new password.</param>
		/// <returns>true if the password was changed; otherwise, false.</returns>
		/// <exception cref="System.ArgumentException">newPassword</exception>
		/// <exception cref="System.Configuration.Provider.ProviderException">Uesr not found or database failure.</exception>
		public override bool ResetPasswordWithToken(string token, string newPassword)
		{
			VerifyInitialized();
			if (newPassword.IsEmpty())
			{
				throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "newPassword");
			}
			using (var db = NewMySqlSecurityDbContext)
			{
				var membership = db.Memberships.SingleOrDefault(x => x.PasswordVerificationToken == token && x.PasswordVerificationTokenExpirationDate > DateTime.Now);

				if (membership != null)
				{
					int userId = membership.UserId;
					bool success = SetPassword(db, userId, newPassword);
					if (success)
					{
						// Clear the Token on success
						var newMembership = db.Memberships.SingleOrDefault(x => x.UserId == userId);

						if (newMembership != null)
						{
							newMembership.PasswordVerificationToken = null;
							newMembership.PasswordVerificationTokenExpirationDate = null;
							db.SaveChanges();
						}
						else
							throw new ProviderException(Resources.Security_DbFailure);
					}
					return success;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Updates information about a user in the data source.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="user">A <see cref="T:System.Web.Security.MembershipUser" /> object that represents the user to update and the updated information for the user.</param>
		/// <exception cref="System.NotSupportedException">Not initialized MySqlSimpleMembershipProvider. 
		/// This provider initialized call by <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string)"/> or <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string, string)"/> methods.
		/// </exception>
		public override void UpdateUser(MembershipUser user)
		{
			if (!InitializeCalled)
			{
				PreviousProvider.UpdateUser(user);
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Clears a lock so that the membership user can be validated.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="userName">The membership user whose lock status you want to clear.</param>
		/// <returns>true if the membership user was successfully unlocked; otherwise, false.</returns>
		/// <exception cref="System.NotSupportedException">Not initialized MySqlSimpleMembershipProvider. 
		/// This provider initialized call by <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string)"/> or <seealso cref="MySql.Web.Security.MySqlWebSecurity.InitializeDatabaseConnection(string, string)"/> methods.
		/// </exception>
		public override bool UnlockUser(string userName)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.UnlockUser(userName);
			}
			throw new NotSupportedException();
		}

		internal void ValidateTable()
		{
			using (var db = NewMySqlSecurityDbContext)
			{
				// GetUser will fail with an exception if the user table isn't set up properly
				try
				{
					if (db.Database.CompatibleWithModel(false) == false)
						(new DropCreateMySqlDatabaseAlways<MySqlSecurityDbContext>()).InitializeDatabase(db);
				}
				catch (Exception e)
				{

					throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, Resources.Security_FailedToFindUserTable, "UserProfile"), e);
				}
			}
		}

		/// <summary>
		/// Verifies that the specified user name and password exist in the data source.
		/// </summary>
		/// <remarks>Inherited from MembershipProvider ==> Forwarded to previous provider if this provider hasn't been initialized</remarks>
		/// <param name="username">The name of the user to validate.</param>
		/// <param name="password">The password for the specified user.</param>
		/// <returns>true if the specified username and password are valid; otherwise, false.</returns>
		/// <exception cref="System.ArgumentException">
		/// username
		/// or
		/// password
		/// </exception>
		public override bool ValidateUser(string username, string password)
		{
			if (!InitializeCalled)
			{
				return PreviousProvider.ValidateUser(username, password);
			}
			if (username.IsEmpty())
			{
				throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "username");
			}
			if (password.IsEmpty())
			{
				throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "password");
			}

			using (var db = NewMySqlSecurityDbContext)
			{
				int userId = VerifyUserNameHasConfirmedAccount(db, username, throwException: false);
				if (userId == -1)
				{
					return false;
				}
				else
				{
					return CheckPassword(db, userId, password);
				}
			}
		}

		/// <summary>
		/// Returns the user name that is associated with the specified user ID.
		/// </summary>
		/// <param name="userId">The user ID to get the name for.</param>
		/// <returns>The user name.</returns>
		public override string GetUserNameFromId(int userId)
		{
			VerifyInitialized();

			using (var db = NewMySqlSecurityDbContext)
			{
				var user = db.UserProfiles.SingleOrDefault(x => x.UserId == userId);

				if (user != null)
					return user.UserName;
				else
					return string.Empty;
			}
		}

		/// <summary>
		/// When overridden in a derived class, creates a new OAuth membership account, or updates an existing OAuth Membership account.
		/// </summary>
		/// <param name="provider">The OAuth or OpenID provider.</param>
		/// <param name="providerUserId">The OAuth or OpenID provider user ID. This is not the user ID of the user account, but the user ID on the OAuth or Open ID provider.</param>
		/// <param name="userName">The user name.</param>
		/// <exception cref="System.Web.Security.MembershipCreateUserException">
		/// The user was not created. Check the <seealso cref="System.Web.Security.MembershipCreateUserException.StatusCode"/> property for a <seealso cref="System.Web.Security.MembershipCreateStatus"/> value.
		/// </exception>
		public override void CreateOrUpdateOAuthAccount(string provider, string providerUserId, string userName)
		{
			VerifyInitialized();

			if (userName.IsEmpty())
			{
				throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
			}

			int userId = GetUserId(userName);
			if (userId == -1)
			{
				throw new MembershipCreateUserException(MembershipCreateStatus.InvalidUserName);
			}

			var oldUserId = GetUserIdFromOAuth(provider, providerUserId);
			using (var db = NewMySqlSecurityDbContext)
			{
				if (oldUserId == -1)
				{
					db.OAuthMemberships.Add(new OAuthMembership
					{
						Provider = provider,
						ProviderUserId = providerUserId,
						UserId = userId,
					});
					// account doesn't exist. create a new one.
				}
				else
				{
					// account already exist. update it
					var oAuthMembership = db.OAuthMemberships
						.SingleOrDefault(x => x.Provider.ToUpper() == provider && x.ProviderUserId.ToUpper() == providerUserId);

					if (oAuthMembership != null)
						oAuthMembership.UserId = userId;
				}

				int insert = db.SaveChanges();
				if (insert != 1)
				{
					throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
				}
			}
		}

		/// <summary>
		/// When overridden in a derived class, deletes the OAuth or OpenID account with the specified provider name and provider user ID.
		/// </summary>
		/// <param name="provider">The name of the OAuth or OpenID provider.</param>
		/// <param name="providerUserId">The OAuth or OpenID provider user ID. This is not the user ID of the user account, but the user ID on the OAuth or Open ID provider.</param>
		/// <exception cref="System.Web.Security.MembershipCreateUserException">
		/// The user was not created. Check the <seealso cref="System.Web.Security.MembershipCreateUserException.StatusCode"/> property for a <seealso cref="System.Web.Security.MembershipCreateStatus"/> value.
		/// </exception>
		public override void DeleteOAuthAccount(string provider, string providerUserId)
		{
			VerifyInitialized();

			using (var db = NewMySqlSecurityDbContext)
			{
				// account doesn't exist. create a new one.
				var oAuthMembership = db.OAuthMemberships
					.SingleOrDefault(x => x.Provider.ToUpper() == provider.ToUpper() && x.ProviderUserId.ToUpper() == providerUserId.ToUpper());

				if (oAuthMembership != null)
					db.OAuthMemberships.Remove(oAuthMembership);
				int insert = db.SaveChanges();
				if (insert != 1)
				{
					throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
				}
			}
		}

		/// <summary>
		/// When overridden in a derived class, returns the user ID for the specified OAuth or OpenID provider and provider user ID.
		/// </summary>
		/// <param name="provider">The name of the OAuth or OpenID provider.</param>
		/// <param name="providerUserId">The OAuth or OpenID provider user ID. This is not the user ID of the user account, but the user ID on the OAuth or Open ID provider.</param>
		/// <returns>System.Int32.</returns>
		public override int GetUserIdFromOAuth(string provider, string providerUserId)
		{
			VerifyInitialized();

			using (var db = NewMySqlSecurityDbContext)
			{
				var oAuthMembership = db.OAuthMemberships
					.SingleOrDefault(x => x.Provider.ToUpper() == provider.ToUpper() && x.ProviderUserId.ToUpper() == providerUserId.ToUpper());

				if (oAuthMembership != null)
					return oAuthMembership.UserId;
				else
					return -1;
			}
		}

		/// <summary>
		/// Gets the OAuth token secret.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <returns>System.String.</returns>
		public override string GetOAuthTokenSecret(string token)
		{
			VerifyInitialized();

			using (var db = NewMySqlSecurityDbContext)
			{
				CreateOAuthTokenTableIfNeeded(db);

				// Note that token is case-sensitive
				var oAuthToken = db.OAuthTokens.SingleOrDefault(x => x.Token == token);

				if (oAuthToken != null)
					return oAuthToken.Secret;
				else
					return string.Empty;
			}
		}

		/// <summary>
		/// Stores the OAuth request token.
		/// </summary>
		/// <param name="requestToken">The request token.</param>
		/// <param name="requestTokenSecret">The request token secret.</param>
		/// <exception cref="System.Configuration.Provider.ProviderException">Uesr not found or database failure.</exception>
		public override void StoreOAuthRequestToken(string requestToken, string requestTokenSecret)
		{
			VerifyInitialized();

			string existingSecret = GetOAuthTokenSecret(requestToken);

			using (var db = NewMySqlSecurityDbContext)
			{
				if (existingSecret != null)
				{
					if (existingSecret == requestTokenSecret)
					{
						// the record already exists
						return;
					}

					CreateOAuthTokenTableIfNeeded(db);

					var oAuthToken = db.OAuthTokens.SingleOrDefault(x => x.Token == requestToken);

					if (oAuthToken != null)
						// the token exists with old secret, update it to new secret
						oAuthToken.Secret = requestTokenSecret;
				}
				else
				{
					CreateOAuthTokenTableIfNeeded(db);

					// insert new record
					db.OAuthTokens.Add(new OAuthToken
					{
						Token = requestToken,
						Secret = requestTokenSecret,
					});
				}

				int insert = db.SaveChanges();
				if (insert != 1)
				{
					throw new ProviderException(Resources.SimpleMembership_FailToStoreOAuthToken);
				}
			}
		}

		/// <summary>
		/// Replaces the request token with access token and secret.
		/// </summary>
		/// <param name="requestToken">The request token.</param>
		/// <param name="accessToken">The access token.</param>
		/// <param name="accessTokenSecret">The access token secret.</param>
		public override void ReplaceOAuthRequestTokenWithAccessToken(string requestToken, string accessToken, string accessTokenSecret)
		{
			VerifyInitialized();

			using (var db = NewMySqlSecurityDbContext)
			{
				CreateOAuthTokenTableIfNeeded(db);

				// insert new record
				var oAuthToken = db.OAuthTokens.SingleOrDefault(x => x.Token == requestToken);

				if (oAuthToken != null)
				{
					db.OAuthTokens.Remove(oAuthToken);
					db.SaveChanges();
				}

				// Although there are two different types of tokens, request token and access token,
				// we treat them the same in database records.
				StoreOAuthRequestToken(accessToken, accessTokenSecret);
			}
		}

		/// <summary>
		/// Deletes the OAuth token from the backing store from the database.
		/// </summary>
		/// <param name="token">The token to be deleted.</param>
		public override void DeleteOAuthToken(string token)
		{
			VerifyInitialized();

			using (var db = NewMySqlSecurityDbContext)
			{
				CreateOAuthTokenTableIfNeeded(db);

				// Note that token is case-sensitive
				var oAuthToken = db.OAuthTokens.SingleOrDefault(x => x.Token == token);

				if (oAuthToken != null)
				{
					db.OAuthTokens.Remove(oAuthToken);
					db.SaveChanges();
				}
			}
		}

		/// <summary>
		/// When overridden in a derived class, returns all OAuth membership accounts associated with the specified user name.
		/// </summary>
		/// <param name="userName">The user name.</param>
		/// <returns>A list of all OAuth membership accounts associated with the specified user name.</returns>
		public override ICollection<OAuthAccountData> GetAccountsForUser(string userName)
		{
			VerifyInitialized();

			int userId = GetUserId(userName);
			if (userId != -1)
			{
				using (var db = NewMySqlSecurityDbContext)
				{
					var records = db.OAuthMemberships.Where(x => x.UserId == userId)
						.ToArray()
						.Select(x => new OAuthAccountData(x.Provider, x.ProviderUserId)).ToList();

					return records;
				}
			}

			return new OAuthAccountData[0];
		}

		/// <summary>
		/// Determines whether there exists a local account (as opposed to OAuth account) with the specified userId.
		/// </summary>
		/// <param name="userId">The user id to check for local account.</param>
		/// <returns>
		///   <c>true</c> if there is a local account with the specified user id]; otherwise, <c>false</c>.
		/// </returns>
		public override bool HasLocalAccount(int userId)
		{
			VerifyInitialized();

			using (var db = NewMySqlSecurityDbContext)
			{
				var membership = db.Memberships.SingleOrDefault(x => x.UserId == userId);

				if (membership != null)
					return true;
				else
					return false;
			}
		}
	}
}
