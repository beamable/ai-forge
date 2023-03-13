using Beamable.Tournaments;
using Beamable.UI;
using NUnit.Framework;
using UnityEngine;

namespace Beamable.Tests.Modules.Tournaments.TournamentScoreUtilTests
{
	public class GetShortScoreTests
	{
		[Test]
		public void TestHexParse_Red()
		{
			var color = BeamableColorUtil.FromHex("#FF0000");
			Assert.AreEqual(new Color(1, 0, 0), color);
		}
		[Test]
		public void TestHexParse_RedNoOctothorp()
		{
			var color = BeamableColorUtil.FromHex("FF0000");
			Assert.AreEqual(new Color(1, 0, 0), color);
		}
		[Test]
		public void TestHexParse_Green()
		{
			var color = BeamableColorUtil.FromHex("#00FF00");
			Assert.AreEqual(new Color(0, 1, 0), color);
		}
		[Test]
		public void TestHexParse_Blue()
		{
			var color = BeamableColorUtil.FromHex("#0000FF");
			Assert.AreEqual(new Color(0, 0, 1), color);
		}
		[Test]
		public void TestHexParse_White()
		{
			var color = BeamableColorUtil.FromHex("#FFFFFF");
			Assert.AreEqual(new Color(1, 1, 1), color);
		}
		[Test]
		public void TestHexParse_Black()
		{
			var color = BeamableColorUtil.FromHex("#000000");
			Assert.AreEqual(new Color(0, 0, 0), color);
		}
		[Test]
		public void TestHexParse_RandoGreen()
		{
			var color = BeamableColorUtil.FromHex("#a4c862");
			Assert.AreEqual(new Color(164 / 255f, 200 / 255f, 98 / 255f), color);
		}

		[Test]
		public void SingleDigit()
		{
			var res = TournamentScoreUtil.GetShortScore(4);
			Assert.AreEqual("4", res);
		}

		[Test]
		public void DoubleDigit()
		{
			var res = TournamentScoreUtil.GetShortScore(42);
			Assert.AreEqual("42", res);
		}

		[Test]
		public void TripleDigit()
		{
			var res = TournamentScoreUtil.GetShortScore(463);
			Assert.AreEqual("463", res);
		}

		[Test]
		public void Thousand_Abbreviate_NoRounding()
		{
			var res = TournamentScoreUtil.GetShortScore(5000);
			Assert.AreEqual("5K", res);
		}

		[Test]
		public void Thousand_Abbreviate_EasyRounding()
		{
			var res = TournamentScoreUtil.GetShortScore(5500);
			Assert.AreEqual("5.5K", res);
		}

		[Test]
		public void Thousand_Abbreviate_ImperfectRounding()
		{
			var res = TournamentScoreUtil.GetShortScore(5501);
			Assert.AreEqual("5.5K", res);
		}

		[Test]
		public void Thousand_Abbreviate_ImperfectRounding_RoundsUp()
		{
			var res = TournamentScoreUtil.GetShortScore(5350);
			Assert.AreEqual("5.4K", res);
		}

		[Test]
		public void TensOfThousands()
		{
			var res = TournamentScoreUtil.GetShortScore(50000);
			Assert.AreEqual("50K", res);
		}

		[Test]
		public void TensOfThousands_EasyRounding()
		{
			var res = TournamentScoreUtil.GetShortScore(50001);
			Assert.AreEqual("50K", res);
		}

		[Test]
		public void TensOfThousands_NonTenthRounding()
		{
			var res = TournamentScoreUtil.GetShortScore(51300);
			Assert.AreEqual("51K", res);
		}

		[Test]
		public void TensOfThousands_NonTenthRounding_RoundsUp()
		{
			var res = TournamentScoreUtil.GetShortScore(51900);
			Assert.AreEqual("52K", res);
		}

		[Test]
		public void HundredsOfThousands()
		{
			var res = TournamentScoreUtil.GetShortScore(100000);
			Assert.AreEqual("100K", res);
		}

		[Test]
		public void HundredsOfThousands_Rounding()
		{
			var res = TournamentScoreUtil.GetShortScore(100100);
			Assert.AreEqual("100K", res);
		}

		[Test]
		public void HundredsOfThousands_ThirdCharacter()
		{
			var res = TournamentScoreUtil.GetShortScore(185000);
			Assert.AreEqual("185K", res);
		}

		[Test]
		public void HundredsOfThousands_ThirdCharacterRoundingUp()
		{
			var res = TournamentScoreUtil.GetShortScore(250900);
			Assert.AreEqual("251K", res);
		}


		[Test]
		public void HundredsOfThousands_RoundingUp_ToAMillion()
		{
			var res = TournamentScoreUtil.GetShortScore(999900);
			Assert.AreEqual("1M", res);
		}

		[Test]
		public void Millions()
		{
			var res = TournamentScoreUtil.GetShortScore(1000000);
			Assert.AreEqual("1M", res);
		}

		[Test]
		public void Millions_Abbreviate()
		{
			var res = TournamentScoreUtil.GetShortScore(1500000);
			Assert.AreEqual("1.5M", res);
		}

		[Test]
		public void Millions_Rounding()
		{
			var res = TournamentScoreUtil.GetShortScore(1560000);
			Assert.AreEqual("1.6M", res);
		}

		[Test]
		public void TensOfMillions()
		{
			var res = TournamentScoreUtil.GetShortScore(10000000);
			Assert.AreEqual("10M", res);
		}

		[Test]
		public void TensOfMillions2()
		{
			var res = TournamentScoreUtil.GetShortScore(15000000);
			Assert.AreEqual("15M", res);
		}

		[Test]
		public void TensOfMillions_Rounding()
		{
			var res = TournamentScoreUtil.GetShortScore(15600000);
			Assert.AreEqual("16M", res);
		}

		[Test]
		public void HundredsOfMillions()
		{
			var res = TournamentScoreUtil.GetShortScore(100000000);
			Assert.AreEqual("100M", res);
		}

		[Test]
		public void HundredsOfMillions_SecondCharacter()
		{
			var res = TournamentScoreUtil.GetShortScore(190000000);
			Assert.AreEqual("190M", res);
		}

		[Test]
		public void HundredsOfMillions_ThirdCharacter()
		{
			var res = TournamentScoreUtil.GetShortScore(149000000);
			Assert.AreEqual("149M", res);
		}

		[Test]
		public void HundredsOfMillions_ThirdCharacter_RoundsUp()
		{
			var res = TournamentScoreUtil.GetShortScore(148900000);
			Assert.AreEqual("149M", res);
		}


		[Test]
		public void HundredsOfMillions_RoundingUpToABillion()
		{
			var res = TournamentScoreUtil.GetShortScore(999900000);
			Assert.AreEqual("1B", res);
		}

		[Test]
		public void Billions()
		{
			var res = TournamentScoreUtil.GetShortScore(1000000000);
			Assert.AreEqual("1B", res);
		}

		[Test]
		public void Billions2()
		{
			var res = TournamentScoreUtil.GetShortScore(8000000000);
			Assert.AreEqual("8B", res);
		}

		[Test]
		public void Billions_Abbreviate()
		{
			var res = TournamentScoreUtil.GetShortScore(8400000000);
			Assert.AreEqual("8.4B", res);
		}

		[Test]
		public void Trillion()
		{
			var res = TournamentScoreUtil.GetShortScore(1000000000000);
			Assert.AreEqual("1T", res);
		}

		[Test]
		public void Trillion_ThirdCharacter()
		{
			var res = TournamentScoreUtil.GetShortScore(101000000000000);
			Assert.AreEqual("101T", res);
		}

		[Test]
		public void Trillion_UsesDecimal()
		{
			var res = TournamentScoreUtil.GetShortScore(1100000000000);
			Assert.AreEqual("1.1T", res);
		}

		[Test]
		public void Trillion_RoundsUpDecimal()
		{
			var res = TournamentScoreUtil.GetShortScore(1190000000000);
			Assert.AreEqual("1.2T", res);
		}

		[Test]
		public void Trillion_RoundUpFromBottom()
		{
			var res = TournamentScoreUtil.GetShortScore(100990000000000);
			Assert.AreEqual("101T", res);
		}

		[Test]
		public void Trillion_OutOfULongRange()
		{
			var res = TournamentScoreUtil.GetShortScore(7000000000000);
			Assert.AreEqual("7T", res);
		}

		[Test]
		public void Quadrillion()
		{
			var res = TournamentScoreUtil.GetShortScore(1000000000000000);
			Assert.AreEqual("1Q", res);
		}

		[Test]
		public void Quadrillion_ThreeDigits()
		{
			var res = TournamentScoreUtil.GetShortScore(573000000000000000);
			Assert.AreEqual("573Q", res);
		}

		[Test]
		public void Max()
		{
			var res = TournamentScoreUtil.GetShortScore(1000000000000000000);
			Assert.AreEqual("max", res);
		}

	}
}
