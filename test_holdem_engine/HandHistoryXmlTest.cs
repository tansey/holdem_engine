using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using holdem_engine;
using Action = holdem_engine.Action;
using System.IO;
using System.Xml.Serialization;

namespace test_holdem_engine
{
	[TestFixture]
	public class HandHistoryXmlTest
	{
		HandEngine engine;
		HandHistory hist;

		[SetUp]
		public void SetUp()
		{
			engine = new HandEngine();

			#region Setup the actions
			Action[] jcloub = new Action[]{
				new Action("jcloub", Action.ActionTypes.Raise, 15),
				new Action("jcloub", Action.ActionTypes.Bet, 10),
				new Action("jcloub", Action.ActionTypes.Bet, 20),
				new Action("jcloub", Action.ActionTypes.Bet, 20),
			};

			Action[] makelgrus = new Action[]{
				new Action("MakelGrus", Action.ActionTypes.Call, 10),
				new Action("MakelGrus", Action.ActionTypes.Call, 10),
				new Action("MakelGrus", Action.ActionTypes.Call, 20),
				new Action("MakelGrus", Action.ActionTypes.Fold),
			};

			Action[] hustler_ed = new Action[]{
				new Action("Hustler_Ed", Action.ActionTypes.Call, 10),
				new Action("Hustler_Ed", Action.ActionTypes.Call, 10),
				new Action("Hustler_Ed", Action.ActionTypes.Call, 10),
				new Action("Hustler_Ed", Action.ActionTypes.Call, 20),
				new Action("Hustler_Ed", Action.ActionTypes.Fold),
			};

			Action[] shammybaby = new Action[]{
				new Action("Shammybaby", Action.ActionTypes.Fold),
			};

			Action[] marine0193 = new Action[]{
				new Action("marine0193", Action.ActionTypes.Fold),
			};

			Action[] teejayortj5 = new Action[]{
				new Action("TeeJayorTJ5", Action.ActionTypes.Fold),
			};
			#endregion

			#region Setup players
			SequencePlayer[] brains = new SequencePlayer[]{
				new SequencePlayer(jcloub),
				new SequencePlayer(makelgrus),
				new SequencePlayer(hustler_ed),
				new SequencePlayer(shammybaby),
				new SequencePlayer(marine0193),
				new SequencePlayer(teejayortj5)
			};

			var seqPlayers = new Seat[]{
				new Seat(1, "jcloub", 2044.5, brains[0]),
				new Seat(3, "MakelGrus", 498, brains[1]),
				new Seat(5, "Hustler_Ed", 470, brains[2]),
				new Seat(6, "Shammybaby", 551, brains[3]),
				new Seat(8, "marine0193", 538, brains[4]),
				new Seat(10, "TeeJayorTJ5", 484, brains[5])
			};
			#endregion

			var blinds = new double[] { 5, 10 };

			engine = new HandEngine();
			hist = new HandHistory(seqPlayers, 1, 10, blinds, 0, BettingStructure.Limit);
			engine.PlayHand(hist);
		}

		[Test]
		public void TestHistory()
		{
			PokerHandHistory.PokerHand xml = hist.ToXmlHand();

			PokerHandHistory.PokerHandXML hands = new PokerHandHistory.PokerHandXML(){
				Hands = new PokerHandHistory.PokerHand[] { xml }
			};

			StringBuilder sb = new StringBuilder();
			using(TextWriter writer = new StringWriter(sb))
			{
				XmlSerializer ser = new XmlSerializer(typeof(PokerHandHistory.PokerHandXML));
				ser.Serialize(writer, hands);
			}

			Console.WriteLine(sb.ToString());
		}
		const string HAND_XML = @"<?xml version=""1.0"" encoding=""utf-8""?>
		<PokerHandXML xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
		<Hands>
			<Blinds Player=""jcloub"" Type=""SmallBlind"" Amount=""5"" />
				<Blinds Player=""MakelGrus"" Type=""BigBlind"" Amount=""10"" />
				<HoleCards Rank=""Six"" Suit=""Spades"" />
				<HoleCards Rank=""Queen"" Suit=""Clubs"" />
				<Rounds>
				<Actions Player=""Hustler_Ed"" Type=""Call"" Amount=""10"" />
				<Actions Player=""Shammybaby"" Type=""Fold"" />
				<Actions Player=""marine0193"" Type=""Fold"" />
				<Actions Player=""TeeJayorTJ5"" Type=""Fold"" />
				<Actions Player=""jcloub"" Type=""Raise"" Amount=""15"" />
				<Actions Player=""MakelGrus"" Type=""Call"" Amount=""10"" />
				<Actions Player=""Hustler_Ed"" Type=""Call"" Amount=""10"" />
				</Rounds>
				<Rounds>
				<CommunityCards Rank=""Five"" Suit=""Diamonds"" />
				<CommunityCards Rank=""Seven"" Suit=""Hearts"" />
				<CommunityCards Rank=""Ten"" Suit=""Diamonds"" />
				<Actions Player=""jcloub"" Type=""Bet"" Amount=""10"" />
				<Actions Player=""MakelGrus"" Type=""Call"" Amount=""10"" />
				<Actions Player=""Hustler_Ed"" Type=""Call"" Amount=""10"" />
				</Rounds>
				<Rounds>
				<CommunityCards Rank=""Seven"" Suit=""Diamonds"" />
				<Actions Player=""jcloub"" Type=""Bet"" Amount=""20"" />
				<Actions Player=""MakelGrus"" Type=""Call"" Amount=""20"" />
				<Actions Player=""Hustler_Ed"" Type=""Call"" Amount=""20"" />
				</Rounds>
				<Rounds>
				<CommunityCards Rank=""Four"" Suit=""Hearts"" />
				<Actions Player=""jcloub"" Type=""Bet"" Amount=""20"" />
				<Actions Player=""MakelGrus"" Type=""Fold"" />
				<Actions Player=""Hustler_Ed"" Type=""Fold"" />
				</Rounds>
				<Context Site=""PartyPoker"" Currency=""USD"" ID=""1413811115"" Table=""Table  14795 "" TimeStamp=""2005-01-09T19:07:56"" Format=""CashGame"" Button=""10"" BigBlind=""10"" SmallBlind=""5"" BettingType=""FixedLimit"" PokerVariant=""TexasHoldEm"" />
				<Results Player=""jcloub"">
				<WonPots Amount=""168"" />
				</Results>
				<Players Name=""jcloub"" Stack=""2044.5"" Seat=""1"" />
				<Players Name=""MakelGrus"" Stack=""498"" Seat=""3"" />
				<Players Name=""Hustler_Ed"" Stack=""470"" Seat=""5"" />
				<Players Name=""Shammybaby"" Stack=""551"" Seat=""6"" />
				<Players Name=""marine0193"" Stack=""538"" Seat=""8"" />
				<Players Name=""TeeJayorTJ5"" Stack=""484"" Seat=""10"" />
				<Rake>2.00</Rake>
				<Hero>TeeJayorTJ5</Hero>
				</Hands>
				</PokerHandXML>";
	}
}

