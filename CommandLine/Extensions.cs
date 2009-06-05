using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;

namespace CodeRun.Utils.CommandLine
{
	internal static class Extensions
	{


		public static T GetCustomAttribute<T>(this ICustomAttributeProvider target) where T : Attribute
		{
			return (T)target.GetCustomAttributes(typeof(T), false).FirstOrDefault();
		}
	}
}
