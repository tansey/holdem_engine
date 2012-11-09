using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using holdem_engine;
using FastPokerAction = holdem_engine.Action;
using Action = holdem_engine.Action;
namespace test_holdem_engine
{
    [TestFixture]
    public class BetManagerTest
    {
        private BetManager betMan;
        private Dictionary<string, double> namesToChips;

        [SetUp]
        public void SetUp()
        {
            namesToChips = new Dictionary<string, double>();
            namesToChips["Player0"] = 200;
            namesToChips["Player1"] = 200;
            namesToChips["Player2"] = 200;
            namesToChips["Player3"] = 200;
            namesToChips["Player4"] = 200;
        }

        [Test]
        public void TestBlinds()
        {
            double[] blinds = new double[]{1,2};
            betMan = new BetManager(namesToChips, BettingStructure.NoLimit, blinds, 0);

            FastPokerAction[] actions = new FastPokerAction[] {
                new FastPokerAction("Player3", FastPokerAction.ActionTypes.PostSmallBlind, 25),
                new FastPokerAction("Player4", FastPokerAction.ActionTypes.PostBigBlind, 62),
                new FastPokerAction("Player0", FastPokerAction.ActionTypes.Raise, 1),
            };

            FastPokerAction action = betMan.GetValidatedAction(actions[0]);
            Assert.AreEqual(1, action.Amount);
            Assert.AreEqual(FastPokerAction.ActionTypes.PostSmallBlind, action.ActionType);
            
            betMan.Commit(action);
            Assert.IsFalse(betMan.RoundOver);

            action = betMan.GetValidatedAction(actions[1]);
            Assert.AreEqual(2, action.Amount);
            Assert.AreEqual(FastPokerAction.ActionTypes.PostBigBlind, action.ActionType);

            betMan.Commit(action);
            Assert.IsFalse(betMan.RoundOver);

            action = betMan.GetValidatedAction(actions[2]);
            Assert.AreEqual(4, action.Amount);
            Assert.AreEqual(Action.ActionTypes.Raise, action.ActionType);

            betMan.Commit(action);
            Assert.IsFalse(betMan.RoundOver);
        }

        [Test]
        public void TestAntes()
        {
            double[] blinds = new double[] { 2, 4 };
            betMan = new BetManager(namesToChips, BettingStructure.NoLimit, blinds, 1);

            Action[] actions = new Action[] {
                new Action("Player3", Action.ActionTypes.PostAnte, 25),//should be 1
                new Action("Player4", Action.ActionTypes.PostAnte, 0.5),// should be 1
                new Action("Player0", Action.ActionTypes.PostAnte, 2),//should be PostAnte and 1
            };

            Action action = betMan.GetValidatedAction(actions[0]);
            Assert.AreEqual(1, action.Amount);
            Assert.AreEqual(Action.ActionTypes.PostAnte, action.ActionType);

            betMan.Commit(action);
            Assert.IsFalse(betMan.RoundOver);

            action = betMan.GetValidatedAction(actions[1]);
            Assert.AreEqual(1, action.Amount);
            Assert.AreEqual(Action.ActionTypes.PostAnte, action.ActionType);

            betMan.Commit(action);
            Assert.IsFalse(betMan.RoundOver);

            action = betMan.GetValidatedAction(actions[2]);
            Assert.AreEqual(1, action.Amount);
            Assert.AreEqual(Action.ActionTypes.PostAnte, action.ActionType);

            betMan.Commit(action);
            Assert.IsFalse(betMan.RoundOver);
        }

        [Test]
        public void TestNoLimitRaising()
        {
            double[] blinds = new double[] { 2, 4 };
            betMan = new BetManager(namesToChips, BettingStructure.NoLimit, blinds, 0);

            Action[] actions = new Action[] {
                new Action("Player0", Action.ActionTypes.PostSmallBlind, 2),
                new Action("Player1", Action.ActionTypes.PostBigBlind, 4),
                new Action("Player2", Action.ActionTypes.Bet, 6),//should be corrected to Raise 8
                new Action("Player3", Action.ActionTypes.Raise, 20),
                new Action("Player4", Action.ActionTypes.Raise, 0)//should be corrected to 32
            };

            betMan.Commit(actions[0]);
            betMan.Commit(actions[1]);

            Action action = betMan.GetValidatedAction(actions[2]);
            Assert.AreEqual(8, action.Amount);
            Assert.AreEqual(Action.ActionTypes.Raise, action.ActionType);
            betMan.Commit(action);
            Assert.IsFalse(betMan.RoundOver);

            action = betMan.GetValidatedAction(actions[3]);
            Assert.AreEqual(20, action.Amount);
            Assert.AreEqual(Action.ActionTypes.Raise, action.ActionType);
            betMan.Commit(action);
            Assert.IsFalse(betMan.RoundOver);

            action = betMan.GetValidatedAction(actions[4]);
            Assert.AreEqual(32, action.Amount);
            Assert.AreEqual(Action.ActionTypes.Raise, action.ActionType);
            betMan.Commit(action);
            Assert.IsFalse(betMan.RoundOver);
        }

        [Test]
        public void TestLimitBetting()
        {
        }

        [Test]
        public void TestPotLimitBetting()
        {
        }
    }
}
