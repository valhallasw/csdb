using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using CodeRun.Util;

namespace csdb
{

	internal class DatabaseBuilder
	{
		Database Database;
		Table CurrentTable;

		public DatabaseBuilder(Database db)
		{
			Database = db;
		}
		public ILogger Log = Logger.Null;

		internal void ParseFullTableName(string fullTableName, out string schemaName, out string tableName)
		{
			var tokens = fullTableName.Split('.');
			if (tokens.Length == 1)
			{
				tableName = tokens[0];
				schemaName = "dbo";
			}
			else
			{
				schemaName = tokens.First();
				tableName = String.Join(".", tokens.Skip(1).ToArray());
			}
		}
		internal void BuildTable(MetaTable mt)
		{
			string schemaName;
			string tableName;
			ParseFullTableName(mt.TableName, out schemaName, out tableName);

			var table = Database.Tables[tableName, schemaName];
			if (table == null)
			{
				Log.Inform("Creating Table {0}.{1}", schemaName, tableName);
				table = new Table(Database, tableName, schemaName);
				Database.Tables.Add(table);
				OnCreated(table);
			}
			CurrentTable = table;
			BuildFieldDeclarations(mt);
			BuildPrimaryKey(mt);
		}


		internal void BuildFieldDeclarations(MetaTable table)
		{
			var count = 0;
			Dictionary<object, string> memberNameToMappedName = new Dictionary<object, string>();
			foreach (MetaType type in table.RowType.InheritanceTypes)
			{
				count += BuildFieldDeclarations(type, memberNameToMappedName);
			}
			if (count == 0)
			{
				throw Common.CreateDatabaseFailedBecauseOfClassWithNoMembers(table.RowType.Type);
			}
		}

		string GetSqlDefaultValue(MetaDataMember member)
		{
			var type = ReflectionHelper.GetMemberType(member.Member);
			if (type == typeof(bool))
				return "0";
			if (type == typeof(DateTime))
				return DateTime.MinValue.ToString("dd/MM/yyyy");
			if (type.IsEnum)
				return "0";
			var defaultValue = ReflectionHelper.GetDefaultValue(type);
			if (defaultValue == null)
				return null;
			return defaultValue.ToString();
		}

		private int BuildFieldDeclarations(MetaType type, Dictionary<object, string> memberNameToMappedName)
		{
			var count = 0;
			foreach (var member in type.DataMembers)
			{
				try
				{
					if ((!member.IsDeclaredBy(type) || member.IsAssociation) || !member.IsPersistent)
					{
						continue;
					}
					string mappedName;
					object key = InheritanceRules.DistinguishedMemberName(member.Member);
					if (!memberNameToMappedName.TryGetValue(key, out mappedName))
						memberNameToMappedName.Add(key, member.MappedName);
					else if (mappedName == member.MappedName)
						continue;
					var col = CurrentTable.Columns[member.MappedName];
					bool created = false;
					if (col == null)
					{
						col = new Column(CurrentTable, member.MappedName);
						CurrentTable.Columns.Add(col);
						OnCreated(col);
						created = true;
					}
					if (!string.IsNullOrEmpty(member.Expression))
					{
						if (col.Computed != true || col.ComputedText != member.Expression)
						{
							col.Computed = true;
							col.ComputedText = member.Expression;
							OnChanged(col);
						}
					}
					else
					{
						var dataType = GetDbType(member);
						if (!DataTypeEquals(col.DataType, dataType) || col.Nullable != member.CanBeNull || (col.Identity != member.IsDbGenerated && member.IsPrimaryKey))
						{

							col.DataType = dataType;
							col.Nullable = member.CanBeNull;
							col.Identity = member.IsDbGenerated && member.IsPrimaryKey;
							if (created && !col.Identity && !col.Nullable)
							{
								var defaultValue = GetSqlDefaultValue(member);
								if (defaultValue != null && col.DefaultConstraint == null)
								{
									var dc = col.AddDefaultConstraint();
									dc.Text = defaultValue.ToString();
								}
							}
							OnChanged(col);
						}
					}
					count++;
				}
				catch (Exception e)
				{
					throw new Exception("Error while building member: " + type.Name + "." + member.Name, e);
				}
			}
			return count;
		}

		bool DataTypeEquals(DataType x, DataType y)
		{
			if (_DataTypeEquals(x, y)) //x.Equals(y)) in smo 10
				return true;
			if (x.SqlDataType != y.SqlDataType)
				return false;
			if (x.MaximumLength == 0)
				x.MaximumLength = y.MaximumLength;
			else if (y.MaximumLength == 0)
				y.MaximumLength = x.MaximumLength;

			if (x.NumericPrecision == 0)
				x.NumericPrecision = y.NumericPrecision;
			else if (y.NumericPrecision == 0)
				y.NumericPrecision = x.NumericPrecision;

			if (x.NumericScale == 0)
				x.NumericScale = y.NumericScale;
			else if (y.NumericScale == 0)
				y.NumericScale = x.NumericScale;

			if (_DataTypeEquals(x, y))//x.Equals(y)) in smo 10
				return true;
			return false;
		}

		static bool _DataTypeEquals(DataType x, DataType dt)
		{
			return x.Equals(dt);
			//if (dt == null)
			//{
			//  return false;
			//}
			//if (x.SqlDataType != dt.SqlDataType)
			//{
			//  return false;
			//}
			//if (x.Name != dt.Name)
			//{
			//  return false;
			//}
			//if (x.Schema != dt.Schema)
			//{
			//  return false;
			//}
			//if (x.MaximumLength != dt.MaximumLength)
			//{
			//  return false;
			//}
			//if (x.NumericPrecision != dt.NumericPrecision)
			//{
			//  return false;
			//}
			//if (x.NumericScale != dt.NumericScale)
			//{
			//  return false;
			//}
			//return true;

		}

		private void OnCreated(Table obj)
		{
			MarkedForCreate.Add(obj);
		}

		private void OnChanged(Table obj)
		{
			if (!MarkedForCreate.Contains(obj))
			{
				MarkedForAlter.Add(obj);
			}
		}
		private void OnCreated(Column obj)
		{
			var table = (Table)obj.Parent;
			Log.Inform("Created column {0}.{1}", table.Name, obj.Name);
			OnChanged(table);
			//if (MarkedForCreate.Contains(table) || MarkedForAlter.Contains(table))
			//  return;
			//MarkedForAlter.Add(table);
		}

		private void OnChanged(Column obj)
		{
			var table = (Table)obj.Parent;
			Log.Inform("Changed column {0}.{1}", table.Name, obj.Name);
			OnChanged(table);
			//if (obj.State == SqlSmoState.Existing)
			//{
			//  MarkedForAlter.Add(obj);
			//}
		}

		internal HashSet<Table> MarkedForCreate = new HashSet<Table>();
		internal HashSet<Table> MarkedForAlter = new HashSet<Table>();

		private void BuildPrimaryKey(MetaTable table)
		{
			var pkName = "PK_" + table.TableName;
			var pk = CurrentTable.Indexes[pkName];
			if (pk == null)
			{
				OnChanged(CurrentTable);
				pk = new Index(CurrentTable, pkName);
				pk.IndexKeyType = IndexKeyType.DriPrimaryKey;
				pk.IsUnique = true;
				pk.IsClustered = true;
				CurrentTable.Indexes.Add(pk);
				foreach (MetaDataMember member in table.RowType.IdentityMembers)
				{
					pk.IndexedColumns.Add(new IndexedColumn(pk, member.MappedName));
				}
			}
		}

		internal IEnumerable<string> BuildForeignKeys(MetaTable table)
		{
			foreach (var type in table.RowType.InheritanceTypes)
			{
				BuildForeignKeys(type);
			}
			return new string[0];
		}

		private static string[] BuildKey(IEnumerable<MetaDataMember> members)
		{
			return members.Select(t => t.MappedName).ToArray();
		}

		private void BuildForeignKeys(MetaType type)
		{
			string schemaName;
			string tableName;
			ParseFullTableName(type.Table.TableName, out schemaName, out tableName);
			foreach (var dm in type.DataMembers)
			{
				if (dm.IsDeclaredBy(type) && dm.IsAssociation)
				{
					var association = dm.Association;
					if (association.IsForeignKey)
					{
						var table = Database.Tables[tableName];
						if (table == null)
							throw new Exception("Cannot find table: " + tableName);
						//var sb = new StringBuilder();
						var thisKey = BuildKey(association.ThisKey);
						var otherKey = BuildKey(association.OtherKey);

						string otherSchemaName;
						string otherTableName;
						ParseFullTableName(association.OtherType.Table.TableName, out otherSchemaName, out otherTableName);



						var mappedName = dm.MappedName;
						if (mappedName == dm.Name)
						{
							mappedName = string.Format("FK_{0}_{1}", tableName, dm.Name);
						}
						var fkName = mappedName;
						var fk = table.ForeignKeys[fkName];
						if (fk == null)
						{
							OnChanged(table);
							fk = new ForeignKey(table, fkName);
							fk.ReferencedTable = otherTableName;
							if (thisKey.Length != otherKey.Length)
								throw new Exception("ThisKey and OtherKey length doesn't match");
							var i = 0;
							foreach (var thisKeyToken in thisKey)
							{
								var otherKeyToken = otherKey[i];
								var fkc = new ForeignKeyColumn(fk, thisKeyToken, otherKeyToken);
								fk.Columns.Add(fkc);
								i++;
							}
							//var cmd = "ALTER TABLE {0}" + Environment.NewLine + "  ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3}({4})";
							var otherMember = dm.Association.OtherMember;
							if (otherMember != null)
							{
								string deleteRule = otherMember.Association.DeleteRule;
								if (deleteRule != null)
								{
									fk.DeleteAction = ParseEnum<ForeignKeyAction>(deleteRule);
									//cmd = cmd + Environment.NewLine + "  ON DELETE " + deleteRule;
								}
							}
							table.ForeignKeys.Add(fk);
							Log.Inform("Added foreign key. {0} -> {1}", table.Name, fk.ReferencedTable);
							//sb.AppendFormat(cmd, new object[] { SqlIdentifier.QuoteCompoundIdentifier(tableName), SqlIdentifier.QuoteIdentifier(mappedName), SqlIdentifier.QuoteCompoundIdentifier(thisKey), SqlIdentifier.QuoteCompoundIdentifier(otherTable), SqlIdentifier.QuoteCompoundIdentifier(otherKey) });
						}
					}
				}
			}
		}

		T ParseEnum<T>(string s)
		{
			return (T)Enum.Parse(typeof(T), s, true);
		}

		private DataType ParseDbType(string dbType)
		{
			string dbTypeName = dbType.Replace("(", "").Replace(")", "");
			var member = typeof(DataType).GetMember(dbTypeName, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).FirstOrDefault();
			var dbTypePrms = new ArrayList();
			if (member == null)
			{
				var i1 = dbType.IndexOf("(");
				var i2 = dbType.IndexOf(")");
				if (i1 == -1)
				{
					dbTypeName = dbType.Split(' ').First();
				}
				else
				{
					dbTypeName = dbType.Substring(0, i1);
					var prms = dbType.Substring(i1 + 1, i2 - i1 - 1).Split(',');
					foreach (var prm in prms)
					{
						dbTypePrms.Add(int.Parse(prm));
					}
				}
				dbTypePrms.Reverse();//because scale is before precision. we get (precision, scale)
			}
			object res;
			member = typeof(DataType).GetMember(dbTypeName, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).First();
			try
			{
				if (member is PropertyInfo)
					res = ((PropertyInfo)member).GetValue(null, null);
				else
				{
					var method = (MethodInfo)member;
					if (dbTypeName == "Decimal" && dbTypePrms.Count == 1)
						dbTypePrms.Insert(0, 0);
					res = method.Invoke(null, dbTypePrms.ToArray());
				}
			}
			catch (Exception e)
			{
				throw new Exception("Error while trying to ParseDbType(" + dbType + ")", e);
			}
			return (DataType)res;
		}

		private DataType GetDbType(MetaDataMember mm)
		{
			string dbType = mm.DbType;
			if (dbType != null)
			{
				return ParseDbType(dbType);
			}
			StringBuilder builder = new StringBuilder();
			Type type = mm.Type;
			bool canBeNull = mm.CanBeNull;
			if (type.IsValueType && IsNullable(type))
			{
				type = type.GetGenericArguments()[0];
			}
			if (mm.IsVersion)
			{
				builder.Append("Timestamp");
			}
			else if (mm.IsPrimaryKey && mm.IsDbGenerated)
			{
				switch (Type.GetTypeCode(type))
				{
					case TypeCode.Object:
						if (type != typeof(Guid))
						{
							throw Common.CouldNotDetermineDbGeneratedSqlType(type);
						}
						builder.Append("UniqueIdentifier");
						break;

					case TypeCode.DBNull:
					case TypeCode.Boolean:
					case TypeCode.Char:
					case TypeCode.Single:
					case TypeCode.Double:
						break;

					case TypeCode.SByte:
					case TypeCode.Int16:
						builder.Append("SmallInt");
						break;

					case TypeCode.Byte:
						builder.Append("TinyInt");
						break;

					case TypeCode.UInt16:
					case TypeCode.Int32:
						builder.Append("Int");
						break;

					case TypeCode.UInt32:
					case TypeCode.Int64:
						builder.Append("BigInt");
						break;

					case TypeCode.UInt64:
					case TypeCode.Decimal:
						builder.Append("Decimal(20)");
						break;
				}
			}
			else
			{
				switch (Type.GetTypeCode(type))
				{
					case TypeCode.Object:
						if (type != typeof(Guid))
						{
							if (type == typeof(byte[]))
							{
								builder.Append("VarBinary(8000)");
							}
							else
							{
								if (type != typeof(char[]))
								{
									throw Common.CouldNotDetermineSqlType(type);
								}
								builder.Append("NVarChar(4000)");
							}
						}
						else
						{
							builder.Append("UniqueIdentifier");
						}
						break;

					case TypeCode.DBNull:
					case (TypeCode.DateTime | TypeCode.Object):
						break;

					case TypeCode.Boolean:
						builder.Append("Bit");
						break;

					case TypeCode.Char:
						builder.Append("NChar(1)");
						break;

					case TypeCode.SByte:
					case TypeCode.Int16:
						builder.Append("SmallInt");
						break;

					case TypeCode.Byte:
						builder.Append("TinyInt");
						break;

					case TypeCode.UInt16:
					case TypeCode.Int32:
						builder.Append("Int");
						break;

					case TypeCode.UInt32:
					case TypeCode.Int64:
						builder.Append("BigInt");
						break;

					case TypeCode.UInt64:
						builder.Append("Decimal(20)");
						break;

					case TypeCode.Single:
						builder.Append("Real");
						break;

					case TypeCode.Double:
						builder.Append("Float");
						break;

					case TypeCode.Decimal:
						builder.Append("Decimal(29, 4)");
						break;

					case TypeCode.DateTime:
						builder.Append("DateTime");
						break;

					case TypeCode.String:
						builder.Append("NVarChar(4000)");
						break;
				}
			}
			if (!canBeNull)
			{
				builder.Append(" NOT NULL");
			}
			if (mm.IsPrimaryKey && mm.IsDbGenerated)
			{
				if (type == typeof(Guid))
				{
					builder.Append(" DEFAULT NEWID()");
				}
				else
				{
					builder.Append(" IDENTITY");
				}
			}
			return ParseDbType(builder.ToString());
		}

		internal bool IsNullable(Type type)
		{
			return (type.IsGenericType && typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()));
		}

	}




}
