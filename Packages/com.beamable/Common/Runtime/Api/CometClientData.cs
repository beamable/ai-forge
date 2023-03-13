using System;
using System.Collections.Generic;

namespace Beamable.Common.Api
{
	[Serializable]
	public class CometClientData
	{
		public CometClientDataEntry[] clientDataList;
		private Dictionary<string, string> _clientData;

		/// <summary>
		/// Produce a dictionary of client data from the serialized <see cref="clientDataList"/>
		/// </summary>
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

		/// <summary>
		/// Look up a client data property by key.
		/// This will try to get the given key from the <see cref="ClientData"/> dictionary.
		/// </summary>
		/// <param name="key">Some key</param>
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
		/// <summary>
		/// The unique key of a client data property
		/// </summary>
		public string name;

		/// <summary>
		/// The value of a client data property
		/// </summary>
		public string value;
	}
}
