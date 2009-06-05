using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeRun.Utils.CommandLine;
using System.Reflection;
using System.Data.Linq;
using System.Data.SqlClient;
using System.IO;
using System.Data.Linq.Mapping;
using CodeRun.Util;
using System.Diagnostics;
using Microsoft.SqlServer.Management.Smo;

namespace csdb
{
	class Program
	{
		public static CsdbOptions Options;
		static MSBuildLogger Log;
		static int Main(string[] args)
		{
			Log = new MSBuildLogger(Console.Out);
			try
			{
				Options = new CsdbOptions(args);
				if (Options.Help.GetValueOrDefault())
				{
					CommandLineArgumentsGenerator.GenerateHelp(typeof(CsdbOptions), Console.Out);
					return 0;
				}
				if (!Options.AutoCreate.GetValueOrDefault() && !Options.AutoUpdate.GetValueOrDefault())
				{
					Log.Inform("Please specify either the /autocreate or /autoupdate option.");
					return 0;
				}
				Type type=null;
				if (Options.BinPath.IsNotNullOrEmpty())
					AssemblyResolver.RegisterBinPath(Options.BinPath);
				if (Options.AssemblyFile.IsNotNullOrEmpty())
				{
					var asm = Assembly.LoadFrom(Options.AssemblyFile);
					if (Options.TypeName.IsNullOrEmpty())
					{
						var types = asm.GetTypes().Where(t => t.IsSubclassOf(typeof(DataContext)));
						foreach (var type2 in types)
						{
							CreateOrUpdateDatabase(type2, null, null);
						}
					}
					else
					{
						type = asm.GetType(Options.TypeName, true);
					}
				}
				else if (Options.TypeName.IsNotNullOrEmpty())
				{
					type = Type.GetType(Options.TypeName);
				}
				if (type != null)
				{
					CreateOrUpdateDatabase(type, Options.ConnectionString, Options.ScriptFile);
				}
				return 0;
			}
			catch (Exception e)
			{
				Log.Error("Error during csdb. {0}", e);
				return -1;
			}
			//Console.ReadLine();
		}

		static void CreateOrUpdateDatabase(Type type, string cs, string scriptFile)
		{
			MetaModel model = null;
			if (type != null)
			{
				var ms = new AttributeMappingSource();
				model = ms.GetModel(type);
			}
			if (cs.IsNullOrEmpty())
			{
				try
				{
					var x = (DataContext)Activator.CreateInstance(type, true);
					cs = x.Connection.ConnectionString;
				}
				catch (Exception e)
				{
					throw new Exception("Cannot obtain connection string from DataContext " + type, e);
				}
			}
			if (cs.IsNullOrEmpty())
			{
				throw new Exception("ConnectionString not provided for DataContext: " + type);
			}
			Log.Inform("Updating database from type {0}, {1}", type, cs);
			Log.Inform(cs);
			Log.Inform("{0}", type);
			//dc.Log = Console.Out;
			var ms2 = new MetaSynchronizer(model, cs);
			ms2.ScriptFile = scriptFile;
			ms2.Log = Log;
			ms2.WindowsUsername = Program.Options.WindowsUsername;
			ms2.CreateOrUpdateDatabase(Options.AutoCreate.GetValueOrDefault(), Options.AutoUpdate.GetValueOrDefault());
		}
	}



}


//var mappingSource = new AttributeMappingSource();
//var model = mappingSource.GetModel(type);
//			if (dc.DatabaseExists())
				//{
					//var cb = new SqlConnectionStringBuilder(dc.Connection.ConnectionString);
				//if (cb.AttachDBFilename.IsNullOrEmpty())
				//  throw new NotImplementedException();
				//var file = new FileInfo(cb.AttachDBFilename);
				//var logFile = file.Directory.GetFile(file.GetNameWithoutExtension() + ".ldf");
				//if (file.Exists)
				//{
				//  if (!logFile.Exists)
				//    throw new Exception("Couldn't file log file " + logFile.FullName);
				//  var date = DateTime.Now.ToString("yyyy-MM-dd--mm-ss");
				//  var suffix = "_" + date + ".backup";
				//  file.CopyTo(file.FullName + suffix);
				//  logFile.CopyTo(logFile.FullName + suffix);
				//  dc.DeleteDatabase();
				//}
				//dc.CreateDatabase();
