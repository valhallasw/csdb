using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using System.Xml.Linq;

namespace csdb.edmx
{
    class edmxTable : MetaTable
    {
        public String tableName;
        public edmxType rowType;
        public List<String> pkColumns;
        public XElement e;
        public edmxType membersType;

        public edmxTable(XElement _e) {
            e = _e;
            tableName = e.Attribute("Name").Value;
            rowType = new edmxType(this.tableName) {
                it = new List<MetaType>(),
                im = new List<MetaDataMember>()
            };

            this.membersType = new edmxType(this.tableName) {
                dm = new List<MetaDataMember>(),
                table = this
            };
            this.rowType.it.Add(this.membersType);

            this.parseTable();
        }

        public void parseTable() {
           pkColumns = (from pk in e.Descendants()
                                where pk.Parent.Name.LocalName == "Key"
                                   && pk.Name.LocalName == "PropertyRef"
                                select pk.Attribute("Name").Value).ToList();

            foreach (var c in
                      (from c in e.Descendants() where c.Name.LocalName == "Property" select c)
                    ) {

                var dm = new edmxDataMember(c, this.membersType, pkColumns);
                if (dm.isPrimaryKey) rowType.im.Add(dm);

                membersType.dm.Add(dm);
            }
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
