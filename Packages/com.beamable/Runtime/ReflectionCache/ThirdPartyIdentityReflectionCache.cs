using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Common.Assistant;
using Beamable.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace Beamable.Reflection
{
#if BEAMABLE_DEVELOPER
	[CreateAssetMenu(fileName = "ThirdPartyIdentityReflectionCache",
	                 menuName = "Beamable/Reflection/Third Party Identities Cache",
	                 order = Constants.MenuItems.Assets.Orders.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
#endif
	public class ThirdPartyIdentityReflectionCache : ReflectionSystemObject
	{
		private Registry Cache;

		public override IReflectionSystem System => Cache;
		public override IReflectionTypeProvider TypeProvider => Cache;
		public override Type SystemType => typeof(Registry);

		public ThirdPartyIdentityReflectionCache()
		{
			Cache = new Registry();
		}

		public class Registry : IReflectionSystem
		{
			private static readonly BaseTypeOfInterest I_THIRD_PARTY_CLOUD_IDENTITY_INTERFACE = new BaseTypeOfInterest(
				typeof(IThirdPartyCloudIdentity));

			private IBeamHintGlobalStorage _hintGlobalStorage;

			public List<string> ThirdPartiesOptions { get; private set; } = new List<string>();

			public List<BaseTypeOfInterest> BaseTypesOfInterest =>
				new List<BaseTypeOfInterest> { I_THIRD_PARTY_CLOUD_IDENTITY_INTERFACE };

			public List<AttributeOfInterest> AttributesOfInterest => new List<AttributeOfInterest>();

			public void OnBaseTypeOfInterestFound(BaseTypeOfInterest baseType, IReadOnlyList<MemberInfo> cachedSubTypes)
			{
				ThirdPartiesOptions = GetIdentitiesOptions(cachedSubTypes);
			}

			private List<string> GetIdentitiesOptions(IReadOnlyList<MemberInfo> cachedSubTypes)
			{
				List<string> list = new List<string>();

				foreach (MemberInfo info in cachedSubTypes)
				{
					if (info is Type type &&
						FormatterServices.GetUninitializedObject(type) is IThirdPartyCloudIdentity identity)
					{
						list.Add(identity.UniqueName);
					}
				}

				return list;
			}
			public void SetStorage(IBeamHintGlobalStorage hintGlobalStorage) => _hintGlobalStorage = hintGlobalStorage;
			public void ClearCachedReflectionData() { }
			public void OnAttributeOfInterestFound(AttributeOfInterest attributeType,
												   IReadOnlyList<MemberAttribute> cachedMemberAttributes)
			{ }
			public void OnSetupForCacheGeneration() { }
			public void OnReflectionCacheBuilt(PerBaseTypeCache perBaseTypeCache,
											   PerAttributeCache perAttributeCache)
			{ }
		}
	}
}
