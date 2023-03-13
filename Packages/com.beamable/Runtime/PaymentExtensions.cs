// unset

namespace Beamable.Api.Payments
{
	public static class PaymentExtensions
	{
		public static string GetLocalizedText(this Price self, BeamContext ctx = null)
		{
			ctx = ctx ?? BeamContext.Default;
			return ctx.ServiceProvider.GetService<IBeamablePurchaser>().GetLocalizedPrice(self.symbol) ?? "";
		}
	}
}
