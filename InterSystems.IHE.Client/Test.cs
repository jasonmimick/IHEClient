using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Diagnostics;
using InterSystems.IHE.Client.IHE.XDSb;
using InterSystems.IHE.Client;

namespace InterSystems.IHE.Client.Test
{
    class MainTest
    {
        public static void Main()
        {

			var pdqClient = new PDQClient() 
				{ ExchangeEndpoint = PDQClient.TestingEndpoint };
			var xdsbClient = new XDSbClient()
				{ ExchangeEndpoint = XDSbClient.TestingEndpoint };

			//pdqClient.Verbose = true;
			xdsbClient.Verbose = true;
            var queries = new List<PDQRequest>();
            queries.Add(new PDQRequest() { FirstName = "Steven" });
            queries.Add(new PDQRequest() { LastName = "Smith" });
            queries.Add(new PDQRequest() { FirstName = "Reba", LastName = "Cook" });
			foreach(var q in queries) {
					q.HttpUserName = "_system";
					q.HttpPassword = "SYS";
					q.WSSecurityUserName = "HS_Services";
					q.WSSecurityPassword = "HS_Services" ;
			}
            //var results = new List<Object>();
            queries.ForEach(query => 
					Console.WriteLine(
					string.Join(",", query) + "\n" + 
					string.Join(",", 
					pdqClient.SearchExchange(query))));

            var results = pdqClient.SearchExchange(queries[1]);
			Console.WriteLine( string.Join( ",",results ) );

			var reba_query = new PDQRequest() { 
						FirstName = "Reba", 
						LastName = "Cook",
						HttpUserName = "_system",
						HttpPassword = "SYS",
						WSSecurityUserName = "HS_Services",
						WSSecurityPassword = "HS_Services" };

			var reba = pdqClient.SearchExchange( reba_query )[0];

			Console.WriteLine( reba );
			Console.WriteLine( string.Join(",", reba.Identifiers) );
			//XDSbClient.spike(reba.Identifiers[0].id,reba.Identifiers[0].source);
            var docString = @"<?xml version='1.0'?>
                <foo>
                <Hello>HELLO DUDE! HERE IS SOME IHE SHIZZZNITZZZ</Hello>
                </foo>";
            var docBytes = Encoding.UTF8.GetBytes(docString);
			var patientId = reba.Identifiers.First().ID;
			var sourceId = reba.Identifiers.First().Source;
            var xdsb_request = new XDSbRequest() { 
                mimeType = MimeType.XML, patientId = patientId, 
				idSource = sourceId, data = docBytes,
						HttpUserName = "_system",
						HttpPassword = "SYS",
						WSSecurityUserName = "HS_Services",
						WSSecurityPassword = "HS_Services" };
			xdsb_request.options = XDSbRequest.getDefaultOptions();
			var xdsb_response = xdsbClient.ProvideAndRegisterDocument( xdsb_request );
			Console.WriteLine(xdsb_response);
			Console.WriteLine("xdsb_response.Successful="+xdsb_response.Successful);
            //Console.ReadLine();
         }
    }
}
