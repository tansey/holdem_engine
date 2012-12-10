using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using holdem_engine;
using Action = holdem_engine.Action;
using PokerHandHistory;
using System.IO;
using System.Xml.Serialization;
using System.Linq;

namespace test_holdem_engine
{
    [TestFixture]
    public class HandServerTest
    {
        const string riverHand1 = @"<PokerHand>
    <Blinds Player=""TAP_OR_SNAP"" Type=""SmallBlind"" Amount=""5"" />
    <Blinds Player=""OsoWhisper"" Type=""BigBlind"" Amount=""10"" />
    <Rounds>
      <Actions Player=""Sevillano720"" Type=""Fold"" />
      <Actions Player=""LC1492"" Type=""Call"" Amount=""10"" />
      <Actions Player=""Dodenburg"" Type=""Fold"" />
      <Actions Player=""TeeJay5"" Type=""Raise"" Amount=""20"" />
      <Actions Player=""TAP_OR_SNAP"" Type=""Call"" Amount=""15"" />
      <Actions Player=""OsoWhisper"" Type=""Call"" Amount=""10"" />
      <Actions Player=""LC1492"" Type=""Call"" Amount=""10"" />
    </Rounds>
    <Rounds>
      <CommunityCards Rank=""Ten"" Suit=""Diamonds"" />
      <CommunityCards Rank=""Jack"" Suit=""Diamonds"" />
      <CommunityCards Rank=""Eight"" Suit=""Clubs"" />
      <Actions Player=""TAP_OR_SNAP"" Type=""Bet"" Amount=""10"" />
      <Actions Player=""OsoWhisper"" Type=""Raise"" Amount=""20"" />
      <Actions Player=""LC1492"" Type=""Fold"" />
      <Actions Player=""TeeJay5"" Type=""Call"" Amount=""20"" />
      <Actions Player=""TAP_OR_SNAP"" Type=""Raise"" Amount=""20"" />
      <Actions Player=""OsoWhisper"" Type=""Raise"" Amount=""20"" />
      <Actions Player=""TeeJay5"" Type=""Fold"" />
      <Actions Player=""TAP_OR_SNAP"" Type=""Call"" Amount=""10"" />
    </Rounds>
    <Rounds>
      <CommunityCards Rank=""Five"" Suit=""Spades"" />
      <Actions Player=""TAP_OR_SNAP"" Type=""Check"" />
      <Actions Player=""OsoWhisper"" Type=""Bet"" Amount=""20"" />
      <Actions Player=""TAP_OR_SNAP"" Type=""Call"" Amount=""20"" />
    </Rounds>
    <Rounds>
      <CommunityCards Rank=""Nine"" Suit=""Clubs"" />
    </Rounds>
    <Context Site=""PartyPoker"" Currency=""USD"" ID=""2229799540"" Table=""Table  12551 "" TimeStamp=""2005-06-19T04:15:10"" Format=""CashGame"" Button=""1"" BigBlind=""10"" SmallBlind=""5"" BettingType=""FixedLimit"" PokerVariant=""TexasHoldEm"" />
    <Players Name=""TeeJay5"" Stack=""526"" Seat=""1"" />
    <Players Name=""TAP_OR_SNAP"" Stack=""301"" Seat=""2"" />
    <Players Name=""OsoWhisper"" Stack=""177.77"" Seat=""3"" />
    <Players Name=""Sevillano720"" Stack=""742"" Seat=""4"" />
    <Players Name=""Dodenburg"" Stack=""458.5"" Seat=""6"" />
    <Players Name=""LC1492"" Stack=""641"" Seat=""5"" />
    <Rake>2.00</Rake>
    <Hero>TeeJay5</Hero>
    </PokerHand>";

		const string riverHand2 = @"<PokerHand>
    <Blinds Player=""TAP_OR_SNAP"" Type=""SmallBlind"" Amount=""5"" />
    <Blinds Player=""OsoWhisper"" Type=""BigBlind"" Amount=""10"" />
    <Rounds>
      <Actions Player=""Sevillano720"" Type=""Fold"" />
      <Actions Player=""LC1492"" Type=""Call"" Amount=""10"" />
      <Actions Player=""Dodenburg"" Type=""Fold"" />
      <Actions Player=""TeeJay5"" Type=""Raise"" Amount=""20"" />
      <Actions Player=""TAP_OR_SNAP"" Type=""Call"" Amount=""15"" />
      <Actions Player=""OsoWhisper"" Type=""Call"" Amount=""10"" />
      <Actions Player=""LC1492"" Type=""Call"" Amount=""10"" />
    </Rounds>
    <Rounds>
      <CommunityCards Rank=""Ten"" Suit=""Diamonds"" />
      <CommunityCards Rank=""Jack"" Suit=""Diamonds"" />
      <CommunityCards Rank=""Eight"" Suit=""Clubs"" />
      <Actions Player=""TAP_OR_SNAP"" Type=""Bet"" Amount=""10"" />
      <Actions Player=""OsoWhisper"" Type=""Raise"" Amount=""20"" />
      <Actions Player=""LC1492"" Type=""Fold"" />
      <Actions Player=""TeeJay5"" Type=""Call"" Amount=""20"" />
      <Actions Player=""TAP_OR_SNAP"" Type=""Raise"" Amount=""20"" />
      <Actions Player=""OsoWhisper"" Type=""Raise"" Amount=""20"" />
      <Actions Player=""TeeJay5"" Type=""Fold"" />
      <Actions Player=""TAP_OR_SNAP"" Type=""Call"" Amount=""10"" />
    </Rounds>
    <Rounds>
      <CommunityCards Rank=""Five"" Suit=""Spades"" />
      <Actions Player=""TAP_OR_SNAP"" Type=""Check"" />
      <Actions Player=""OsoWhisper"" Type=""Bet"" Amount=""20"" />
      <Actions Player=""TAP_OR_SNAP"" Type=""Call"" Amount=""20"" />
    </Rounds>
    <Rounds>
      <CommunityCards Rank=""Nine"" Suit=""Clubs"" />
      <Actions Player=""TAP_OR_SNAP"" Type=""Check"" />
      <Actions Player=""OsoWhisper"" Type=""Bet"" Amount=""20"" />
    </Rounds>
    <Context Site=""PartyPoker"" Currency=""USD"" ID=""2229799540"" Table=""Table  12551 "" TimeStamp=""2005-06-19T04:15:10"" Format=""CashGame"" Button=""1"" BigBlind=""10"" SmallBlind=""5"" BettingType=""FixedLimit"" PokerVariant=""TexasHoldEm"" />
    <Players Name=""TeeJay5"" Stack=""526"" Seat=""1"" />
    <Players Name=""TAP_OR_SNAP"" Stack=""301"" Seat=""2"" />
    <Players Name=""OsoWhisper"" Stack=""177.77"" Seat=""3"" />
    <Players Name=""Sevillano720"" Stack=""742"" Seat=""4"" />
    <Players Name=""Dodenburg"" Stack=""458.5"" Seat=""6"" />
    <Players Name=""LC1492"" Stack=""641"" Seat=""5"" />
    <Rake>2.00</Rake>
    <Hero>TeeJay5</Hero>
    </PokerHand>";

		const string bbHand = @"<PokerHand>
    <Blinds Player=""TAP_OR_SNAP"" Type=""SmallBlind"" Amount=""5"" />
    <Context Site=""PartyPoker"" Currency=""USD"" ID=""2229799540"" Table=""Table  12551 "" TimeStamp=""2005-06-19T04:15:10"" Format=""CashGame"" Button=""1"" BigBlind=""10"" SmallBlind=""5"" BettingType=""FixedLimit"" PokerVariant=""TexasHoldEm"" />
    <Players Name=""TeeJay5"" Stack=""526"" Seat=""1"" />
    <Players Name=""TAP_OR_SNAP"" Stack=""301"" Seat=""2"" />
    <Players Name=""OsoWhisper"" Stack=""177.77"" Seat=""3"" />
    <Players Name=""Sevillano720"" Stack=""742"" Seat=""4"" />
    <Players Name=""Dodenburg"" Stack=""458.5"" Seat=""6"" />
    <Players Name=""LC1492"" Stack=""641"" Seat=""5"" />
    <Rake>2.00</Rake>
    <Hero>TeeJay5</Hero>
    </PokerHand>";

        HandServer server;
        PokerHand hand;
        CachedHand cache;
        [SetUp]
        public void SetUp()
        {
            server = new HandServer();
            Random r = new Random();
            cache = new CachedHand(6, r);
        }

		private PokerHand fromString(string s)
		{
			using (StringReader reader = new StringReader(s))
			{
				XmlSerializer ser = new XmlSerializer(typeof(PokerHand));
				return (PokerHand)ser.Deserialize(reader);
			}
		}

		[Test]
		public void TestBigBlindNext()
		{
			//Next: <Blinds Player=""OsoWhisper"" Type=""BigBlind"" Amount=""10"" />
			hand = fromString(bbHand);
			var history = server.Resume(hand, cache);
			var next = server.ValidNextActions;
			Assert.AreEqual(next.Count(), 1);
			var action = next.ElementAt(0);
			Assert.AreEqual(action.Name, "OsoWhisper");
			Assert.AreEqual(action.ActionType, Action.ActionTypes.PostBigBlind);
			Assert.AreEqual(action.Amount, 10);
		}

		[Test]
		public void TestRiver1()
		{
			hand = fromString(riverHand1);
			var history = server.Resume(hand, cache);
			var next = server.ValidNextActions;
			Assert.AreEqual(2, next.Count());
			
			var check = next.FirstOrDefault(a => a.ActionType == Action.ActionTypes.Check);
			Assert.NotNull(check);
			Assert.AreEqual("TAP_OR_SNAP", check.Name);
			Assert.AreEqual(0, check.Amount);
			Assert.AreEqual(false, check.AllIn);

			var bet = next.FirstOrDefault(a => a.ActionType == Action.ActionTypes.Bet);
			Assert.NotNull(bet);
			Assert.AreEqual("TAP_OR_SNAP", bet.Name);
			Assert.AreEqual(20, bet.Amount);
			Assert.AreEqual(false, bet.AllIn);
		}

        [Test]
        public void TestRiver2()
        {
			hand = fromString(riverHand2);
            var history = server.Resume(hand, cache);
			var next = server.ValidNextActions;
			Assert.AreEqual(3, next.Count());

			var fold = next.FirstOrDefault(a => a.ActionType == Action.ActionTypes.Fold);
			Assert.NotNull(fold);
			Assert.AreEqual("TAP_OR_SNAP", fold.Name);
			Assert.AreEqual(0, fold.Amount);
			Assert.AreEqual(false, fold.AllIn);

			var call = next.FirstOrDefault(a => a.ActionType == Action.ActionTypes.Call);
			Assert.NotNull(call);
			Assert.AreEqual("TAP_OR_SNAP", call.Name);
			Assert.AreEqual(20, call.Amount);
			Assert.AreEqual(false, call.AllIn);

			var raise = next.FirstOrDefault(a => a.ActionType == Action.ActionTypes.Raise);
			Assert.NotNull(raise);
			Assert.AreEqual("TAP_OR_SNAP", raise.Name);
			Assert.AreEqual(40, raise.Amount);
			Assert.AreEqual(false, raise.AllIn);
        }
    }
}
