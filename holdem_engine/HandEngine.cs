using System;
using System.Collections.Generic;
using System.Text;
using HoldemHand;

namespace holdem_engine
{
    /// <summary>
    /// A class to handle the playing of a hand of poker.
    /// 
    /// Author: Wesley Tansey
    /// </summary>
    public class HandEngine 
    {
        #region Member Variables
        private Seat[] seats;
        private BetManager betManager;
        private PotManager potManager;
        private HandHistory history;
        private CircularList<int> playerIndices;
        private int buttonIdx;
        private int utgIdx;
        private int bbIdx;
        private CachedHand cache;
        #endregion

        #region Constructors
        public HandEngine()
        {
        }
        #endregion

        public void PlayHand(HandHistory handHistory, CachedHand cachedHand)
        {
            cache = cachedHand;
            PlayHand(handHistory);
        }

        public void PlayHand(HandHistory handHistory)
        {
            #region Hand Setup
            seats = handHistory.Players;
            handHistory.HoleCards = new ulong[seats.Length];
            handHistory.DealtCards = 0UL;
            handHistory.Flop = 0UL;
            handHistory.Turn = 0UL;
            handHistory.River = 0UL;

            //Setup the hand history
            this.history = handHistory;


            //Create a new map from player names to player chips for the BetManager
            Dictionary<string, double> namesToChips = new Dictionary<string, double>();

            //Create a new list of players for the PlayerManager
            playerIndices = new CircularList<int>();
            playerIndices.Loop = true;

            //Create a map of player names to their seat numbers
            //seatNumbers = new Dictionary<string, int>();

            for (int i = 0; i < seats.Length; i++)
            {
                namesToChips[seats[i].Name] = seats[i].Chips;
                if (seats[i].SeatNumber == history.Button)
                {
                    buttonIdx = i;
                    utgIdx = (i + 1) % seats.Length;
                }
            }
            for (int i = (buttonIdx + 1) % seats.Length; playerIndices.Count < seats.Length;)
            {
                playerIndices.Add(i);
                i = (i + 1) % seats.Length;
            }


            betManager = new BetManager(namesToChips, history.BettingStructure, history.AllBlinds, history.Ante);
            potManager = new PotManager(seats);
            #endregion

            

            if (betManager.In > 1)
            {
                GetBlinds();
                DealHoleCards();
            }
            
            history.CurrentRound = Round.Preflop;

            if (betManager.CanStillBet > 1)
            {
                GetBets(history.PreflopActions);
            }
            if (betManager.In <= 1)
            {
                payWinners();
                return;
            }

            DealFlop();
            history.CurrentRound = Round.Flop;

            if (betManager.CanStillBet > 1)
            {
                GetBets(history.FlopActions);
            }
            if (betManager.In <= 1)
            {
                payWinners();
                return;
            }
            
            DealTurn();
            history.CurrentRound = Round.Turn;

            if (betManager.CanStillBet > 1)
            {
                GetBets(history.TurnActions);
            }
            if (betManager.In <= 1)
            {
                payWinners();
                return;
            }

            DealRiver();
            history.CurrentRound = Round.River;

            if (betManager.CanStillBet > 1)
            {
                GetBets(history.RiverActions);
            }
            if (betManager.In <= 1)
            {
                payWinners();
                return;
            }

            payWinners();
            history.ShowDown = true;
            history.CurrentRound = Round.Over;
        }

        private void payWinners()
        {
            uint[] strengths = new uint[seats.Length];
            for (int i = 0; i < strengths.Length; i++)
                if(!history.Folded[i])
                    strengths[i] = HoldemHand.Hand.Evaluate(history.HoleCards[i] | history.Board, 7);
            
            List<Winner> winners = potManager.GetWinners(strengths);
            history.Winners = winners;
        }

        /// <summary>
        /// Gets the bets from all the players still in the hand.
        /// </summary>
        public void GetBets(List<Action> curRoundActions)
        {
            bool roundOver = false;
            
            int pIdx = GetFirstToAct(history.CurrentRound == Round.Preflop);
            
            //keep getting bets until the round is over
            while (!roundOver)
            {
                history.CurrentBetLevel = betManager.BetLevel;
                history.Pot = potManager.Total;
                history.Hero = pIdx;
                
                //get the next player's action
                Action.ActionTypes actionType; double amount;
                seats[pIdx].Brain.GetAction(history, out actionType, out amount);

                AddAction(pIdx, new Action(seats[pIdx].Name, actionType, amount), curRoundActions);

                roundOver = betManager.RoundOver;

				if(!roundOver)
                	pIdx = playerIndices.Next;
            }
            
        }

        private int GetFirstToAct(bool preflop)
        {
            int desired = ((preflop ? bbIdx : buttonIdx) + 1) % seats.Length;
            while (!playerIndices.Contains(desired)) { desired = (desired + 1) % seats.Length; }
            while(playerIndices.Next != desired){}

            return desired;
        }

        private void AddAction(int pIdx, Action action, List<Action> curRoundActions)
        {
            //if (action.ActionType == Action.ActionTypes.Raise && curRoundActions.Count == 0 && history.CurrentRound != Round.Preflop)
                //Console.WriteLine("");

            //Action unvalidatedAction = new Action(action.Name, action.ActionType, action.Amount, action.AllIn);
            action = betManager.GetValidatedAction(action);
            
            betManager.Commit(action);
            curRoundActions.Add(action);

            if (action.Amount > 0)
                seats[pIdx].Chips -= action.Amount;

            //update the pots
            potManager.AddAction(pIdx, action);

            if (action.ActionType == Action.ActionTypes.None)
                throw new Exception("Must have an action");

            //if the player either folded or went all-in, they can no longer
            //bet so remove them from the player pool
            if (action.ActionType == Action.ActionTypes.Fold)
            {
                playerIndices.Remove(pIdx);
                history.Folded[pIdx] = true;
            }
            else if (action.AllIn)
            {
                playerIndices.Remove(pIdx);
                history.AllIn[pIdx] = true;
            }
            
        }

        /// <summary>
        /// Forces players to post blinds before the hand can start.
        /// </summary>
        public void GetBlinds()
        {
            if (history.Ante > 0)
            {
                for (int i = utgIdx, count = 0; count < seats.Length; i = (i + 1) % seats.Length, count++)
                {
                    AddAction(i, new Action(seats[i].Name, Action.ActionTypes.PostAnte, history.Ante), history.PredealActions);
                }
            }

            // If there is no small blind, the big blind is the utg player, otherwise they're utg+1
            bbIdx = playerIndices.Next;
            if (history.SmallBlind > 0)
            {
                
                // If there was an ante and the small blind was put all-in, they can't post the small blind
                if (playerIndices.Contains(utgIdx))
                {
                    AddAction(bbIdx, 
                              new Action(seats[bbIdx].Name, Action.ActionTypes.PostSmallBlind, history.SmallBlind),
                              history.PredealActions);
                }
                bbIdx = playerIndices.Next;
            }
            if (history.BigBlind > 0)
            {
                if (playerIndices.Contains(bbIdx))
                {
                    AddAction(bbIdx, 
                              new Action(seats[bbIdx].Name, Action.ActionTypes.PostBigBlind, history.BigBlind), 
                              history.PredealActions);
                }
            }
        }

        /// <summary>
        /// Deals out all of the players' hole cards.
        /// </summary>
        public void DealHoleCards()
        {
            for (int i = 0; i < seats.Length; i++)
            {
                history.HoleCards[i] = cache != null ? cache.HoleCards[i] : Hand.RandomHand(history.DealtCards, 2);
                history.DealtCards = history.DealtCards | history.HoleCards[i];
            }
        }

        public void DealFlop()
        {
            history.Flop = cache != null ? cache.Flop : Hand.RandomHand(history.DealtCards, 3);
            history.DealtCards = history.DealtCards | history.Flop;
        }

        public void DealTurn()
        {
            history.Turn = cache != null ? cache.Turn : Hand.RandomHand(history.DealtCards, 1);
            history.DealtCards = history.DealtCards | history.Turn;
        }

        public void DealRiver()
        {
            history.River = cache != null ? cache.River : Hand.RandomHand(history.DealtCards, 1);
            history.DealtCards = history.DealtCards | history.River;
        }



    }
 
}
