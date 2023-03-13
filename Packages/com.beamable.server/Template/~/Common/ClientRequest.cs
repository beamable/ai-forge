namespace Core.Server.Common
{
	public struct ClientRequest
	{

		public object[] Payload;
	}

	public struct GatewayResponse
	{
		public int id;
		public int status;
		public ClientResponse body;
	}

	public struct ClientResponse
	{
		public string payload;
	}

}
