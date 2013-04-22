MySql SimpleMembership Provider for ASP.NET MVC 4
=================================================


with Entity Framework 5.x CodeFirst
-----------------------------------


# Libraries
* MySql.Data.Extension : MySql Entity Framework Extension for Entity Framework Code First
* MySql.Web.Extension : MySql SimpleMembership Provider for Entity Framework Code First

# Samples
* SimpleMembershipTest : ASP.NET MVC 4 Simple Membership sample web site
* SimpleMembership.Dac : SimpleMembership Data access control


## MySql.Data.Extension Use only

NuGet Package management 

  * Add Entity Framework 5.x above
  * Add MySql.Net 6.6.4 above

######  In app.config

```xml
<connectionStrings>
  <add name="SimpleMembershipTestDbContext"
		 connectionString="server=localhost;port=3306;database=SimpleMembershipTest;User Id=dev;Password=thePassword;Persist Security Info=True;"
		 providerName="MySql.Data.MySqlClient" />
</connectionStrings>
```

```xml
<entityFramework>
	<defaultConnectionFactory type="MySql.Data.MySqlClient.MySqlClientFactory,MySql.Data" />
</entityFramework>
```

```xml
<system.data>
	<DbProviderFactories>
		<remove invariant="MySql.Data.MySqlClient" />
		<add name="MySQL Data Provider"
			 invariant="MySql.Data.MySqlClient"
			 description=".Net Framework Data Provider for MySQL"
			 type="MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data, Version=6.6.4.0, Culture=neutral, PublicKeyToken=c5687fc88969c44d" />
	</DbProviderFactories>
</system.data>
```

###### DbContext class inherited MySqlDbContext

```java
public class SimpleMembershipTestDbContext : MySqlDbContext
{
		public SimpleMembershipTestDbContext()
			: base("SimpleMembershipTestDbContext")
		{
		}
}
```

###### Grant privileges to user

> mysql --user=root -p

> grant all privileges on SimpleMembershipTest.* to 'dev'@'%' identified by 'thePassword' with grant option;

> -- grant all privileges on **&lt;Database name&gt;**.* to '**&lt;UserName&gt;**'@'%' identified by '**&lt;UserPassword&gt;**' with grant option;

> flush privileges;

> use mysql;

> select host, user from user where user = 'dev';

> select host, user, db from db where user = 'dev';


## MySql.Web.Extension Use

NuGet Package management 

  * Add Entity Framework 5.x above
  * Add MySql.Net 6.6.4 above
  * Microsoft ASP.NET Razor 2
  * Microsoft ASP.NET Web Pages 2
  * Microsoft ASP.NET Web Pages 2 Data
  * Microsoft ASP.NET Web Pages 2 Web Data
  * Microsoft.Web.Infrastructure

* Add MySql.Data.Extension project and grant privilege to user

######  In web.config

```xml
<appSettings>
	<add key="enableMySqlSimpleMembership" value="true" />
	<add key="mySqlSecurityInheritedContextType" value="SimpleMembershipTest.Dac.SimpleMembershipTestDbContext, SimpleMembershipTest.Dac" />
</appSettings>
```

```xml
<connectionStrings>
  <add name="SimpleMembershipTestDbContext"
		 connectionString="server=localhost;port=3306;database=SimpleMembershipTest;User Id=dev;Password=thePassword;Persist Security Info=True;"
		 providerName="MySql.Data.MySqlClient" />
</connectionStrings>
```

```xml
<system.web>
	<membership defaultProvider="MySqlSimpleMembershipProvider">
		<providers>
			<clear />
			<add name="MySqlSimpleMembershipProvider"
				 type="MySql.Web.Security.MySqlSimpleMembershipProvider, MySql.Web.Extension" />
		</providers>
	</membership>

	<roleManager enabled="true" defaultProvider="MySqlSimpleRoleProvider">
		<providers>
			<clear />
			<add name="MySqlSimpleRoleProvider"
				 type="MySql.Web.Security.MySqlSimpleRoleProvider, MySql.Web.Extension" />
		</providers>
	</roleManager>
</system.web>
```

```xml
<entityFramework>
	<defaultConnectionFactory type="MySql.Data.MySqlClient.MySqlClientFactory,MySql.Data" />
</entityFramework>
```

```xml
<system.data>
	<DbProviderFactories>
		<remove invariant="MySql.Data.MySqlClient" />
		<add name="MySQL Data Provider"
			 invariant="MySql.Data.MySqlClient"
			 description=".Net Framework Data Provider for MySQL"
			 type="MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data, Version=6.6.4.0, Culture=neutral, PublicKeyToken=c5687fc88969c44d" />
	</DbProviderFactories>
</system.data>
```

###### DbContext class inherited MySecuritySqlDbContext

Inherited MySqlSecurityDbContext class that generate below tables.

  * UserProfile
  * WebPages_Membership
  * WebPages_OAuthMembership
  * WebPages_OAuthToken
  * WebPages_Rroles
  * WebPages_UsersInRoles
  * RoleMemberships

```java
public class SimpleMembershipTestDbContext : MySqlSecurityDbContext
{
		public SimpleMembershipTestDbContext()
			: base("SimpleMembershipTestDbContext")
		{
		}
}
```

