using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using System.Xml.Linq;

namespace csdb.edmx
{
    class edmxModel : MetaModel
    {
        public List<MetaTable> tables;
        public String modelName;
        public XDocument edmxDoc;

        public edmxModel() {
            this.tables = new List<MetaTable>();
        }

        public edmxModel(String fileName) {
            tables = new List<MetaTable>();
            edmxDoc = XDocument.Load(fileName);
            modelName = (from e in edmxDoc.Descendants() where e.Name.LocalName == "Schema" select e.Attribute("Namespace").Value).First().ToString().Split('.').First();

            parseModel();
        }

        public void parseModel() {
            foreach (XElement e in
                       (from e in edmxDoc.Descendants()
                        where e.Parent != null
                           && e.Parent.Parent != null
                           && e.Parent.Parent.Name.LocalName == "StorageModels"
                           && e.Name.LocalName == "EntityType"
                        select e)
                     ) {

                tables.Add(new edmxTable(e));

            }
        }

        public override string DatabaseName {
            get { return modelName; }
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
