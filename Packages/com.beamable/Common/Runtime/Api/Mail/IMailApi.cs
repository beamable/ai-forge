namespace Beamable.Common.Api.Mail
{
	public interface IMailApi : ISupportsGet<MailQueryResponse>
	{
		/// <summary>
		/// Find mail given a <see cref="SearchMailRequest"/> argument.
		/// The request contains a set of <see cref="SearchMailRequestClause"/> that can be configured to search for
		/// specific types of mail.
		/// </summary>
		/// <param name="request">
		/// A <see cref="SearchMailRequest"/> to filter the player's mail with.
		/// </param>
		/// <returns>
		/// A <see cref="Promise{T}"/> containing a <see cref="SearchMailResponse"/>
		/// The response will include a set of <see cref="SearchMailResponseClause"/>s that correspond to
		/// each <see cref="SearchMailRequestClause"/> in the request.
		/// </returns>
		Promise<SearchMailResponse> SearchMail(SearchMailRequest request);

		/// <summary>
		/// Get the latest mail for a player
		/// </summary>
		/// <param name="category">The category of mail can be any string</param>
		/// <param name="startId">An offset can be used to page through the players new mail</param>
		/// <param name="limit">Limit how many messages can appear in the resulting <see cref="ListMailResponse.result"/> field.</param>
		/// <returns>A <see cref="Promise"/> containing a <see cref="ListMailResponse"/> with the player's latest mail.</returns>
		Promise<ListMailResponse> GetMail(string category, long startId = 0, long limit = 100);

		/// <summary>
		/// Must be sent from an admin user or a microservice.
		/// Send mail to one or many users.
		/// </summary>
		/// <param name="request">A <see cref="MailSendRequest"/></param>
		/// <returns>A <see cref="Promise{T}"/> representing the network request.</returns>
		Promise<EmptyResponse> SendMail(MailSendRequest request);

		/// <summary>
		/// Must be sent from an admin user or a microservice.
		/// Update a mailing after it has been sent.
		/// </summary>
		/// <param name="updates">A <see cref="MailUpdateRequest"/></param>
		/// <returns>A <see cref="Promise{T}"/> representing the network request.</returns>
		Promise<EmptyResponse> Update(MailUpdateRequest updates);
	}
}
