using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Diagnostics;

namespace CodeRun.Util
{
	public static class ReflectionHelper
	{
		public static T GetCustomAttribute<T>(this Type type) where T:Attribute
		{
			return (T)type.GetCustomAttributes(typeof(T), false).FirstOrDefault();
		}
		public static T GetCustomAttribute<T>(this Type type, bool inherit) where T : Attribute
		{
			return (T)type.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
		}

		//public static string Get
		public static T CreateSpecificOrDefaultInstance<T>(string typeName, params object[] prms)
		{
			Type type;
			if (typeName != null)
			{
				type = Type.GetType(typeName);
			}
			else
			{
				type = typeof(T);
			}
			T instance = (T)Activator.CreateInstance(type, prms);
			return instance;
		}

		public static Type ClimbUp(Type type)
		{
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				return type.GetGenericTypeDefinition();
			}
			return type.BaseType;
		}

		public static object GetDefaultValue(Type type)
		{
			if (type.IsValueType)
			{
				return Activator.CreateInstance(type);
			}
			else
				return null;
		}

		public static Type GetMemberType(MemberInfo mi)
		{
			if (mi is FieldInfo)
				return ((FieldInfo)mi).FieldType;
			return ((PropertyInfo)mi).PropertyType;
		}


		public static object GetMemberValue(MemberInfo mi, object obj)
		{
			if (mi is FieldInfo)
				return ((FieldInfo)mi).GetValue(obj);
			return ((PropertyInfo)mi).GetValue(obj, null);
		}

		public static void SetMemberValue(MemberInfo mi, object obj, object value)
		{
			if (mi is FieldInfo)
				((FieldInfo)mi).SetValue(obj, value);
			else
				((PropertyInfo)mi).SetValue(obj, value, null);
		}


		public static object CreateGenericInstance(Type type, object[] args, params Type[] genericParameters)
		{
			Type genericType = type.MakeGenericType(genericParameters);
			object obj = Activator.CreateInstance(genericType, args);
			return obj;
		}

		//private static Type _GetCollectionItemType(Type collectionType)
		//{
		//  PropertyInfo[] pis = collectionType.GetProperties();
		//  foreach (PropertyInfo pi in pis)
		//  {
		//    if (pi.Name == "Item")
		//    {
		//      return pi.PropertyType;
		//    }
		//  }
		//  return null;
		//}


		//public static Type GetCollectionItemType(Type listType)
		//{
		//  if (listType.IsArray)
		//  {
		//    return listType.GetElementType();
		//  }
		//  else
		//  {
		//    return _GetCollectionItemType(listType);
		//  }
		//}

		//public static T GetAttributes<T>(Assembly asm)
		//{
		//}

		public static IList<T> GetAttributes<T>(ICustomAttributeProvider mi, bool inherit) where T : Attribute //TODO: Optimize (cache attribute access)
		{
			object[] attributes = mi.GetCustomAttributes(typeof(T), inherit);
			List<T> list = new List<T>();
			foreach(T attribute in attributes)
			{
				list.Add(attribute);
			}
			return list;
		}

		public static T GetAttribute<T>(ICustomAttributeProvider mi, bool inherit) where T : Attribute //TODO: Optimize (cache attribute access)
		{
			IList<T> list = GetAttributes<T>(mi, inherit);
			if (list.Count == 0)
				return null;
			return list[0];
		}

		public static T GetAttribute<T>(ICustomAttributeProvider mi) where T : Attribute //TODO: Optimize (cache attribute access)
		{
			return GetAttribute<T>(mi, true);
		}

		public static bool IsCollection(Type realType)
		{
			if (typeof(IList).IsAssignableFrom(realType))
				return true;
			return false;
		}

		public static bool IsNullable(Type type)
		{
			if (type.IsValueType)
			{
				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
					return true;
				return false;
			}
			return true;
		}

		public static T GetAttribute<T>(Assembly asm) where T: Attribute
		{
			object[] list = asm.GetCustomAttributes(typeof(T), false);
			if (list.Length > 0)
				return (T)list[0];
			return null;
		}


		public static Type GetFirstGenericType(Type type)
		{
			Type[] types = type.GetGenericArguments();
			if (types.Length == 0)
			{
				type = type.BaseType;
				if (type != null)
					return GetFirstGenericType(type);
				return null;
			}
			return types[0];
		}


		public static object InvokeMethod(object instance, string methodName, object[] prms)
		{
			MethodInfo mi = instance.GetType().GetMethod(methodName);
			object result = mi.Invoke(instance, prms);
			return result;
		}

		public static object TryInvokeMethodWithParams(object obj, string methodName, object[] prms)
		{
			var types = Type.GetTypeArray(prms);
			MethodInfo mi = obj.GetType().GetMethod(methodName, types);
			if (mi == null)
				return null;
			object result = mi.Invoke(obj, prms);
			return result;
		}
    //Instance
		public static object GetPropertyValue(object instance, string propertyName)
		{
			return instance.GetType().GetProperty(propertyName).GetValue(instance, null);
		}

    //Static
    public static object GetPropertyValue(Type T, string propertyName)
    {
      return T.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null, null);
    }
		public static object GetFieldValue(object instance, string fieldName)
		{
			return instance.GetType().GetField(fieldName).GetValue(instance);
		}

		public static void SetFieldValue(object obj, string fieldName, object value)
		{
			obj.GetType().GetField(fieldName).SetValue(obj, value);
		}


		#region Clr Convertion

		public static object ForceClrConvert(object value, Type targetType)
		{
			object newValue;
			if (!TryClrConvert(value, targetType, out newValue))
			{
				return ReflectionHelper.GetDefaultValue(targetType);
			}
			return newValue;
		}

		public static object ClrConvert(object value, Type targetType)
		{
			object convertedValue;
			if (!TryClrConvert(value, targetType, out convertedValue))
				throw new Exception("could not convert");
			return convertedValue;
		}

		public static bool TryClrConvert(object value, Type targetType, out object convertedValue)
		{
			if (value == null)
			{
				if (targetType.IsValueType)
					convertedValue = ReflectionHelper.GetDefaultValue(targetType);
				else
					convertedValue = null;
				return true;
			}

			bool success = TryClrConvertNonNull(value, targetType, out convertedValue);
			if (success)
				return true;

			if (value.GetType() != typeof(string) && targetType != typeof(string))
			{
				object sValue;
				if (TryClrConvertNonNull(value, typeof(string), out sValue))
				{
					return TryClrConvert(sValue, targetType, out convertedValue);
				}
			}
			convertedValue = null;
			return false;
		}

		public static bool TryClrConvertNonNull(object value, Type targetType, out object convertedValue)
		{
			TypeConverter converter = TypeDescriptor.GetConverter(value);
			try
			{
				if (converter.CanConvertTo(targetType))
				{
					convertedValue = converter.ConvertTo(value, targetType);
					return true;
				}
			}
			catch (Exception e)
			{
				TraceHelper.WriteLineEx("Error", "JavaScriptHelper", "ConvertTo", String.Format("Could not convert value using the ConvertTo method {0} to type {1}, {2}", value, targetType, e), "", "");
			}

			converter = TypeDescriptor.GetConverter(targetType);
			try
			{
				if (converter.CanConvertFrom(value.GetType()))
				{
					convertedValue = converter.ConvertFrom(value);
					return true;
				}
			}
			catch (Exception e)
			{
				TraceHelper.WriteLineEx("Error", "JavaScriptHelper", "ConvertTo", String.Format("Could not convert value {0} to type {1}, {2}", value, targetType, e), "", "");
			}
			convertedValue = null;
			return false;
		}

		#endregion

		public static bool IsStatic(MemberInfo mi)
		{
			PropertyInfo pi = mi as PropertyInfo;
			if (pi != null)
			{
				return pi.GetAccessors()[0].IsStatic;
			}
			FieldInfo fi = mi as FieldInfo;
			if (fi != null)
			{
				return fi.IsStatic;
			}
			throw new NotSupportedException("couldn't determine if memberinfo is static");
		}

		public static bool IsPublic(MemberInfo mi)
		{
			PropertyInfo pi = mi as PropertyInfo;
			if (pi != null)
			{
				return pi.GetAccessors()[0].IsPublic;
			}
			FieldInfo fi = mi as FieldInfo;
			if (fi != null)
			{
				return fi.IsPublic;
			}
			throw new NotSupportedException("couldn't determine if memberinfo is public");
		}


		public static bool IsDefaultValue(Type type, object value)
		{
			var def = GetDefaultValue(type);
			return Object.Equals(def, value);
		}
	}


	public class ExtendedBooleanConverter : BooleanConverter
	{
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			string s = value as string;
			if (s != null)
			{
				if (s.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
					return true;
				if (s.Equals("no", StringComparison.InvariantCultureIgnoreCase))
					return false;
			}
			return base.ConvertFrom(context, culture, value);
		}
	}

	public class ExtendedDecimalConverter : DecimalConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(int))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if (value is int)
			{
				return Convert.ToDecimal(value);
			}
			return base.ConvertFrom(context, culture, value);
		}
	}

	public class ExtendedDateTimeConverter : DateTimeConverter
	{
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if (value is string)
			{
				return DateTime.ParseExact(value as string, "dd/MM/yyyy hh:mm:ss", DateTimeFormatInfo.InvariantInfo);
				//TODO: This is the format JavaScript uses to send dates to the server. The method below returned 01/01/01 for 27/4/2003 -Alon
				//DateTime.TryParse(value as string, out dt);
				//return dt;
			}
			return base.ConvertFrom(context, culture, value);
		}
	}

}
