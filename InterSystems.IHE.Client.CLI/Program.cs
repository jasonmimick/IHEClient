using System;

using InterSystems.IHE.Client;
using System.Text;
using System.Collections.Generic;
using System.Dynamic;

namespace InterSystems.IHE.Client.CLI
{

	class MainClass
	{
		public static String Version = "0.1";

		public static void Main (string[] args)
		{

			var main = new MainClass ();
			main.parseArgs (args);
			if ( main.operation.Equals(op_help ) ) {
				main.Usage ();
				return;
			}
			if ( main.operation.Equals( op_pdq ) ){
				main.PDQ();
				return;
			}
			if (main.operation.Equals (op_xdsb)) {
				main.XDSb ();
				return;
			}
			
		}

		private string TryGetArgumentValue(string key)
		{
			string value;
			this.arguments.TryGetValue (key, out value);
			return value;
		}
		private void XDSb() 
		{
			var xdsbClient = new InterSystems.IHE.Client.XDSbClient ();
			xdsbClient.Verbose = this.arguments.ContainsKey (arg_verbose);
			xdsbClient.ExchangeEndpoint = this.TryGetArgumentValue (arg_endpoint);
			var xdsbRequest = new InterSystems.IHE.Client.XDSbRequest ();
			xdsbRequest.options = XDSbRequest.getDefaultOptions ();
			xdsbRequest.HttpUserName= this.TryGetArgumentValue(arg_httpusername);
			xdsbRequest.HttpPassword= this.TryGetArgumentValue(arg_httppassword);
			xdsbRequest.WSSecurityUserName= this.TryGetArgumentValue(arg_wssecurityusername);
			xdsbRequest.WSSecurityPassword= this.TryGetArgumentValue(arg_wssecuritypassword);
			var path = this.TryGetArgumentValue (arg_document);
			xdsbRequest.data = System.IO.File.ReadAllBytes (path);
			xdsbRequest.patientId = this.TryGetArgumentValue (arg_patientid);
			xdsbRequest.idSource = this.TryGetArgumentValue (arg_sourceid);
			xdsbRequest.creationDateTime = DateTime.Now;
			if ( path.EndsWith(".pdf" ) ) {
				xdsbRequest.mimeType = MimeType.PDF;
			}
			if (path.EndsWith (".xml")) {
				xdsbRequest.mimeType = MimeType.XML;
			}
			var xdsbResponse = xdsbClient.ProvideAndRegisterDocument (xdsbRequest);
			Console.WriteLine(xdsbResponse.RawResponse);
		}

		private void PDQ() 
		{
			var pdqClient = new InterSystems.IHE.Client.PDQClient ();
			pdqClient.Verbose = this.arguments.ContainsKey(arg_verbose);
			pdqClient.ExchangeEndpoint = this.arguments [arg_endpoint];
			var query = parseQuery (this.arguments [arg_query]);
			Console.WriteLine (string.Join(";",query));
			var pdqReq = new PDQRequest () { 
				FirstName = query ["FirstName"], 
				LastName = query ["LastName"]
			};

			pdqReq.HttpUserName= this.TryGetArgumentValue(arg_httpusername);
			pdqReq.HttpPassword= this.TryGetArgumentValue(arg_httppassword);
			pdqReq.WSSecurityUserName= this.TryGetArgumentValue(arg_wssecurityusername);
			pdqReq.WSSecurityPassword= this.TryGetArgumentValue(arg_wssecuritypassword);


			var results = pdqClient.SearchExchange (pdqReq);
			Console.WriteLine(string.Join(",",results));
		}
		private IDictionary<string,string> parseQuery( string q )
		{
			//var obj = new ExpandoObject ();
			var dict = new Dictionary<string,string> ();
			var parts = q.Split (',');
			dict.Add ("LastName", "");dict.Add ("FirstName", "");
			foreach (var part in parts) {
				var pp = part.Split ('=');
				var key = pp [0];
				var value = pp [1];
				if (dict.ContainsKey (key)) {
					dict.Remove (key);
				}
				dict.Add (key, value);
				//((IDictionary<string,object>)obj).Add (key, value);
				//obj.key = value;
			}
			//var oo = new object ();
			//oo.GetType ().GetProperty ("foo").SetValue (obj, value);
			return dict;
		}
		private Dictionary<string,string> arguments;

		private string operation;
		private static string op_help = "help";
		private static string op_pdq = "pdq";
		private static string op_xdsb = "xdsb";

		private List<String> valid_operations = new List<String> { op_help, op_pdq, op_xdsb };

		private void parseArgs(string[] args)
		{
			if (args.Length == 0) {
				return;
			}
			this.operation = args [0].ToLowerInvariant();

			if (!this.valid_operations.Contains (this.operation)) {
				throw new Exception ("Invalid operation=" + this.operation + " should be " +
				string.Join (",", this.valid_operations));
			}
			this.arguments = new Dictionary<string, string> ();
			for (var i=1; i<args.Length; i=i+2) {
				var key = args [i];
				var normalized_key = key.ToLowerInvariant ();
				if (normalized_key.Substring (0, 2) == "--") {
					normalized_key = normalized_key.Substring (2);
				}
				string value;
				if ( normalized_key.Equals( arg_verbose ) ) {
					value = "true";
					i = i - 1;
				} else {
					value = args[i+1];
				}
				this.arguments.Add (normalized_key, value);
			}
		}

		private static string arg_verbose = "verbose";
		private static string arg_query = "query";
		private static string arg_document = "document";
		private static string arg_endpoint = "endpoint";
		private static string arg_patientid = "patientid";
		private static string arg_sourceid = "sourceid";
		private static string arg_httpusername = "httpusername";
		private static string arg_httppassword = "httppassword";
		private static string arg_wssecurityusername = "wssecurityusername";
		private static string arg_wssecuritypassword = "wssecuritypassword";
		private void Usage()
		{
			var sb = new StringBuilder ();
			sb.Append ("InterSystems.IHE.Client.CLI ");
			sb.AppendLine ("Version: " + Version);
			sb.Append ("usage: InterSystems.IHE.Client.CLI.exe [PDQ|XDSb] ");
			sb.Append ("--Endpoint <URL for Exchange EndPoint> ");
			sb.Append ("--HttpUserName <http request username> ");
			sb.Append ("--HttpPassword <http request password> ");
			sb.Append ("--WSSecurityUserName <soap ws-security username> ");
			sb.Append ("--WSSecurityPassword <soap ws-security password> ");
			sb.Append ("[--Query 'LastName=Smith,FirstName=John'] ");
			sb.Append ("[--Document <Path to document to upload>] ");
			sb.Append ("[--PatientId <patientId> ]");
			sb.AppendLine ("[--SourceId <sourceId ]");
			sb.AppendLine ("Examples:");
			var ex = @"Running a PDQ query to find all people with last name 'Smith':
		
InterSystems.IHE.Client.CLI.exe PDQ --endpoint http://exchange.healthshare.us:57772/csp/healthshare/hsregistry/HS.IHE.PDQv3.Supplier.Services.CLS --httpusername _system --httppassword SYS --wssecurityusername HS_Services --wssecuritypassword HS_Services --query 'LastName=Smith'
			
Registering a pdf document to a patient:

InterSystems.IHE.Client.CLI.exe XDSb --endpoint http://exchange.healthshare.us:57772/csp/healthshare/hsrepository/HS.IHE.XDSb.Repository.Services.CLS --httpusername _system --httppassword SYS --wssecurityusername HS_Services --wssecuritypassword HS_Services --patientid 100000073 --sourceid 1.3.6.1.4.1.21367.2010.1.2.300 --document /Users/jmimick/Documents/lhs.pdf
			";
			sb.AppendLine (ex);
			Console.WriteLine (sb.ToString ());
		}
	}
}
