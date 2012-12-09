using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using holdem_engine;
using Action = holdem_engine.Action;

namespace test_holdem_engine
{
    [TestFixture]
    public class HandEngineTest
    {
        private Seat[] seqPlayers;
        private HandEngine engine;
        private double[] blinds;

        [SetUp]
        public void SetUp()
        {
            engine = new HandEngine();
            seqPlayers = new Seat[5];
            for (int i = 0; i < seqPlayers.Length; i++)
            {
                seqPlayers[i] = new Seat();
                seqPlayers[i].Chips = 200.0;
                seqPlayers[i].Name = "Seq" + i;
                seqPlayers[i].SeatNumber = i + 1;
            }
            blinds = new double[] { 1, 2 };
        }

        [Test]
        public void TestPlayHand()
        {
            //The hand will be 
            //Preflop:
            //Seq0 UTG, raises to $4
            //Seq4 is on BB and will reraise to $6
            //Seq0 flat calls
            //Flop:
            //Seq4 goes all-in for $194
            //Seq0 folds.

            Action[] actions0 = new Action[] {
                new Action("Seq0", Action.ActionTypes.Raise, 4),
                new Action("Seq0", Action.ActionTypes.Call),
                new Action("Seq0", Action.ActionTypes.Fold)
            };
            seqPlayers[0].Brain = new SequencePlayer(actions0);

            seqPlayers[1].Brain = new SequencePlayer();
            seqPlayers[2].Brain = new SequencePlayer();
            seqPlayers[3].Brain = new SequencePlayer();

            Action[] actions4 = new Action[] {
                new Action("Seq4", Action.ActionTypes.Raise, 4),
                new Action("Seq4", Action.ActionTypes.Raise, 194)
            };
            seqPlayers[4].Brain = new SequencePlayer(actions4);

            //seq2 is on _buttonIdx (seat 3), seq3 is small blind ($1), seq4 is big blind ($2), hand number is 42
            HandHistory results = new TournamentHandHistory(seqPlayers, 42, 3, blinds, 0, BettingStructure.NoLimit);
            engine.PlayHand(results);
            
            Console.WriteLine(results);

            Assert.AreEqual(194.0, seqPlayers[0].Chips);//raised to $4, called a $2 reraise and folded flop
            Assert.AreEqual(200.0, seqPlayers[1].Chips);//folded preflop
            Assert.AreEqual(200.0, seqPlayers[2].Chips);//folded preflop
            Assert.AreEqual(199.0, seqPlayers[3].Chips);//folded preflop, but paid $1 small blind
            Assert.AreEqual(207.0, seqPlayers[4].Chips);//won the hand, including $6 from seq0 and $1 small blind


        }
    }
}
