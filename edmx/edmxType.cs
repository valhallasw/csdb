using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace csdb.edmx
{
    class edmxType : MetaType
    {
        public List<MetaType> it;
        public List<MetaDataMember> dm;
        public List<MetaDataMember> im;
        public String name;
        public MetaTable table;

        public edmxType(String name) {
            this.name = name;
        }

        public override System.Collections.ObjectModel.ReadOnlyCollection<MetaAssociation> Associations {
            get { throw new NotImplementedException(); }
        }

        public override bool CanInstantiate {
            get { throw new NotImplementedException(); }
        }

        public override MetaDataMember DBGeneratedIdentityMember {
            get { throw new NotImplementedException(); }
        }

        public override System.Collections.ObjectModel.ReadOnlyCollection<MetaDataMember> DataMembers {
            get { return  new System.Collections.ObjectModel.ReadOnlyCollection<MetaDataMember>(dm); }
        }

        public override System.Collections.ObjectModel.ReadOnlyCollection<MetaType> DerivedTypes {
            get { throw new NotImplementedException(); }
        }

        public override MetaDataMember Discriminator {
            get { throw new NotImplementedException(); }
        }

        public override MetaDataMember GetDataMember(System.Reflection.MemberInfo member) {
            throw new NotImplementedException();
        }

        public override MetaType GetInheritanceType(Type type) {
            throw new NotImplementedException();
        }

        public override MetaType GetTypeForInheritanceCode(object code) {
            throw new NotImplementedException();
        }

        public override bool HasAnyLoadMethod {
            get { throw new NotImplementedException(); }
        }

        public override bool HasAnyValidateMethod {
            get { throw new NotImplementedException(); }
        }

        public override bool HasInheritance {
            get { throw new NotImplementedException(); }
        }

        public override bool HasInheritanceCode {
            get { throw new NotImplementedException(); }
        }

        public override bool HasUpdateCheck {
            get { throw new NotImplementedException(); }
        }

        public override System.Collections.ObjectModel.ReadOnlyCollection<MetaDataMember> IdentityMembers {
            get { return new System.Collections.ObjectModel.ReadOnlyCollection<MetaDataMember>(im); }
        }

        public override MetaType InheritanceBase {
            get { throw new NotImplementedException(); }
        }

        public override object InheritanceCode {
            get { throw new NotImplementedException(); }
        }

        public override MetaType InheritanceDefault {
            get { throw new NotImplementedException(); }
        }

        public override MetaType InheritanceRoot {
            get { throw new NotImplementedException(); }
        }

        public override System.Collections.ObjectModel.ReadOnlyCollection<MetaType> InheritanceTypes {
            get { return new System.Collections.ObjectModel.ReadOnlyCollection<MetaType>(it);
            }
        }

        public override bool IsEntity {
            get { throw new NotImplementedException(); }
        }

        public override bool IsInheritanceDefault {
            get { throw new NotImplementedException(); }
        }

        public override MetaModel Model {
            get { throw new NotImplementedException(); }
        }

        public override string Name {
            get { return (String)name; }
        }

        public override System.Reflection.MethodInfo OnLoadedMethod {
            get { throw new NotImplementedException(); }
        }

        public override System.Reflection.MethodInfo OnValidateMethod {
            get { throw new NotImplementedException(); }
        }

        public override System.Collections.ObjectModel.ReadOnlyCollection<MetaDataMember> PersistentDataMembers {
            get { throw new NotImplementedException(); }
        }

        public override MetaTable Table {
            get { return table; }
        }

        public override Type Type {
            get { throw new NotImplementedException(); }
        }

        public override MetaDataMember VersionMember {
            get { throw new NotImplementedException(); }
        }
    }
}
