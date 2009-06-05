using System;
using System.IO;
using System.Collections.Generic;
namespace CodeRun.Util
{
	public interface ILogger
	{
		void Error(string msg, params object[] args);
		void Inform(string msg, params object[] args);
		void Warn(string msg, params object[] args);
		void Write(string format, params object[] args);
		void WriteLine(string format, params object[] args);
		void WriteLine();
	}

	public class Logger
	{
		static Logger()
		{
			Null = new NullLogger();
			Console = new ConsoleLogger();
		}
		public static ILogger Null;
		public static ILogger Console;
	}

	class NullLogger : ILogger
	{

		#region ILogger Members

		public void Error(string msg, params object[] args)
		{
		}

		public void Inform(string msg, params object[] args)
		{
		}

		public void Warn(string msg, params object[] args)
		{
		}

		public void Write(string format, params object[] args)
		{
		}

		public void WriteLine(string format, params object[] args)
		{
		}



		public void WriteLine()
		{
		}

		#endregion
	}

	public class TextWriterLogger : ILogger
	{
		public TextWriterLogger(TextWriter output)
		{
			Output = output;
		}

		TextWriter Output;
		public void Inform(string msg, params object[] args)
		{
			Output.Write("message: ");
			Output.WriteLine(msg, args);
		}

		public void Warn(string msg, params object[] args)
		{
			Output.Write("warning: ");
			Output.WriteLine(msg, args);
		}

		public void Error(string msg, params object[] args)
		{
			Output.Write("error: ");
			Output.WriteLine(msg, args);
		}

		public void Write(string format, params object[] args)
		{
			Output.Write(format, args);
		}

		public void WriteLine(string format, params object[] args)
		{
			Output.WriteLine(format, args);
		}



		public void WriteLine()
		{
			Output.WriteLine();
		}

	}

	public class ConsoleLogger : TextWriterLogger
	{
		public ConsoleLogger()
			: base(Console.Out)
		{
		}

	}

	/// <summary>
	/// Enables Logging in MSBuild format
	/// </summary>
	public class MSBuildLogger : ILogger
	{
		public MSBuildLogger(TextWriter output)
		{
			Output = output;
		}
		public MSBuildLogger(TextWriter output, string filename, int line, int column, string errorCode)
		{
			Output = output;
			Filename = filename;
			Line = line;
			Column = column;
			ErrorCode = errorCode;
		}
		public string Filename;
		public int Line;
		public int Column;
		public string ErrorCode;
		TextWriter Output;
		public List<ILogger> AdditionalLoggers = new List<ILogger>();

		public void Inform(string msg, params object[] args)
		{
			if (msg == null)
				return;
			//Output.Write("message: ");
			Output.WriteLine(msg, args);

			if (AdditionalLoggers != null)
				AdditionalLoggers.ForEach(l => l.Inform(msg, args));
		}

		void StartMessage(string errorKind)
		{
			var msg = String.Format("{0}({1},{2}): {3} {4}: ", Filename, Line, Column, errorKind, ErrorCode);
			Output.Write(msg);
			if (AdditionalLoggers != null)
				AdditionalLoggers.ForEach(l => l.Write(msg));
		}

		public void Warn(string msg, params object[] args)
		{
			StartMessage("warning");
			Output.WriteLine(msg, args);
			if (AdditionalLoggers != null)
				AdditionalLoggers.ForEach(l => l.Warn(msg, args));
		}

		public void Error(string msg, params object[] args)
		{
			StartMessage("error");
			Output.WriteLine(msg, args);
			if (AdditionalLoggers != null)
				AdditionalLoggers.ForEach(l => l.Error(msg, args));
		}

		public void Write(string format, params object[] args)
		{
			Output.Write(format, args);
			if (AdditionalLoggers != null)
				AdditionalLoggers.ForEach(l => l.Write(format, args));
		}

		public void WriteLine(string format, params object[] args)
		{
			Output.WriteLine(format, args);
			if (AdditionalLoggers != null)
				AdditionalLoggers.ForEach(l => l.WriteLine(format, args));
		}



		public void WriteLine()
		{
			Output.WriteLine();
		}

	}


}
