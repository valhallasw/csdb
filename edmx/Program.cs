using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Data.Linq.Mapping;

namespace csdb.edmx
{
    class Program
    {
        static void Main(string[] args) {

            csdb.Program.Options =  new CsdbOptions(args);
            XDocument edmxDoc = XDocument.Load(@"C:\Documents and Settings\valhallasw\Mijn documenten\Visual Studio 2008\Projects\ConsoleApplication1\Model1.edmx");

            String modelName = (from e in edmxDoc.Descendants() where e.Name.LocalName == "Schema" select e.Attribute("Namespace").Value).First().ToString().Split('.').First();
            edmxModel model = new edmxModel(modelName) { tables = new List<MetaTable>() };
            
            foreach (var e in
                       (from e in edmxDoc.Descendants()
                        where e.Parent != null
                           && e.Parent.Parent != null
                           && e.Parent.Parent.Name.LocalName == "StorageModels"
                           && e.Name.LocalName == "EntityType"
                        select e)
                     ) {
                Console.WriteLine(e.Attribute("Name").Value);

                edmxTable t = new edmxTable(e.Attribute("Name").Value) { 
                                  rowType = new edmxType(e.Attribute("Name").Value) {
                                    it = new List<MetaType>(),
                                    im = new List<MetaDataMember>()
                                    
                                  }
                };
                
                edmxType membersType = new edmxType(e.Attribute("Name").Value) {
                                            dm = new List<MetaDataMember>(),
                                            table = t
                                            
                };

                t.rowType.it.Add(membersType);

                List<String> pks = (from pk in e.Descendants()
                                                  where pk.Parent.Name.LocalName == "Key"
                                                     && pk.Name.LocalName == "PropertyRef"
                                                  select pk.Attribute("Name").Value).ToList();
                          

                foreach (var c in
                          (from c in e.Descendants() where c.Name.LocalName == "Property" select c)
                        ) {
                    Console.WriteLine(" - " + c.Attribute("Name").Value);
                    edmxDataMember dm = new edmxDataMember(c.Attribute("Name").Value);
                    dm.parentType = membersType;
                    dm.pi = new edmxPropertyInfo(e.Attribute("Name").Value + "." + c.Attribute("Name").Value);
                    
                    dm.dbType = c.Attribute("Type").Value;

                    if (c.Attribute("Maxlength") != null) {
                        dm.dbType += String.Format("({0})", c.Attribute("Maxlength"));
                    } else if (dm.dbType == "nvarchar" || dm.dbType == "char") {
                        dm.dbType += "(2000)";
                    }
                    try {
                        dm.nullable = Convert.ToBoolean(c.Attribute("Nullable").Value);
                    } catch (System.NullReferenceException) {
                        dm.nullable = false;
                    }

                    if (pks.Contains(dm.name)) {
                        dm.isPrimaryKey = true;
                        t.rowType.im.Add(dm);
                    }

                    dm.isDbGenerated = (c.Attribute("StoreGeneratedPattern") != null);



                    membersType.dm.Add(dm);
                }

                model.tables.Add(t);

            }

            
            var ms2 = new MetaSynchronizer(model, @"Data Source=ECHIDNA\SQLEXPRESS;Initial Catalog=dbmkii;Integrated Security=True;Pooling=False");
            ms2.Log = new CodeRun.Util.MSBuildLogger(Console.Out);

            ms2.CreateOrUpdateDatabase(true, true);

            Console.ReadLine();


        }
    }
}
