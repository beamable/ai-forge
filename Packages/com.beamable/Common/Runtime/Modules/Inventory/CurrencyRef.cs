using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using UnityEngine;
#pragma warning disable CS0618

namespace Beamable.Common.Inventory
{
	/// <summary>
	/// This type defines a methodology for resolving a reference to a %Beamable %ContentObject.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#contentlink-vs-contentref">ContentLink vs ContentRef</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[System.Serializable]
	[Agnostic]
	public class CurrencyLink : ContentLink<CurrencyContent> { }

	/// <summary>
	/// This type defines a methodology for resolving a reference to a %Beamable %ContentObject.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#contentlink-vs-contentref">ContentLink vs ContentRef</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[System.Serializable]
	[Agnostic]
	public class CurrencyRef : CurrencyRef<CurrencyContent>
	{
		public CurrencyRef() { }

		public CurrencyRef(string id)
		{
			Id = id;
		}

		public static implicit operator string(CurrencyRef data) => data.GetId();
		public static implicit operator CurrencyRef(string data) => new CurrencyRef { Id = data };
	}

	/// <summary>
	/// This type defines a methodology for resolving a reference to a %Beamable %ContentObject.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#contentlink-vs-contentref">ContentLink vs ContentRef</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[System.Serializable]
	public class CurrencyRef<TContent> : ContentRef<TContent> where TContent : CurrencyContent, new()
	{

	}

	[System.Serializable]
	[Agnostic]
	public class CurrencyAmount
	{
		[Tooltip(ContentObject.TooltipAmount1)]
		public int amount;

		[Tooltip(ContentObject.TooltipCurrency1)]
		[MustReferenceContent]
		public CurrencyRef symbol;
	}
}
