using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using CodeRun.Util;

namespace CodeRun.Utils.CommandLine
{
	public class CommandLineArgumentSyntaxAttribute : Attribute
	{
		public CommandLineArgumentSyntaxAttribute(string format)
		{
			Format = format;
		}
		public string Format { get; private set; }

	}

	public class DefaultCommandLineArgumentAttribute : Attribute
	{

	}
	public class CommandLineArgumentTrimValueCharsAttribute : Attribute
	{
		public CommandLineArgumentTrimValueCharsAttribute(char[] chars)
		{
			TrimValueChars = chars;
		}
		public char[] TrimValueChars { get; private set; }

	}






}
