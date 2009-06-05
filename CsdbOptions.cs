using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeRun.Utils.CommandLine;

namespace csdb
{
	class CsdbOptions
	{
		internal CsdbOptions(string[] args)
		{
			CommandLineArgumentsParser.ParseInto(this, args);
		}
		[CommandLineArgumentSyntax("/cs:{0}")]
		public string ConnectionString { get; set; }

		[CommandLineArgumentSyntax("/assembly:{0}")]
		public string AssemblyFile { get; set; }

		[CommandLineArgumentSyntax("/binpath:{0}")]
		public string BinPath { get; set; }

		[CommandLineArgumentSyntax("/type:{0}")]
		public string TypeName { get; set; }

		[CommandLineArgumentSyntax("/autocreate")]
		public bool? AutoCreate { get; set; }

		[CommandLineArgumentSyntax("/autoupdate")]
		public bool? AutoUpdate { get; set; }

		[CommandLineArgumentSyntax("/CreateScriptFile:{0}")]
		public string ScriptFile { get; set; }

		[CommandLineArgumentSyntax("/Username:{0}")]
		public string Username { get; set; }

		[CommandLineArgumentSyntax("/WindowsUsername:{0}")]
		public string WindowsUsername { get; set; }
		//[CommandLineArgumentSyntax("/Password:{0}")]
		//public string Password { get; set; }

		[CommandLineArgumentSyntax("/?")]
		public bool? Help { get; set; }

		[CommandLineArgumentSyntax("/dbfilename:{0}")]
		public string DatabaseFilename { get; set; }
	}
}
