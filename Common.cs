using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Reflection;
using System.Collections;
using System.Runtime.InteropServices;

namespace csdb
{
	public static class DataContextExtensions
	{
		public static void CreateOrUpdateDatabase(this DataContext dc, bool create, bool update)
		{
			new MetaSynchronizer(dc).CreateOrUpdateDatabase();
		}
	}


	class Common
	{
		internal static Exception CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(string p)
		{
			return new Exception();
		}

		internal static Exception CouldNotDetermineCatalogName()
		{
			return new Exception();
		}

		internal static Exception CreateDatabaseFailedBecauseOfContextWithNoTables(string p)
		{
			return new Exception("CreateDatabaseFailedBecauseOfContextWithNoTables");
		}

		internal static Exception CreateDatabaseFailedBecauseOfClassWithNoMembers(Type type)
		{
			return new Exception();
		}

		internal static Exception ArgumentOutOfRange(string p)
		{
			return new Exception();
		}

		internal static Exception ArgumentNull(string p)
		{
			return new Exception();
		}

		internal static Exception ArgumentWrongValue(string p)
		{
			return new Exception();
		}

		internal static Exception CouldNotDetermineSqlType(Type type)
		{
			return new Exception();
		}

		internal static Exception CouldNotDetermineDbGeneratedSqlType(Type type)
		{
			return new Exception();
		}
	}

	internal static class InheritanceRules
	{
		// Methods
		internal static bool AreSameMember(MemberInfo mi1, MemberInfo mi2)
		{
			return DistinguishedMemberName(mi1).Equals(DistinguishedMemberName(mi2));
		}

		internal static object DistinguishedMemberName(MemberInfo mi)
		{
			PropertyInfo info = mi as PropertyInfo;
			if (!(mi is FieldInfo))
			{
				if (info == null)
				{
					throw Common.ArgumentOutOfRange("mi");
				}
				MethodInfo getMethod = null;
				if (info.CanRead)
				{
					getMethod = info.GetGetMethod();
				}
				if ((getMethod == null) && info.CanWrite)
				{
					getMethod = info.GetSetMethod();
				}
				if ((getMethod != null) && getMethod.IsVirtual)
				{
					return mi.Name;
				}
			}
			return new MetaPosition(mi);
		}

		#region Not Used

		//internal static object InheritanceCodeForClientCompare(object rawCode, ProviderType providerType)
		//{
		//    if (!providerType.IsFixedSize || (rawCode.GetType() != typeof(string)))
		//    {
		//        return rawCode;
		//    }
		//    string str = (string) rawCode;
		//    if (providerType.Size.HasValue)
		//    {
		//        if (str.Length != providerType.Size)
		//        {
		//            str = str.PadRight(providerType.Size.Value).Substring(0, providerType.Size.Value);
		//        }
		//    }
		//    return str;
		//}
		#endregion

	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct MetaPosition : IEqualityComparer<MetaPosition>, IEqualityComparer
	{
		private int metadataToken;
		private Assembly assembly;
		internal MetaPosition(MemberInfo mi)
			: this(mi.DeclaringType.Assembly, mi.MetadataToken)
		{
		}

		private MetaPosition(Assembly assembly, int metadataToken)
		{
			this.assembly = assembly;
			this.metadataToken = metadataToken;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != base.GetType())
			{
				return false;
			}
			return AreEqual(this, (MetaPosition)obj);
		}

		public override int GetHashCode()
		{
			return this.metadataToken;
		}

		public bool Equals(MetaPosition x, MetaPosition y)
		{
			return AreEqual(x, y);
		}

		public int GetHashCode(MetaPosition obj)
		{
			return obj.metadataToken;
		}

		bool IEqualityComparer.Equals(object x, object y)
		{
			return this.Equals((MetaPosition)x, (MetaPosition)y);
		}

		int IEqualityComparer.GetHashCode(object obj)
		{
			return this.GetHashCode((MetaPosition)obj);
		}

		private static bool AreEqual(MetaPosition x, MetaPosition y)
		{
			return ((x.metadataToken == y.metadataToken) && (x.assembly == y.assembly));
		}

		public static bool operator ==(MetaPosition x, MetaPosition y)
		{
			return AreEqual(x, y);
		}

		public static bool operator !=(MetaPosition x, MetaPosition y)
		{
			return !AreEqual(x, y);
		}

		internal static bool AreSameMember(MemberInfo x, MemberInfo y)
		{
			return ((x.MetadataToken == y.MetadataToken) && (x.DeclaringType.Assembly == y.DeclaringType.Assembly));
		}
	}


}
