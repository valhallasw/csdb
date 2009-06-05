using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Smo;

namespace csdb
{
	static class DbPermissionsHelper
	{
		public static void GrantDbPermissions(Database db, string username)
		{
			var server = db.Parent;
			Login login;
			if (!(server.Logins.Contains(username)))
			{
				login = new Login(server, username);
				login.LoginType = LoginType.WindowsUser;
				login.DefaultDatabase = db.Name;
				login.Create();
			}
			else
			{
				login = server.Logins[username];
			}

			User newUser;
			if (!(db.Users.Contains(username)))
			{
				newUser = new User(db, username);
				newUser.Login = username;
				newUser.UserType = UserType.SqlLogin;

				newUser.Create();
			}
			else
			{
				newUser = db.Users[username];
			}

			AddUserToRoles(newUser, "db_datareader", "db_datawriter", "db_ddladmin");
		}

		private static void AddUserToRole(User user, string role)
		{
			if (!(user.IsMember(role)))
			{
				user.AddToRole(role);
				user.Alter();
			}
		}
		private static void AddUserToRoles(User user, params string[] roles)
		{
			if (roles.IsNullOrEmpty())
				return;
			foreach (var role in roles)
			{
				if (!(user.IsMember(role)))
				{
					user.AddToRole(role);
				}
			}
			user.Alter();
		}
	}
}
