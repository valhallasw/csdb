using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using System.Xml.Linq;

namespace csdb.edmx
{
    class edmxDataMember : MetaDataMember
    {
        public String name;
        public String dbType;
        public Boolean nullable;
        public Boolean isDbGenerated;
        public Boolean isPrimaryKey;
        public MetaType parentType;
        public edmxPropertyInfo pi;
        public XElement c;

        public edmxDataMember(XElement _c, MetaType _parentType, List<String> pks) {
            c = _c;
            parentType = _parentType;
            name = c.Attribute("Name").Value;
            pi = new edmxPropertyInfo(c.Parent.Attribute("Name").Value + "." + c.Attribute("Name").Value);

            parseMember(pks);
        }

        public void parseMember(List<String> pks) {
            determineType();
            determineNullable();
            determinePk(pks);
            determineGenerated();
        }

        private void determineGenerated() {
           isDbGenerated = (c.Attribute("StoreGeneratedPattern") != null);
        }

        public void determinePk(List<String> pks) {
            if (pks.Contains(name)) {
                isPrimaryKey = true;
            }
        }

        public void determineNullable() {
            try {
                nullable = Convert.ToBoolean(c.Attribute("Nullable").Value);
            } catch (System.NullReferenceException) {
                nullable = false;
            }
        }

        public void determineType() {
            dbType = c.Attribute("Type").Value;

            if (c.Attribute("Maxlength") != null) {
                dbType += String.Format("({0})", c.Attribute("Maxlength"));
            } else if (dbType == "nvarchar" || dbType == "char") {
                dbType += "(2000)";
            }
        }


        public override MetaAssociation Association {
            get { throw new NotImplementedException(); }
        }

        public override AutoSync AutoSync {
            get { throw new NotImplementedException(); }
        }

        public override bool CanBeNull {
            get { return nullable; }
        }

        public override string DbType {
            get { return dbType; }
        }

        public override MetaType DeclaringType {
            get { throw new NotImplementedException(); }
        }

        public override MetaAccessor DeferredSourceAccessor {
            get { throw new NotImplementedException(); }
        }

        public override MetaAccessor DeferredValueAccessor {
            get { throw new NotImplementedException(); }
        }

        public override string Expression {
            get { return null; }
        }

        public override bool IsAssociation {
            get { return false; }
        }

        public override bool IsDbGenerated {
            get { return isDbGenerated; }
        }

        public override bool IsDeclaredBy(MetaType type) {
            return (type == parentType);
        }

        public override bool IsDeferred {
            get { throw new NotImplementedException(); }
        }

        public override bool IsDiscriminator {
            get { throw new NotImplementedException(); }
        }

        public override bool IsPersistent {
            get { return true; }
        }

        public override bool IsPrimaryKey {
            get { return isPrimaryKey; }
        }

        public override bool IsVersion {
            get { throw new NotImplementedException(); }
        }

        public override System.Reflection.MethodInfo LoadMethod {
            get { throw new NotImplementedException(); }
        }

        public override string MappedName {
            get { return name; }
        }

        public override System.Reflection.MemberInfo Member {
            get { return pi; }
        }

        public override MetaAccessor MemberAccessor {
            get { throw new NotImplementedException(); }
        }

        public override string Name {
            get { return name; }
        }

        public override int Ordinal {
            get { throw new NotImplementedException(); }
        }

        public override MetaAccessor StorageAccessor {
            get { throw new NotImplementedException(); }
        }

        public override System.Reflection.MemberInfo StorageMember {
            get { throw new NotImplementedException(); }
        }

        public override Type Type {
            get { throw new NotImplementedException(); }
        }

        public override UpdateCheck UpdateCheck {
            get { throw new NotImplementedException(); }
        }
    }
}
