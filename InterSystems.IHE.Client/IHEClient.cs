using System.Collections.Generic;
using System.Diagnostics;
namespace InterSystems.IHE.Client
{
	public abstract class IHEClient
	{
		public bool Verbose { get; set; }
		public string ExchangeEndpoint { get; set; }
		internal void validate(IHERequest ir)
		{
			var errors = new List<string>();
			if ( string.IsNullOrEmpty(this.ExchangeEndpoint) ) 
			{
				errors.Add("Client missing ExchangeEndpoint");
			}
			if ( string.IsNullOrEmpty(ir.HttpUserName) ) {
				errors.Add("Request missing HttpUserName");
			} 
			if ( string.IsNullOrEmpty(ir.HttpPassword) ) {
				errors.Add("Request missing HttpPassword");
			} 
			if ( string.IsNullOrEmpty(ir.WSSecurityUserName) ) {
				errors.Add("Request missing WSSecurityUserName");
			} 
			if ( string.IsNullOrEmpty(ir.WSSecurityPassword) ) {
				errors.Add("Request missing WSSecurityPassword");
			} 
			if ( errors.Count > 0 ) {
				throw new System.Exception( string.Join(",",errors) );
			}
		}
		internal void trace(object o) {
			if ( this.Verbose ) {
				Trace.TraceInformation(o.ToString());
			}
		}

	}
}
