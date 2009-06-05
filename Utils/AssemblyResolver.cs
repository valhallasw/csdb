using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Configuration;

namespace CodeRun.Util
{
	/// <summary>
	/// Assembly Resolved Configuration
	/// </summary>
	class AssemblyResolverConfiguration// : IValidatableConfiguration
	{
		/// <summary>
		/// Assembly search path
		/// </summary>
		[DefaultSettingValue("")]
		public string[] BinPath { get; set; }

		#region IValidatableConfiguration Members

		public void ValidateConfiguration()
		{
			foreach (var s in BinPath)
			{
				if (s.IsNullOrEmpty())
					continue;
				if (!Directory.Exists(s))
					throw new Exception("A Folder ( " + s + ") that was specified in CodeRun.AssemblyResolver.BinPath was not found");
			}
		}

		#endregion
	}

	public static class AssemblyResolver
	{
		static AssemblyResolver()
		{
			BinPath = new HashSet<string>();
		}
		static HashSet<string> BinPath;
		//public static void LoadConfiguration()
		//{
		//  LoadConfiguration(ConfigurationHelper.LoadConfigurationSet<AssemblyResolverConfiguration>().BinPath);
		//}

		public static void LoadConfiguration(string[] BinPath)
		{
			if (BinPath.IsNotNullOrEmpty())
				RegisterBinPath(BinPath);
		}

		public static string[] GetBinPaths()
		{
			return BinPath.ToArray();
		}

		static string[] GetPaths(string paths)
		{
			var pathArray = paths.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(t => new DirectoryInfo(t.Trim()).FullName).ToArray();
			return pathArray;
		}
		public static void RegisterBinPath(string paths)
		{
			RegisterBinPath(GetPaths(paths));
		}
		public static void RegisterBinPath(string [] paths)
		{
			foreach (var path in paths.Where(p=>p.IsNotNullOrEmpty()))
			{
				if (BinPath.Add(path) && BinPath.Count == 1)
				{
					Domain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
				}
			}
		}
		static AppDomain _Domain;
		static AppDomain Domain
		{
			get
			{
				if (_Domain == null)
					_Domain = AppDomain.CurrentDomain;
				return _Domain;
			}
		}

		public static void UnRegisterBinPath(string paths)
		{
			if (BinPath.Count > 0)
			{
				Domain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
			}
			BinPath.Clear();
		}

		static Dictionary<string, Assembly> AssemblyCache = new Dictionary<string, Assembly>();
		static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			Assembly asm;
			if (!AssemblyCache.TryGetValue(args.Name, out asm))
			{
				var name = args.Name;
				var index = name.IndexOf(",");
				if (index > 0)
					name = name.Substring(0, index);
				foreach (var path in BinPath)
				{
					var filename = name + ".dll";
					var filepath = Path.Combine(path, filename);
					if (!File.Exists(filepath))
					{
						filepath = Path.ChangeExtension(filepath, ".exe");
					}
					if (File.Exists(filepath))
					{
						asm = Assembly.LoadFrom(filepath);
						if (asm != null)
						{
							AssemblyCache[asm.FullName] = asm;
							Console.WriteLine("Assembly resolver found assembly "+asm.Location);
							break;
						}
					}
				}
			}
			return asm;
		}

	}
}
