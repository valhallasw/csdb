using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace csdb
{
	/// <summary>
	/// CodeRun CSDB Configuration
	/// </summary>
	class Configuration
	{
		static Configuration _Current;
		public static Configuration Current
		{
			get
			{
				if (_Current == null)
				{
					var settings = ConfigurationManager.AppSettings;
					_Current = new Configuration
					{
						DefaultDatabaseLocalDirectory = Environment.ExpandEnvironmentVariables(settings["DefaultDatabaseLocalDirectory"] ?? @"%ProgramFiles%\Microsoft SQL Server\MSSQL.1\MSSQL\Data"), 
						DefaultDbServer=settings["DefaultDbServer"] ?? @"localhost\sqlexpress"
					};
				}
				return _Current;
			}
		}
		public bool EnforceUserDatabaseNames { get; set; }
		/// <summary>
		/// Default Database server instance (* REDUNDENT - see Database configuration *)
		/// </summary>
		//[DefaultValue(@"localhost\sqlexpress")]
		public string DefaultDbServer { get; set; }


		//[DefaultValue(@"D:\Cloud\Databases")]
		public string DefaultDatabaseLocalDirectory { get; set; }

	public string DefaultDatabaseRemoteDirectory { get; set; }
	}
}
