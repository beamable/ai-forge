using Beamable.Common.Assistant;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Beamable.Common.Constants.MenuItems.Assets.Orders;

namespace Beamable.Editor.Assistant
{

	/// <summary>
	/// A quick and dirty way of mapping Domains, Sub-Domains and HintIds to specific blobs of text.
	/// TODO: Replace with in-editor localization system 
	/// </summary>
	[CreateAssetMenu(fileName = "BeamHintTextMap", menuName = "Beamable/Assistant/Hint Text Map", order = MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_2)]
	public class BeamHintTextMap : ScriptableObject
	{
		public SerializedStringToTextDictionary HintDomainToTitle = new SerializedStringToTextDictionary();

		public SerializedStringToTextDictionary HintIdToHintTitle = new SerializedStringToTextDictionary();
		public SerializedStringToTextDictionary HintIdToHintIntroText = new SerializedStringToTextDictionary();

		[ContextMenu("Add Domain Title")]
		public void AddDomainTitle() => HintDomainToTitle.Add("hintId", "title");

		public void AddDomainTitle(string id, string title) => HintDomainToTitle.Add(id, title);

		[ContextMenu("Add Hint Title")]
		public void AddHintTitle() => HintIdToHintTitle.Add("hintId", "title");

		public void AddHintTitle(string id, string title) => HintIdToHintTitle.Add(id, title);

		[ContextMenu("Add Hint Intro Text")]
		public void AddHintIntroText() => HintIdToHintIntroText.Add("hintId", "introText");

		public void AddHintIntroText(string id, string introText) => HintIdToHintIntroText.Add(id, introText);

		public bool TryGetDomainTitle(string domainSubString, out string title) => HintDomainToTitle.TryGetValue(domainSubString, out title);
		public bool TryGetHintTitle(BeamHintHeader header, out string title) => HintIdToHintTitle.TryGetValue(header.Id, out title);
		public bool TryGetHintIntroText(BeamHintHeader header, out string title) => HintIdToHintIntroText.TryGetValue(header.Id, out title);
	}

	[Serializable]
	public class SerializedStringToTextDictionary : Dictionary<string, string>, ISerializationCallbackReceiver
	{
		[SerializeField] private List<string> keys = new List<string>();

		[TextArea] [SerializeField] private List<string> values = new List<string>();

		// save the dictionary to lists
		public void OnBeforeSerialize()
		{
			keys.Clear();
			values.Clear();
			foreach (KeyValuePair<string, string> pair in this)
			{
				keys.Add(pair.Key);
				values.Add(pair.Value);
			}
		}

		// load dictionary from lists
		public void OnAfterDeserialize()
		{
			this.Clear();

			if (keys.Count != values.Count)
				throw new System.Exception(string.Format(
											   "there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

			for (int i = 0; i < keys.Count; i++)
				this.Add(keys[i], values[i]);
		}
	}
}
