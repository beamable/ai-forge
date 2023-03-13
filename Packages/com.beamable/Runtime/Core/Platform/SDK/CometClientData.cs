using System;
using System.Collections.Generic;

namespace Beamable.Platform.SDK
{
	[Serializable]
	public class CometClientData
	{
		public CometClientDataEntry[] clientDataList;
		private Dictionary<string, string> _clientData;
		public Dictionary<string, string> ClientData
		{
			get
			{
				if (_clientData != null)
					return _clientData;

				_clientData = new Dictionary<string, string>();
				foreach (var entry in clientDataList)
				{
					_clientData[entry.name] = entry.value;
				}
				return _clientData;
			}
		}

		public string this[string key]
		{
			get
			{
				string result;
				ClientData.TryGetValue(key, out result);
				return result;
			}
		}
	}

	[Serializable]
	public struct CometClientDataEntry
	{
		public string name;
		public string value;
	}
}
