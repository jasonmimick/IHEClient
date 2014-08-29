using System;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Diagnostics;
using InterSystems.IHE.Client.IHE.XDSb;
namespace InterSystems.IHE.Client
{

	public class XDSbResponse {

		private XDSbResponse() {
		}

		public string RawResponse { get; private set; }
		public RegistryResponseType Response { get; private set; }

		public bool Successful {
			get {
				Trace.TraceInformation("XDSbResponse.Successful status="
									+this.Response.status);
				if ( this.Response == null ) { return false; }
				if ( this.Response.status.Contains("Success") ) {
					return true;
				}
				return false;
			}
		}
		public static XDSbResponse CreateFromStream(Stream stream)
		{
			var xdsb_response = new XDSbResponse();
			var serializer = new XmlSerializer( typeof( RegistryResponseType ),
						"urn:oasis:names:tc:ebxml-regrep:xsd:rs:3.0" );
			using ( stream ) {
				using (var reader = new StreamReader( stream ) ) {
					xdsb_response.RawResponse = reader.ReadToEnd();
				}
				Trace.TraceInformation( xdsb_response.RawResponse );
				var xe = XElement.Parse(xdsb_response.RawResponse);
                var content = xe.Elements().First().Elements().First();
				xdsb_response.Response = (RegistryResponseType)
			    				serializer.Deserialize( content.CreateReader() );
			}		
			return xdsb_response;
		}
		public override string ToString()
		{
			return this.RawResponse;
		}
	}
}
