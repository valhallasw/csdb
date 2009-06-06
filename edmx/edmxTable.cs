using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace csdb.edmx
{
    class edmxTable : MetaTable
    {
        public String tableName;
        public edmxType rowType;

        public edmxTable(String tableName) {
            this.tableName = tableName;
        }

        public override System.Reflection.MethodInfo DeleteMethod {
            get { throw new NotImplementedException(); }
        }

        public override System.Reflection.MethodInfo InsertMethod {
            get { throw new NotImplementedException(); }
        }

        public override MetaModel Model {
            get { throw new NotImplementedException(); }
        }

        public override MetaType RowType {
            get { return rowType; }
        }

        public override string TableName {
            get { return tableName; }
        }

        public override System.Reflection.MethodInfo UpdateMethod {
            get { throw new NotImplementedException(); }
        }
    }
}
