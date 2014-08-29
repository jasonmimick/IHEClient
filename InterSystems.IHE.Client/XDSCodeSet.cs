using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;

namespace InterSystems.IHE.Client
{
    public class XDSCodeSet
    {
        // expose public methods to choose display values
        public static IEnumerable<string> ContentTypes()
        {
            return instance.xcodes.Elements("CodeType").Select(e => e.Attribute("name").Value);
        }
        public static IEnumerable<dynamic> Codes(string ContentType)
        {
            var type = instance.xcodes.Elements("CodeType").FirstOrDefault(e=>e.Attribute("name").Value == ContentType);
            var codes = type.Elements("Code").Select(e => new { code = e.Attribute("code").Value, display = e.Attribute("display").Value, codingScheme = e.Attribute("codingScheme").Value });
            return codes;
        }
    
		public static dynamic Code(string code, string codingScheme, string ContentType) {
			var codes = XDSCodeSet.Codes(ContentType);
			if ( codes == null ) {
				throw new Exception("No codes for ContentType=" + ContentType);
			}
			return codes.FirstOrDefault( c => 
				( c.code == code && c.codingScheme == codingScheme ) );
		}

        private XElement xcodes;
        private static readonly XDSCodeSet instance = new XDSCodeSet();
        private XDSCodeSet()
        {
            try
            {
                //var xmlStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("PDQClient.xds.codes.xml");
				//var ass = System.Reflection.Assembly.GetAssembly(typeof(XDSCodeSet));
				//Console.WriteLine(ass);
				//Console.WriteLine(
				//	string.Join(",",ass.GetManifestResourceNames())	);
                var xmlStream = System.Reflection.Assembly.GetAssembly(typeof(XDSCodeSet))
						.GetManifestResourceStream("InterSystems.IHE.Client.xds.codes.xml");
				
                this.xcodes = XElement.Load(xmlStream);
                //Console.WriteLine(this.xcodes);
				//Console.ReadLine();
				Trace.TraceInformation("Loaded XDSCodeSet");
            }
            catch (Exception e)
            {
				Console.ReadLine();
                Trace.TraceError(e.Message);
            }
        }
       
    }
}
