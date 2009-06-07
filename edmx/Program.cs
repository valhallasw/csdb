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
            var log = new CodeRun.Util.MSBuildLogger(Console.Out);
            log.WriteLine("Parsing model");
            edmxModel model = new edmxModel(@"C:\Documents and Settings\valhallasw\Mijn documenten\Visual Studio 2008\Projects\ConsoleApplication1\Model1.edmx");
            
            var ms2 = new MetaSynchronizer(model, @"Data Source=ECHIDNA\SQLEXPRESS;Initial Catalog=dbmkii;Integrated Security=True;Pooling=False");
            ms2.Log = log;

            ms2.CreateOrUpdateDatabase(true, true);

            Console.ReadLine();


        }
    }
}
