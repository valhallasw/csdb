using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using Microsoft.SqlServer.Management.Smo;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.Collections.Specialized;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Configuration;
using System.Diagnostics;
using CodeRun.Util;
using System.Text.RegularExpressions;
using System.Security;

namespace csdb
{

	internal class MetaSynchronizer
	{
		internal MetaSynchronizer(DataContext dataContext)
		{
			_DataContext = dataContext;
			Log = Logger.Null;
		}
		internal MetaSynchronizer(MetaModel model, string connectionString)
		{
			_Model = model;
			_ConnectionString = connectionString;
			Log = Logger.Null;
		}

		public string ScriptFile { get; set; }

		public ILogger Log { get; set; }

		MetaModel _Model;
		string _ConnectionString;

		public MetaModel Model
		{
			get
			{
				if (_DataContext != null)
					return _DataContext.Mapping;
				return _Model;
			}
		}

		public string ConnectionString
		{
			get
			{
				if (_DataContext != null)
					return _DataContext.Connection.ConnectionString;
				return _ConnectionString;
			}
		}
		DatabaseBuilder DatabaseBuilder;
		public Database Database;
		DataContext _DataContext;
		Server server;
		internal void CreateOrUpdateDatabase()
		{
			CreateOrUpdateDatabase(true, true);
		}
		const string DataDirectoryKeyword = "|DataDirectory|";

		static string DefaultDbServer = Configuration.Current.DefaultDbServer;// ConfigurationManager.App-Settings["DefaultDbServer"] ?? @"localhost\sqlexpress";
		/// <summary>
		/// Taken from CodeRun.DataModel.Database
		/// </summary>
		/// <param name="fileOrServerOrConnectionString"></param>
		/// <returns></returns>
		private static string FixInitialConnectionString(string fileOrServerOrConnectionString)
		{
			//if (fileOrServerOrConnectionString.Contains(DataDirectoryKeyword) && HostingEnvironment.IsHosted)
			//{
			//  fileOrServerOrConnectionString = fileOrServerOrConnectionString.Replace(DataDirectoryKeyword, HostingEnvironment.MapPath("~/App_Data"));
			//}
			if (fileOrServerOrConnectionString.IndexOf('=') >= 0)
			{
				return fileOrServerOrConnectionString;
			}
			DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
			if (fileOrServerOrConnectionString.EndsWith(".mdf", StringComparison.OrdinalIgnoreCase))
			{
				builder.Add("Initial Catalog", fileOrServerOrConnectionString);//changed
				builder.Add("Server", DefaultDbServer);
				builder.Add("Integrated Security", "SSPI");
				//removed builder.Add("User Instance", "true");
				builder.Add("MultipleActiveResultSets", "true");
			}
			else if (fileOrServerOrConnectionString.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
			{
				builder.Add("Data Source", fileOrServerOrConnectionString);
			}
			else
			{
				return fileOrServerOrConnectionString;
				//builder.Add("Server", fileOrServerOrConnectionString);
				//builder.Add("Database", db.Mapping.DatabaseName);
				//builder.Add("Integrated Security", "SSPI");
			}
			return builder.ToString();
		}


		void GrantDbPermissions()
		{
			if (WindowsUsername.IsNotNullOrEmpty())
			{
				GrantDbPermissions(WindowsUsername);
			}
		}

		public string WindowsUsername { get; set; }
		public void GrantDbPermissions(string windowsUsername)
		{
			if (windowsUsername.IsNotNullOrEmpty())
			{
				try
				{
					DbPermissionsHelper.GrantDbPermissions(Database, windowsUsername);
				}
				catch (Exception e)
				{
					Log.Error("Error while setting security permissions on database: " + Database + ", for user: " + windowsUsername + "\r\n" + e.ToString());
				}
			}
			else
			{
				Log.Error("Error while setting security permissions on database: " + Database + ", windowsUsername was not supplied");
			}
		}

 

		internal void CreateOrUpdateDatabase(bool create, bool update)
		{
			var cs = FixInitialConnectionString(ConnectionString);
			var sb = new SqlConnectionStringBuilder(cs);
			server = new Server(sb.DataSource);
			string dbFilename;
			string dbName;
			string remoteDbFilename = null;
			if (sb.InitialCatalog.EndsWith(".mdf"))
			{
				dbFilename = sb.InitialCatalog;
				dbName = dbFilename;//dbFilename.RemoveLast(4);
			}
			else
			{
				dbName = sb.InitialCatalog;
				dbFilename = Program.Options.DatabaseFilename;
				if (dbFilename.IsNullOrEmpty())
				{
					var baseDir = Configuration.Current.DefaultDatabaseLocalDirectory;
					if (baseDir.IsNotNullOrEmpty())
					{
						var remoteDir = Configuration.Current.DefaultDatabaseRemoteDirectory;
						dbFilename = Path.Combine(baseDir, sb.InitialCatalog) + ".mdf";
						if (remoteDir.IsNotNullOrEmpty())
						{
							remoteDbFilename = Path.Combine(remoteDir, sb.InitialCatalog) + ".mdf";
							remoteDbFilename = remoteDbFilename.Replace("{ServerName}", server.Name.Split('\\')[0]);
						}

					}
					else
					{
						throw new Exception("Missing /dbfilename: parameter for database names that do not end with .mdf, you can also add a Coderun.CSDB.DefaultDatabaseDirectory");
					}
				}
			}
			if (Configuration.Current.EnforceUserDatabaseNames && Program.Options.Username.IsNotNullOrEmpty() && !dbName.StartsWith(Program.Options.Username + "\\", StringComparison.InvariantCultureIgnoreCase))
			{
				throw new SecurityException("Invalid database name: "+dbName+", Your database name must begin with: '" + Program.Options.Username + "\\'");
			}
			Database = server.Databases[dbName];
			if (Database == null)
			{
				if (create)
				{
					//var dir = Path.GetDirectoryName(dbFilename);
					//if (!Directory.Exists(dir))
					//  Directory.CreateDirectory(dir);
					CreateDatabase(dbFilename, dbName, remoteDbFilename);
					if(Model!=null)
						UpdateDatabase();
				}
			}
			else
			{
				if (update)
					UpdateDatabase();
			}
			GrantDbPermissions();
			//TODO: Reconnect to the database as the WindowsUsername from here.
			if (ScriptFile.IsNotNullOrEmpty() && Database != null && File.Exists(ScriptFile))
			{
				Log.Inform("Executing script file: " + ScriptFile);
				var script = File.ReadAllText(ScriptFile);
				VerifyScriptSecured(script);
				try
				{
					Database.ExecuteNonQuery(script);
				}
				catch (Exception e)
				{
					Log.Error("Error while executing script file: " + ScriptFile+", "+e.ToString());
				}
			}
			Log.Inform("Finished successfully...");
		}

		void VerifyScriptSecured(string script)
		{
			var keywords = new string[]{"DATABASE", "USE"};
			foreach(var keyword in keywords)
			{
				var index = FindKeywordInScript(script, keyword);
				if (index >= 0)
					throw new Exception("Invalid database script. you cannot use the keyword: " + keyword + " in your script. index=" + index);
			}
		}

		int FindKeywordInScript(string script, string keyword)
		{
			var chars = new string[] {" ", "\n", "\t", "\r"};
			foreach (var prefix in chars)
			{
				foreach (var suffix in chars)
				{
					var x = script.IndexOf(prefix + keyword + suffix);
					if (x >= 0)
						return x;
				}
			}
			return - 1;
			
			//var regex = new Regex("[^ ]*"+keyword+"[ \\n]", RegexOptions.Compiled);
			//var match = regex.Match(script);
			//if (match.Success)
			//  return match.Index;
			//return -1;
		}

		private void CreateDatabase(string dbFilename, string dbName, string remoteDbFilename)
		{
			Log.Inform("Database not found - creating...");
			var accFile = dbFilename;
			if (remoteDbFilename.IsNotNullOrEmpty())
				accFile = remoteDbFilename;
			var dir = Path.GetDirectoryName(accFile);
			if (dir.IsNotNullOrEmpty())
			{
				if (!Directory.Exists(dir))
				{
					Log.Inform("Creating database directory: " + dir);
					Directory.CreateDirectory(dir);
				}
			}
			Database = new Database(server, dbName);
			var fileGroup = new FileGroup(Database, "PRIMARY");
			var dataFile = new DataFile(fileGroup, dbFilename, dbFilename);
			dataFile.Growth = 1024;
			dataFile.GrowthType = FileGrowthType.KB;
			fileGroup.Files.Add(dataFile);

			Database.FileGroups.Add(fileGroup);
			Database.DatabaseOptions.AutoShrink = true;
			server.Databases.Add(new Database(server, dbName));
			Database.Create();
			//server.AttachDatabase(sb.AttachDBFilename, new StringCollection { sb.AttachDBFilename });
		}
		
		void UpdateDatabase()
		{
			if (Model == null)
			{
				Log.Warn("Cannot update database. Model doesn't exist");
				return;
			}
			Log.Inform("Updating Database {0}", Model.DatabaseName);

			DatabaseBuilder = new DatabaseBuilder(Database);
			DatabaseBuilder.Log = Log;
			var dbName = Model.DatabaseName;
			var model = Model;
			if (model.GetTables().FirstOrDefault<MetaTable>() == null)
			{
				Log.Inform("No Tables found, nothing to update...");
				return;
				//throw Common.CreateDatabaseFailedBecauseOfContextWithNoTables(model.DatabaseName);
			}
			foreach (MetaTable table4 in model.GetTables())
			{
				DatabaseBuilder.BuildTable(table4);
			}
			ProcessPendingActions();
			foreach (MetaTable table5 in model.GetTables())
			{
				DatabaseBuilder.BuildForeignKeys(table5);
			}
			ProcessPendingActions();

		}

		private void ProcessPendingActions()
		{
			foreach (var table in DatabaseBuilder.MarkedForCreate)
			{
				table.Create();
			}
			foreach (var table in DatabaseBuilder.MarkedForAlter)
			{
				table.Alter();
			}
			DatabaseBuilder.MarkedForAlter.Clear();
			DatabaseBuilder.MarkedForCreate.Clear();
		}

		#region Stale
		

		//void CreateOrUpdateDatabase_Old()
		//{

		//  var sb = new SqlConnectionStringBuilder(ConnectionString);
		//  var attachDbFilename = sb.AttachDBFilename;
		//  Server server = null;
		//  if (!String.IsNullOrEmpty(attachDbFilename) && File.Exists(attachDbFilename))
		//  {
		//    try
		//    {
		//      server = new Server(sb.DataSource);
		//      Database = server.Databases[sb.AttachDBFilename];
		//      if (Database == null)
		//      {
		//        server.AttachDatabase(sb.AttachDBFilename, new StringCollection { sb.AttachDBFilename });
		//      }
		//      Database = server.Databases[sb.AttachDBFilename];

		//      UpdateDatabase();
		//    }
		//    finally
		//    {
		//      try
		//      {
		//        server.DetachDatabase(sb.AttachDBFilename, false);
		//      }
		//      catch
		//      {
		//      }
		//    }
		//  }
		//  else
		//  {
		//    if (!DataContext.DatabaseExists())
		//    {
		//      DataContext.CreateDatabase();
		//      if (attachDbFilename.IsNotNullOrEmpty())
		//      {
		//        sb.AttachDBFilename = "";
		//        sb.InitialCatalog = "master";
		//        var dbName = DataContext.Connection.Database;
		//        var cs = sb.ConnectionString;// DataContext.Connection.ConnectionString;
		//        var connection = new SqlConnection(cs);
		//        DataContext.Dispose();
		//        //connection.ChangeDatabase("master");
		//        connection.Open();
		//        var cmd = new SqlCommand("exec master.dbo.sp_detach_db @dbname = @p1, @keepfulltextindexfile = @p2", connection);
		//        cmd.Parameters.Add(new SqlParameter("p1", dbName));
		//        cmd.Parameters.Add(new SqlParameter("p2", true));
		//        var res = cmd.ExecuteNonQuery();
		//        connection.Close();
		//        Console.WriteLine("Detach result: " + res);
		//      }
		//      return;
		//    }
		//    throw new NotImplementedException();
		//  }
		//}

		#endregion

	}
}
