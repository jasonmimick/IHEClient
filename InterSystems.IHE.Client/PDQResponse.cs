using System.Collections.Generic;
using System.Linq;
namespace InterSystems.IHE.Client
{
	public class PDQIdentifier 
	{
		public PDQIdentifier() { }
		public string ID { get; set; }
		public string Source	{ get; set; }
		public override string ToString() 
		{
			return "ID="+ID+" Source="+Source;
		}
	}
	public class PDQResponse
	{

		internal PDQResponse() 
		{
			this.ids = new List<PDQIdentifier>();
		}
		internal List<PDQIdentifier> ids;

		public string FirstName { get; internal set; }
		public string LastName 	{ get; internal set; }
		public string DateOfBirth { get; internal set; }
		public string Address { get; internal set; }
		public IEnumerable<PDQIdentifier> Identifiers {
			get {
				return this.ids.AsEnumerable<PDQIdentifier>();
			}
		}

		public override string ToString() 
		{
			var x = new List<string>();
			x.Add(FirstName);
			x.Add(LastName);
			x.Add(DateOfBirth);
			x.Add(Address);
			ids.ForEach( id => x.Add(id.ToString()) );
			return string.Join(" ",x);	
		}
	}

}
