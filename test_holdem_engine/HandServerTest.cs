using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using holdem_engine;
using Action = holdem_engine.Action;
using PokerHandHistory;
using System.IO;
using System.Xml.Serialization;

namespace test_holdem_engine
{
    [TestFixture]
    public class HandServerTest
    {
        const string sampleHand = @"<PokerHand>
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

        HandServer server;
        PokerHand hand;
        CachedHand cache;
        [SetUp]
        public void SetUp()
        {
            server = new HandServer();
            using (StringReader reader = new StringReader(sampleHand))
            {
                XmlSerializer ser = new XmlSerializer(typeof(PokerHand));
                hand = (PokerHand)ser.Deserialize(reader);
            }
            Random r = new Random();
            cache = new CachedHand(6, r);
        }

        [Test]
        public void TestFullHand()
        {
            var history = server.Resume(hand, cache);
        }
    }
}
