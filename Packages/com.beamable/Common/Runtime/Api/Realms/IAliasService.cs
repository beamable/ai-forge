using Beamable.Common.Content;
using System;

namespace Beamable.Common.Api.Realms
{
	public interface IAliasService
	{
		/// <summary>
		/// Given a string that could represent an Alias or a Cid, this method will analyze the string,
		/// and resolve it through Beamable to fetch other data.
		/// <para>
		/// If the input is a CID, then the result structure will only include the cid, since that is all that is required to use Beamable.
		/// If the input is an ALIAS, then the result structure will include the given alias, and the customer CID.
		/// </para>
		/// </summary>
		/// <param name="cidOrAlias">A string that is either a CID or an ALIAS</param>
		/// <returns>A structure that has an optional Alias and an optional Cid</returns>
		Promise<AliasResolve> Resolve(string cidOrAlias);
	}

	public class AliasResolve
	{
		public OptionalString Alias = new OptionalString();
		public OptionalString Cid = new OptionalString();
	}

	public class AliasService : IAliasService
	{
		private readonly IBeamableRequester _httpRequester;

		public AliasService(IBeamableRequester httpRequester)
		{
			_httpRequester = httpRequester;
		}

		public async Promise<AliasResolve> Resolve(string cidOrAlias)
		{
			if (AliasHelper.IsCid(cidOrAlias))
			{
				return new AliasResolve { Alias = new OptionalString(), Cid = new OptionalString(cidOrAlias) };
			}

			var resolve = await MapAliasToCid(cidOrAlias);

			if (!resolve.available) // the resolve notion from the server is backwards as of Feb 25th. "available=true" means that the alias has been taken by a customer.
			{
				throw new AliasDoesNotExistException(cidOrAlias);
			}

			return new AliasResolve
			{
				Alias = new OptionalString(resolve.alias),
				Cid = new OptionalString(resolve.cid.ToString())
			};
		}

		async Promise<AliasResolveResponse> MapAliasToCid(string alias)
		{
			AliasHelper.ValidateAlias(alias);

			var url = $"/basic/realms/customer/alias/available?alias={alias}";
			var res = await _httpRequester.Request<AliasResolveResponse>(Method.GET, url);
			return res;
		}

		[Serializable]
		public class AliasResolveResponse
		{
			public string alias;
			public bool available;
			public long cid;
		}

		public class AliasDoesNotExistException : Exception
		{
			public AliasDoesNotExistException(string alias) : base($"Alias does not exist. alias=[{alias}]")
			{

			}
		}
	}



}
