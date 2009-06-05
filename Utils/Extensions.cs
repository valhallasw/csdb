using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Configuration;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace System.CodeDom.Compiler
{

	public static class IndentedTextWriterExtensions
	{
		public static void StartBlock(this IndentedTextWriter writer)
		{
			writer.WriteLine("{");
			writer.Indent++;
		}

		public static void EndBlock(this IndentedTextWriter writer)
		{
			writer.Indent--;
			writer.WriteLine("}");
		}
	}
}

namespace System.Collections.Generic
{
	public static class CollectionExtensions
	{
		public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
		{
			foreach (T item in items)
			{
				list.Add(item);
			}
		}

		/// <summary>
		/// Concatenates string values that are selected from an IEnumerable (e.g CSV parameter list, with ( and ) )
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="stringSelector"></param>
		/// <param name="prefix"></param>
		/// <param name="delim"></param>
		/// <param name="suffix"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public static string StringConcat<T>(this IEnumerable<T> list, Func<T, string> stringSelector, string prefix, string delim, string suffix)
		{
			StringBuilder sb = new StringBuilder();
			if (!String.IsNullOrEmpty(prefix))
				sb.Append(prefix);
			bool first = true, hasDelim = !String.IsNullOrEmpty(delim);
			foreach (T item in list)
			{
				if (hasDelim)
				{
					if (first)
						first = false;
					else
						sb.Append(delim);
				}
				string s = stringSelector(item);
				if (!String.IsNullOrEmpty(s))
					sb.Append(s);
			}
			if (!String.IsNullOrEmpty(suffix))
				sb.Append(suffix);
			return sb.ToString();
		}

		/// <summary>
		/// Concatenates an IEnumerable of strings
		/// </summary>
		/// <param name="list"></param>
		/// <param name="prefix"></param>
		/// <param name="delim"></param>
		/// <param name="suffix"></param>
		/// <returns></returns>
		//[DebuggerStepThrough]
		public static string StringConcat(this IEnumerable<string> list, string prefix, string delim, string suffix)
		{
			StringBuilder sb = new StringBuilder();
			if (!String.IsNullOrEmpty(prefix))
				sb.Append(prefix);
			bool first = true, hasDelim = !String.IsNullOrEmpty(delim);
			foreach (string item in list)
			{
				if (String.IsNullOrEmpty(item))
					continue;
				if (hasDelim)
				{
					if (first)
						first = false;
					else
						sb.Append(delim);
				}
				sb.Append(item);
			}
			if (!String.IsNullOrEmpty(suffix))
				sb.Append(suffix);
			return sb.ToString();
		}
		/// <summary>
		/// Concatenates an IEnumerable of strings
		/// </summary>
		/// <param name="list"></param>
		/// <param name="delim"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public static string StringConcat(this IEnumerable<string> list, string delim)
		{
			return StringConcat(list, null, delim, null);
		}

	}
}
namespace System
{
	public static class ArrayExtensions
	{
		public static bool IsNullOrEmpty(this Array array)
		{
			return array == null || array.Length == 0;
		}

		public static bool IsNotNullOrEmpty(this Array array)
		{
			return array != null && array.Length > 0;
		}
	}

	public static class Extensions
	{

		public static string GetValueOrDefaultIfNullOrEmpty(this string s, string defaultValue)
		{
			if (s == null || s.Length == 0)
				return defaultValue;
			return s;
		}

		public static bool IsNullOrEmpty(this string s)
		{
			return s == null || s.Length==0;
		}

		public static bool IsNotNullOrEmpty(this string s)
		{
			return s != null && s.Length>0;
		}
		public static string HtmlEscape(this string s)
		{
			return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\n", "<br/>");
		}

		public static string ReplaceFirst(this string s, string search, string replace)
		{
			return ReplaceFirst(s, search, replace, StringComparison.CurrentCulture);
		}
		public static string ReplaceFirst(this string s, string search, string replace, StringComparison comparisonType)
		{
			int index = s.IndexOf(search, comparisonType);
			if (index != -1)
			{
				string final = String.Concat(s.Substring(0, index), replace, s.Substring(search.Length));
				return final;
			}
			return s;
		}

		public static string FixCamelCasing(this string s)
		{
			StringBuilder sb = new StringBuilder();
			bool first = true;
			foreach (char c in s)
			{
				if (Char.IsUpper(c) && !first)
				{
					sb.Append(' ');
				}
				sb.Append(c);
				first = false;
			}
			return sb.ToString();
		}

		public static string RemoveLast(this string s, int count)
		{
			return s.Substring(0, s.Length - count);
		}

		public static string TrimEnd(this string s, string trimText)
		{
			if (s.EndsWith(trimText))
				return RemoveLast(s, trimText.Length);
			return s;
		}


		public static Pair<string> SplitAt(this string text, int index)
		{
			return SplitAt(text, index, false);
		}

		public static Pair<string> SplitAt(this string text, int index, bool removeIndexChar)
		{
			string s1 = text.Substring(0, index);
			if (removeIndexChar)
				index++;
			string s2 = text.Substring(index);
			return new Pair<string>(s1, s2);
		}

		public static bool EqualsIgnoreCase(this string s1, string s2)
		{
			return String.Compare(s1, s2, true) == 0;
		}
	}

	public struct Pair<T> : IEnumerable<T>
	{
		public Pair(T first, T second)
		{
			First = first;
			Second = second;
		}
		public T First;
		public T Second;

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator()
		{
			yield return First;
			yield return Second;
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			yield return First;
			yield return Second;
		}

		#endregion
	}

	public struct Pair<T1, T2>
	{
		public Pair(T1 first, T2 second)
		{
			First = first;
			Second = second;
		}
		public T1 First;
		public T2 Second;
	}
	////TODO: pair isn't working on client side - invoking the wrong constructor
	//[RunAtClient]
	//[JsType(Name="System.Pair")]
	//public class JsImplPair<T> : IEnumerable<T>
	//{
	//  public JsImplPair()
	//  {
	//  }
	//  public JsImplPair(T first, T second)
	//  {
	//    First = first;
	//    Second = second;
	//  }
	//  public T First;
	//  public T Second;

	//  #region IEnumerable<T> Members

	//  public IEnumerator<T> GetEnumerator()
	//  {
	//    throw new NotImplementedException(" ");
	//  }

	//  System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
	//  {
	//    return GetEnumerator();
	//  }

	//  #endregion
	//}

	//[RunAtClient]
	//[JsType(Name = "System.Pair")]
	//public class JsImplPair<T1, T2>
	//{
	//  public JsImplPair()
	//  {
	//  }
	//  public JsImplPair(T1 first, T2 second)
	//  {
	//    First = first;
	//    Second = second;
	//  }
	//  public T1 First;
	//  public T2 Second;
	//}



}


namespace System.Linq
{
	public static class Extensions
	{
		[DebuggerStepThrough]
		public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
		{
			foreach (T item in items)
				action(item);
		}
	}

}

