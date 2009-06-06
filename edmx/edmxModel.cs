using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace csdb.edmx
{
    class edmxModel : MetaModel
    {
        public List<MetaTable> tables;
        public String dbName;

        public edmxModel(String dbName) {
            this.dbName = dbName;
        }

        public override string DatabaseName {
            get { return dbName; }
        }

        public override Type ContextType {
            get { throw new NotImplementedException(); }
        }

        public override MetaFunction GetFunction(System.Reflection.MethodInfo method) {
            throw new NotImplementedException();
        }

        public override IEnumerable<MetaFunction> GetFunctions() {
            throw new NotImplementedException();
        }

        public override MetaType GetMetaType(Type type) {
            throw new NotImplementedException();
        }

        public override MetaTable GetTable(Type rowType) {
            throw new NotImplementedException();
        }

        public override IEnumerable<MetaTable> GetTables() {
            return tables;
        }

        public override MappingSource MappingSource {
            get { throw new NotImplementedException(); }
        }

        public override Type ProviderType {
            get { throw new NotImplementedException(); }
        }
    }
}
