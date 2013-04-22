using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using SimpleMembershipTest.Models;

namespace SimpleMembershipTest
{
	public static class AuthConfig
	{
		public static void RegisterAuth()
		{
			// To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
			// you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

			//OAuthWebSecurity.RegisterMicrosoftClient(
			//	clientId: "",
			//	clientSecret: ""
			//);

			// https://dev.twitter.com/apps
			//OAuthWebSecurity.RegisterTwitterClient(
			//	consumerKey: "",
			//	consumerSecret: ""
			//);

			// https://developers.facebook.com/apps
			//OAuthWebSecurity.RegisterFacebookClient(
			//	appId: "",
			//	appSecret: ""
			//);			

			//OAuthWebSecurity.RegisterLinkedInClient(
			//	consumerKey: "",
			//	consumerSecret: "",
			//);

			OAuthWebSecurity.RegisterGoogleClient();
			OAuthWebSecurity.RegisterYahooClient();
		}
	}
}
