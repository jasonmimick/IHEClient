namespace InterSystems.IHE.Client
{
	public abstract class IHERequest
	{

		public string HttpUserName { get; set; }
		public string HttpPassword { get; set; }
		public string WSSecurityUserName { get; set; }
		public string WSSecurityPassword { get; set; }
	}
}
