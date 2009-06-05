using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeRun.Util;
using System.IO;

namespace CodeRun.Utils.CommandLine
{
	public class CommandLineTool<TOptions> where TOptions : class, new()
	{
		public virtual TOptions Options { get; set; }
		public virtual FileInfo ExeFile { get; set; }
		public virtual DirectoryInfo WorkingDirectory { get; set; }
		public virtual ExecuteProgramResult Execute()
		{
			var res = TryExecute();
			if (res.ExitCode != 0)
			{
				throw new Exception(String.Format("Tool {0} failed with exit code {1}. Output: {2}", ExeFile.FullName, res.ExitCode, res.OutputLines.StringConcat("")));
			}
			return res;
		}
		public virtual ExecuteProgramResult TryExecute()
		{
			var res = ProcessHelper.ExecuteProgramWithOutput(ExeFile.FullName, CommandLineArgumentsGenerator.Generate(Options), WorkingDirectory != null ? WorkingDirectory.FullName : Directory.GetCurrentDirectory(), null);
			return res;
		}
	}
}
