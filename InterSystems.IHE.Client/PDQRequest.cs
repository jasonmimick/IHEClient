
namespace InterSystems.IHE.Client
{
	public class PDQRequest : IHERequest
	{

		public PDQRequest() 
		{
		}
		public string FirstName { get; set; }
		public string LastName 	{ get; set; }
		public override string ToString() 
		{
			return "FirstName="+this.FirstName+",LastName="+this.LastName;
		}
	}

}
