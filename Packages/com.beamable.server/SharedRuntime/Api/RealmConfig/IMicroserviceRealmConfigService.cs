using Beamable.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Beamable.Server.Api.RealmConfig
{
	/// <summary>
	/// This type defines the %Microservice main entry point for the %Realm %Configuration feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Server.IBeamableServices script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface IMicroserviceRealmConfigService
	{
		Promise<RealmConfig> GetRealmConfigSettings();
	}

	public class RealmConfigNamespaceData : ReadOnlyDictionary<string, string>
	{
		public RealmConfigNamespaceData(IDictionary<string, string> dictionary) : base(dictionary)
		{

		}

		public string GetSetting(string key, string defaultValue = null) => this[key] ?? defaultValue;
	}

	/// <summary>
	/// This type defines the %Microservice main entry point for the %Realm %Configuration feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Server.IBeamableServices script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class RealmConfig : ReadOnlyDictionary<string, RealmConfigNamespaceData>
	{
		public static readonly RealmConfigNamespaceData EmptyNamespace =
		   new RealmConfigNamespaceData(new Dictionary<string, string>());

		public RealmConfig(IDictionary<string, RealmConfigNamespaceData> dictionary) : base(dictionary)
		{
		}

		public static RealmConfig From(Dictionary<string, Dictionary<string, string>> data)
		{
			var readonlyDict = data.ToDictionary(
			   keySelector: kvp => kvp.Key,
			   elementSelector: kvp => new RealmConfigNamespaceData(kvp.Value));
			return new RealmConfig(readonlyDict);
		}

		public RealmConfigNamespaceData GetNamespace(string nameSpace) => this[nameSpace] ?? EmptyNamespace;

		public string GetSetting(string nameSpace, string key, string defaultValue = null) =>
		   GetNamespace(nameSpace).GetSetting(key, defaultValue);
	}
}
